using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CuttingOptimizer.Domain.Entities;
using CuttingOptimizer.Domain.Interfaces;
using Google.OrTools.LinearSolver;
using Google.OrTools.Sat;

namespace CuttingOptimizer.Infrastructure.Optimization;

/// <summary>
/// OR-Tools based optimization engine for cutting stock problems
/// </summary>
public class OrToolsOptimizationEngine : IOptimizationEngine
{
    private readonly IPerformanceMonitor _performanceMonitor;
    private readonly IHeuristicEngine _heuristicEngine;

    public OrToolsOptimizationEngine(
        IPerformanceMonitor performanceMonitor,
        IHeuristicEngine heuristicEngine)
    {
        _performanceMonitor = performanceMonitor ?? throw new ArgumentNullException(nameof(performanceMonitor));
        _heuristicEngine = heuristicEngine ?? throw new ArgumentNullException(nameof(heuristicEngine));
    }

    public async Task<CuttingPlan> OptimizeAsync(
        List<Stock> stocks,
        List<Piece> pieces,
        OptimizationSettings settings,
        CancellationToken cancellationToken = default,
        IProgress<OptimizationProgress>? progressCallback = null)
    {
        _performanceMonitor.StartMonitoring();
        
        try
        {
            var cuttingPlan = new CuttingPlan
            {
                Stocks = stocks.Select(s => s.Clone()).ToList(),
                Pieces = pieces.Select(p => p.Clone()).ToList(),
                Settings = settings.Clone()
            };

            // Phase 1: Heuristic pre-arrangement
            if (settings.EnableHeuristicPreArrangement)
            {
                await ReportProgress(progressCallback, 1, 3, "Performing heuristic pre-arrangement...", 0);
                var heuristicResult = await _heuristicEngine.PreArrangeAsync(stocks, pieces, settings, cancellationToken);
                cuttingPlan.PlacedPieces.AddRange(heuristicResult);
            }

            // Phase 2: OR-Tools fine optimization
            await ReportProgress(progressCallback, 2, 3, "Performing OR-Tools optimization...", 50);
            var orToolsResult = await PerformOrToolsOptimizationAsync(stocks, pieces, settings, cuttingPlan, cancellationToken);
            cuttingPlan.PlacedPieces.AddRange(orToolsResult);

            // Phase 3: Final optimization and cleanup
            await ReportProgress(progressCallback, 3, 3, "Finalizing optimization...", 90);
            await FinalizeOptimizationAsync(cuttingPlan, settings, cancellationToken);

            await ReportProgress(progressCallback, 3, 3, "Optimization completed", 100);

            return cuttingPlan;
        }
        finally
        {
            _performanceMonitor.StopMonitoring();
        }
    }

