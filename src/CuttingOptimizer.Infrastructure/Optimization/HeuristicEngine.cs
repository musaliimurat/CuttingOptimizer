using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CuttingOptimizer.Domain.Entities;
using CuttingOptimizer.Domain.Interfaces;

namespace CuttingOptimizer.Infrastructure.Optimization;

/// <summary>
/// Heuristic engine for pre-arrangement of pieces using various strategies
/// </summary>
public class HeuristicEngine : IHeuristicEngine
{
    public async Task<List<PlacedPiece>> PreArrangeAsync(
        List<Stock> stocks,
        List<Piece> pieces,
        OptimizationSettings settings,
        CancellationToken cancellationToken = default)
    {
        var placedPieces = new List<PlacedPiece>();
        
        // Sort pieces according to strategy
        var sortedPieces = SortPiecesByStrategy(pieces, settings.HeuristicStrategy);
        
        // Create stock instances
        var stockInstances = CreateStockInstances(stocks);
        
        // Place pieces using greedy algorithm
        foreach (var piece in sortedPieces)
        {
            for (int i = 0; i < piece.Quantity; i++)
            {
                var placedPiece = await PlacePieceGreedyAsync(piece, stockInstances, settings, cancellationToken);
                if (placedPiece != null)
                {
                    placedPieces.Add(placedPiece);
                }
                
                if (cancellationToken.IsCancellationRequested)
                    break;
            }
        }
        
        return placedPieces;
    }
    
    private List<Piece> SortPiecesByStrategy(List<Piece> pieces, HeuristicStrategy strategy)
    {
        return strategy switch
        {
            HeuristicStrategy.LargestFirst => pieces.OrderByDescending(p => p.Area).ToList(),
            HeuristicStrategy.SmallestFirst => pieces.OrderBy(p => p.Area).ToList(),
            HeuristicStrategy.AreaDescending => pieces.OrderByDescending(p => p.Area).ToList(),
            HeuristicStrategy.AreaAscending => pieces.OrderBy(p => p.Area).ToList(),
            HeuristicStrategy.PerimeterDescending => pieces.OrderByDescending(p => 2 * (p.Width + p.Height)).ToList(),
            HeuristicStrategy.PerimeterAscending => pieces.OrderBy(p => 2 * (p.Width + p.Height)).ToList(),
            HeuristicStrategy.Random => pieces.OrderBy(p => Guid.NewGuid()).ToList(),
            _ => pieces.OrderByDescending(p => p.Area).ToList()
        };
    }
    
    private List<StockInstance> CreateStockInstances(List<Stock> stocks)
    {
        var instances = new List<StockInstance>();
        
        foreach (var stock in stocks)
        {
            for (int i = 0; i < stock.Quantity; i++)
            {
                instances.Add(new StockInstance
                {
                    Stock = stock,
                    InstanceId = i,
                    AvailableArea = stock.Area,
                    PlacedPieces = new List<PlacedPiece>()
                });
            }
        }
        
        return instances;
    }
    
    private async Task<PlacedPiece?> PlacePieceGreedyAsync(
        Piece piece,
        List<StockInstance> stockInstances,
        OptimizationSettings settings,
        CancellationToken cancellationToken)
    {
        var bestPlacement = (PlacedPiece?)null;
        var bestUtilization = 0.0;
        
        foreach (var stockInstance in stockInstances)
        {
            if (stockInstance.AvailableArea < piece.Area)
                continue;
            
            // Try different rotations if enabled
            var rotations = settings.EnableRotation ? new[] { 0, 90, 180, 270 } : new[] { 0 };
            
            foreach (var rotation in rotations)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;
                
                var placement = TryPlacePiece(piece, stockInstance, rotation);
                if (placement != null)
                {
                    var utilization = CalculateUtilization(stockInstance, placement);
                    if (utilization > bestUtilization)
                    {
                        bestUtilization = utilization;
                        bestPlacement = placement;
                    }
                }
            }
        }
        
        // Apply the best placement
        if (bestPlacement != null)
        {
            var stockInstance = stockInstances.First(s => s.Stock.Id == bestPlacement.StockId);
            stockInstance.PlacedPieces.Add(bestPlacement);
            stockInstance.AvailableArea -= bestPlacement.Area;
        }
        
        await Task.CompletedTask; // Allow for async operations in the future
        return bestPlacement;
    }
    
    private PlacedPiece? TryPlacePiece(Piece piece, StockInstance stockInstance, int rotation)
    {
        var effectiveWidth = rotation == 90 || rotation == 270 ? piece.Height : piece.Width;
        var effectiveHeight = rotation == 90 || rotation == 270 ? piece.Width : piece.Height;
        
        // Simple placement strategy: try to place in the first available position
        var positions = GeneratePlacementPositions(stockInstance, effectiveWidth, effectiveHeight);
        
        foreach (var position in positions)
        {
            var testPiece = new PlacedPiece(piece, position.x, position.y, rotation, stockInstance.Stock.Id);
            
            // Check if placement is valid
            if (IsValidPlacement(testPiece, stockInstance))
            {
                return testPiece;
            }
        }
        
        return null;
    }
    
    private List<(double x, double y)> GeneratePlacementPositions(StockInstance stockInstance, double width, double height)
    {
        var positions = new List<(double x, double y)>();
        
        // Try corners first
        positions.Add((0, 0));
        positions.Add((stockInstance.Stock.Width - width, 0));
        positions.Add((0, stockInstance.Stock.Height - height));
        positions.Add((stockInstance.Stock.Width - width, stockInstance.Stock.Height - height));
        
        // Try along edges
        for (double x = 0; x <= stockInstance.Stock.Width - width; x += width)
        {
            positions.Add((x, 0));
            positions.Add((x, stockInstance.Stock.Height - height));
        }
        
        for (double y = 0; y <= stockInstance.Stock.Height - height; y += height)
        {
            positions.Add((0, y));
            positions.Add((stockInstance.Stock.Width - width, y));
        }
        
        return positions.Distinct().ToList();
    }
    
    private bool IsValidPlacement(PlacedPiece testPiece, StockInstance stockInstance)
    {
        // Check if piece fits within stock boundaries
        if (!testPiece.FitsInStock(stockInstance.Stock.Width, stockInstance.Stock.Height))
            return false;
        
        // Check for overlaps with existing pieces
        foreach (var existingPiece in stockInstance.PlacedPieces)
        {
            if (testPiece.OverlapsWith(existingPiece))
                return false;
        }
        
        return true;
    }
    
    private double CalculateUtilization(StockInstance stockInstance, PlacedPiece newPiece)
    {
        var totalArea = stockInstance.Stock.Area;
        var usedArea = stockInstance.PlacedPieces.Sum(p => p.Area) + newPiece.Area;
        return usedArea / totalArea;
    }
    
    private class StockInstance
    {
        public Stock Stock { get; set; } = null!;
        public int InstanceId { get; set; }
        public double AvailableArea { get; set; }
        public List<PlacedPiece> PlacedPieces { get; set; } = new();
    }
} 