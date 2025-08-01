namespace CuttingOptimizer.Domain.Interfaces;

/// <summary>
/// Interface for monitoring system performance during optimization
/// </summary>
public interface IPerformanceMonitor
{
    /// <summary>
    /// Gets the current memory usage in MB
    /// </summary>
    double GetCurrentMemoryUsageMB();
    
    /// <summary>
    /// Gets the current CPU usage percentage
    /// </summary>
    double GetCurrentCpuUsagePercentage();
    
    /// <summary>
    /// Gets the peak memory usage in MB since monitoring started
    /// </summary>
    double GetPeakMemoryUsageMB();
    
    /// <summary>
    /// Gets the average CPU usage percentage since monitoring started
    /// </summary>
    double GetAverageCpuUsagePercentage();
    
    /// <summary>
    /// Starts performance monitoring
    /// </summary>
    void StartMonitoring();
    
    /// <summary>
    /// Stops performance monitoring
    /// </summary>
    void StopMonitoring();
    
    /// <summary>
    /// Resets all performance counters
    /// </summary>
    void Reset();
    
    /// <summary>
    /// Gets a performance snapshot
    /// </summary>
    PerformanceSnapshot GetSnapshot();
}

/// <summary>
/// Represents a snapshot of system performance metrics
/// </summary>
public class PerformanceSnapshot
{
    public double CurrentMemoryUsageMB { get; set; }
    public double PeakMemoryUsageMB { get; set; }
    public double CurrentCpuUsagePercentage { get; set; }
    public double AverageCpuUsagePercentage { get; set; }
    public TimeSpan MonitoringDuration { get; set; }
    public DateTime Timestamp { get; set; }
} 