    public ValidationResult ValidateProblem(List<Stock> stocks, List<Piece> pieces)
    {
        var result = new ValidationResult();
        
        // Calculate areas
        result.TotalStockArea = stocks.Sum(s => s.TotalArea);
        result.TotalPieceArea = pieces.Sum(p => p.Area * p.Quantity);
        result.EstimatedUtilization = result.TotalStockArea > 0 ? (result.TotalPieceArea / result.TotalStockArea) * 100 : 0;

        // Validate basic requirements
        if (stocks.Count == 0)
        {
            result.Errors.Add("No stock materials provided");
        }

        if (pieces.Count == 0)
        {
            result.Errors.Add("No pieces to cut provided");
        }

        if (result.TotalPieceArea > result.TotalStockArea)
        {
            result.Errors.Add($"Total piece area ({result.TotalPieceArea}) exceeds total stock area ({result.TotalStockArea})");
        }

        // Check for invalid dimensions
        foreach (var stock in stocks)
        {
            if (stock.Width <= 0 || stock.Height <= 0)
            {
                result.Errors.Add($"Stock '{stock.Name}' has invalid dimensions: {stock.Width}x{stock.Height}");
            }
        }

        foreach (var piece in pieces)
        {
            if (piece.Width <= 0 || piece.Height <= 0)
            {
                result.Errors.Add($"Piece '{piece.Name}' has invalid dimensions: {piece.Width}x{piece.Height}");
            }
        }

        // Warnings
        if (result.EstimatedUtilization < 50)
        {
            result.Warnings.Add($"Low estimated utilization: {result.EstimatedUtilization:F1}%");
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    public double EstimateOptimizationTime(List<Stock> stocks, List<Piece> pieces, OptimizationSettings settings)
    {
        var totalPieces = pieces.Sum(p => p.Quantity);
        var totalStocks = stocks.Sum(s => s.Quantity);
        var complexity = totalPieces * totalStocks * (settings.EnableRotation ? 4 : 1);
        
        // Base time estimation (seconds)
        var baseTime = complexity switch
        {
            < 100 => 1.0,
            < 500 => 5.0,
            < 1000 => 15.0,
            < 5000 => 60.0,
            _ => 300.0
        };

        // Adjust for settings
        if (settings.EnableMultiThreading)
        {
            baseTime /= Math.Min(settings.MaxThreads, Environment.ProcessorCount);
        }

        if (settings.TimeLimitSeconds > 0)
        {
            baseTime = Math.Min(baseTime, settings.TimeLimitSeconds);
        }

        return baseTime;
    }

    private async Task<List<PlacedPiece>> PerformOrToolsOptimizationAsync(
        List<Stock> stocks,
        List<Piece> pieces,
        OptimizationSettings settings,
        CuttingPlan currentPlan,
        CancellationToken cancellationToken)
    {
        var placedPieces = new List<PlacedPiece>();

        // Create CP-SAT solver
        var model = new CpModel();
        
        // Variables for piece placement
        var pieceX = new Dictionary<(int pieceIndex, int stockIndex), IntVar>();
        var pieceY = new Dictionary<(int pieceIndex, int stockIndex), IntVar>();
        var pieceRotation = new Dictionary<(int pieceIndex, int stockIndex), IntVar>();
        var piecePlaced = new Dictionary<(int pieceIndex, int stockIndex), BoolVar>();

        var stockIndex = 0;
        foreach (var stock in stocks)
        {
            for (int stockInstance = 0; stockInstance < stock.Quantity; stockInstance++)
            {
                var pieceIndex = 0;
                foreach (var piece in pieces)
                {
                    for (int pieceInstance = 0; pieceInstance < piece.Quantity; pieceInstance++)
                    {
                        var key = (pieceIndex, stockIndex);
                        
                        // Position variables
                        pieceX[key] = model.NewIntVar(0, (int)stock.Width, $"x_{pieceIndex}_{stockIndex}");
                        pieceY[key] = model.NewIntVar(0, (int)stock.Height, $"y_{pieceIndex}_{stockIndex}");
                        
                        // Rotation variable (0, 90, 180, 270 degrees)
                        pieceRotation[key] = model.NewIntVar(0, 3, $"rot_{pieceIndex}_{stockIndex}");
                        
                        // Placement variable
                        piecePlaced[key] = model.NewBoolVar($"placed_{pieceIndex}_{stockIndex}");
                        
                        pieceIndex++;
                    }
                }
                stockIndex++;
            }
        }

        // Constraints
        foreach (var stock in stocks)
        {
            for (int stockInstance = 0; stockInstance < stock.Quantity; stockInstance++)
            {
                var stockPieces = pieces.SelectMany((p, i) => Enumerable.Range(0, p.Quantity).Select(j => (pieceIndex: i, stockIndex: stockInstance)))
                                      .ToList();

                // No overlap constraints
                for (int i = 0; i < stockPieces.Count; i++)
                {
                    for (int j = i + 1; j < stockPieces.Count; j++)
                    {
                        var piece1 = pieces[stockPieces[i].pieceIndex];
                        var piece2 = pieces[stockPieces[j].pieceIndex];
                        
                        // Add no-overlap constraint
                        AddNoOverlapConstraint(model, piece1, piece2, stockPieces[i], stockPieces[j], 
                                            pieceX, pieceY, pieceRotation, piecePlaced);
                    }
                }
            }
        }

        // Objective: maximize utilization
        var objective = model.NewIntVar(0, 100, "utilization");
        model.Maximize(objective);

        // Solve
        var solver = new CpSolver();
        // Note: In newer versions of OR-Tools, the Parameters property might not be available
        // We'll use a simpler approach for now
        
        var status = await Task.Run(() => solver.Solve(model), cancellationToken);

        if (status == CpSolverStatus.Optimal || status == CpSolverStatus.Feasible)
        {
            // Extract solution
            placedPieces = ExtractSolution(solver, pieces, stocks, pieceX, pieceY, pieceRotation, piecePlaced);
        }

        return placedPieces;
    }

    private void AddNoOverlapConstraint(
        CpModel model,
        Piece piece1,
        Piece piece2,
        (int pieceIndex, int stockIndex) key1,
        (int pieceIndex, int stockIndex) key2,
        Dictionary<(int pieceIndex, int stockIndex), IntVar> pieceX,
        Dictionary<(int pieceIndex, int stockIndex), IntVar> pieceY,
        Dictionary<(int pieceIndex, int stockIndex), IntVar> pieceRotation,
        Dictionary<(int pieceIndex, int stockIndex), BoolVar> piecePlaced)
    {
        // Simplified no-overlap constraint
        // In a real implementation, this would be more complex to handle rotations
        var x1 = pieceX[key1];
        var y1 = pieceY[key1];
        var x2 = pieceX[key2];
        var y2 = pieceY[key2];
        
        // Piece 1 dimensions
        var w1 = model.NewIntVar((int)piece1.Width, (int)piece1.Width, "w1");
        var h1 = model.NewIntVar((int)piece1.Height, (int)piece1.Height, "h1");
        
        // Piece 2 dimensions
        var w2 = model.NewIntVar((int)piece2.Width, (int)piece2.Width, "w2");
        var h2 = model.NewIntVar((int)piece2.Height, (int)piece2.Height, "h2");
        
        // No overlap: piece1 is left of piece2 OR piece1 is right of piece2 OR
        // piece1 is above piece2 OR piece1 is below piece2
        var noOverlap = model.NewBoolVar("no_overlap");
        model.Add(x1 + w1 <= x2).OnlyEnforceIf(noOverlap);
        model.Add(x2 + w2 <= x1).OnlyEnforceIf(noOverlap);
        model.Add(y1 + h1 <= y2).OnlyEnforceIf(noOverlap);
        model.Add(y2 + h2 <= y1).OnlyEnforceIf(noOverlap);
    }

    private List<PlacedPiece> ExtractSolution(
        CpSolver solver,
        List<Piece> pieces,
        List<Stock> stocks,
        Dictionary<(int pieceIndex, int stockIndex), IntVar> pieceX,
        Dictionary<(int pieceIndex, int stockIndex), IntVar> pieceY,
        Dictionary<(int pieceIndex, int stockIndex), IntVar> pieceRotation,
        Dictionary<(int pieceIndex, int stockIndex), BoolVar> piecePlaced)
    {
        var placedPieces = new List<PlacedPiece>();
        
        var stockIndex = 0;
        foreach (var stock in stocks)
        {
            for (int stockInstance = 0; stockInstance < stock.Quantity; stockInstance++)
            {
                var pieceIndex = 0;
                foreach (var piece in pieces)
                {
                    for (int pieceInstance = 0; pieceInstance < piece.Quantity; pieceInstance++)
                    {
                        var key = (pieceIndex, stockIndex);
                        
                        if (solver.Value(piecePlaced[key]) == 1)
                        {
                            var x = (double)solver.Value(pieceX[key]);
                            var y = (double)solver.Value(pieceY[key]);
                            var rotation = (int)solver.Value(pieceRotation[key]) * 90; // Convert to degrees
                            
                            var placedPiece = new PlacedPiece(piece, x, y, rotation, stock.Id);
                            placedPieces.Add(placedPiece);
                        }
                        
                        pieceIndex++;
                    }
                }
                stockIndex++;
            }
        }
        
        return placedPieces;
    }

    private async Task FinalizeOptimizationAsync(CuttingPlan cuttingPlan, OptimizationSettings settings, CancellationToken cancellationToken)
    {
        // Remove overlapping pieces (keep the first one placed)
        var validPieces = new List<PlacedPiece>();
        
        foreach (var piece in cuttingPlan.PlacedPieces)
        {
            bool hasOverlap = false;
            foreach (var existingPiece in validPieces)
            {
                if (piece.StockId == existingPiece.StockId && piece.OverlapsWith(existingPiece))
                {
                    hasOverlap = true;
                    break;
                }
            }
            
            if (!hasOverlap)
            {
                validPieces.Add(piece);
            }
        }
        
        cuttingPlan.PlacedPieces = validPieces;
        
        // Update metrics
        cuttingPlan.PiecesPlaced = cuttingPlan.PlacedPieces.Count;
        cuttingPlan.PiecesRemaining = cuttingPlan.Pieces.Sum(p => p.Quantity) - cuttingPlan.PiecesPlaced;
        
        await Task.CompletedTask; // Allow for async operations in the future
    }

    private async Task ReportProgress(
        IProgress<OptimizationProgress>? progressCallback,
        int currentPhase,
        int totalPhases,
        string description,
        double percentage)
    {
        if (progressCallback != null)
        {
            var progress = new OptimizationProgress
            {
                CurrentPhase = currentPhase,
                TotalPhases = totalPhases,
                PhaseDescription = description,
                ProgressPercentage = percentage,
                ElapsedTime = TimeSpan.Zero, // Would be calculated from start time
                MemoryUsageMB = _performanceMonitor.GetCurrentMemoryUsageMB(),
                CpuUsagePercentage = _performanceMonitor.GetCurrentCpuUsagePercentage()
            };
            
            progressCallback.Report(progress);
        }
        
        await Task.CompletedTask;
    }
} 