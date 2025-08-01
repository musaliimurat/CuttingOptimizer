namespace CuttingOptimizer.Domain.Entities;

/// <summary>
/// Defines settings for the cutting optimization algorithm
/// </summary>
public class OptimizationSettings
{
    // Algorithm settings
    public bool EnableRotation { get; set; } = true;
    public RotationType AllowedRotations { get; set; } = RotationType.All;
    public bool EnableMultiThreading { get; set; } = true;
    public int MaxThreads { get; set; } = Environment.ProcessorCount;
    
    // OR-Tools settings
    public int TimeLimitSeconds { get; set; } = 300; // 5 minutes
    public double GapTolerance { get; set; } = 0.01; // 1% gap tolerance
    public bool EnableSymmetryBreaking { get; set; } = true;
    
    // Heuristic settings
    public bool EnableHeuristicPreArrangement { get; set; } = true;
    public HeuristicStrategy HeuristicStrategy { get; set; } = HeuristicStrategy.LargestFirst;
    public bool EnableGreedyPlacement { get; set; } = true;
    
    // Performance settings
    public bool EnablePerformanceMonitoring { get; set; } = true;
    public bool EnableDetailedLogging { get; set; } = false;
    
    // Output settings
    public bool GenerateVisualization { get; set; } = true;
    public bool GenerateReport { get; set; } = true;
    public ExportFormat ExportFormat { get; set; } = ExportFormat.SVG;
    
    public OptimizationSettings()
    {
    }
    
    /// <summary>
    /// Creates a copy of these settings
    /// </summary>
    public OptimizationSettings Clone()
    {
        return new OptimizationSettings
        {
            EnableRotation = EnableRotation,
            AllowedRotations = AllowedRotations,
            EnableMultiThreading = EnableMultiThreading,
            MaxThreads = MaxThreads,
            TimeLimitSeconds = TimeLimitSeconds,
            GapTolerance = GapTolerance,
            EnableSymmetryBreaking = EnableSymmetryBreaking,
            EnableHeuristicPreArrangement = EnableHeuristicPreArrangement,
            HeuristicStrategy = HeuristicStrategy,
            EnableGreedyPlacement = EnableGreedyPlacement,
            EnablePerformanceMonitoring = EnablePerformanceMonitoring,
            EnableDetailedLogging = EnableDetailedLogging,
            GenerateVisualization = GenerateVisualization,
            GenerateReport = GenerateReport,
            ExportFormat = ExportFormat
        };
    }
}

/// <summary>
/// Defines the heuristic strategy for pre-arrangement
/// </summary>
public enum HeuristicStrategy
{
    LargestFirst,
    SmallestFirst,
    AreaDescending,
    AreaAscending,
    PerimeterDescending,
    PerimeterAscending,
    Random
}

/// <summary>
/// Defines the export format for cutting plans
/// </summary>
public enum ExportFormat
{
    SVG,
    PNG,
    PDF,
    JSON
} 