using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CuttingOptimizer.Domain.Entities;

namespace CuttingOptimizer.Domain.Interfaces;

/// <summary>
/// Core interface for the cutting optimization engine
/// </summary>
public interface IOptimizationEngine
{
    /// <summary>
    /// Optimizes the cutting plan using the provided settings
    /// </summary>
    /// <param name="stocks">Available stock materials</param>
    /// <param name="pieces">Pieces to be cut</param>
    /// <param name="settings">Optimization settings</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="progressCallback">Progress callback for real-time updates</param>
    /// <returns>The optimized cutting plan</returns>
    Task<CuttingPlan> OptimizeAsync(
        List<Stock> stocks,
        List<Piece> pieces,
        OptimizationSettings settings,
        CancellationToken cancellationToken = default,
        IProgress<OptimizationProgress>? progressCallback = null);
    
    /// <summary>
    /// Validates if the optimization problem is feasible
    /// </summary>
    /// <param name="stocks">Available stock materials</param>
    /// <param name="pieces">Pieces to be cut</param>
    /// <returns>Validation result</returns>
    ValidationResult ValidateProblem(List<Stock> stocks, List<Piece> pieces);
    
    /// <summary>
    /// Estimates the optimization time based on problem size
    /// </summary>
    /// <param name="stocks">Available stock materials</param>
    /// <param name="pieces">Pieces to be cut</param>
    /// <param name="settings">Optimization settings</param>
    /// <returns>Estimated time in seconds</returns>
    double EstimateOptimizationTime(List<Stock> stocks, List<Piece> pieces, OptimizationSettings settings);
}

/// <summary>
/// Represents optimization progress information
/// </summary>
public class OptimizationProgress
{
    public int CurrentPhase { get; set; }
    public int TotalPhases { get; set; }
    public string PhaseDescription { get; set; } = string.Empty;
    public double ProgressPercentage { get; set; }
    public int PiecesPlaced { get; set; }
    public int TotalPieces { get; set; }
    public double CurrentUtilization { get; set; }
    public TimeSpan ElapsedTime { get; set; }
    public double MemoryUsageMB { get; set; }
    public double CpuUsagePercentage { get; set; }
}

/// <summary>
/// Represents validation result for optimization problems
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public double TotalStockArea { get; set; }
    public double TotalPieceArea { get; set; }
    public double EstimatedUtilization { get; set; }
} 