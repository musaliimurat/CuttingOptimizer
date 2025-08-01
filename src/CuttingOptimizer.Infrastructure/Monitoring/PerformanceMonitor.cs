using System;
using System.Diagnostics;
using System.Threading;
using CuttingOptimizer.Domain.Interfaces;

namespace CuttingOptimizer.Infrastructure.Monitoring;

/// <summary>
/// Performance monitor implementation using Windows Performance Counters
/// </summary>
public class PerformanceMonitor : IPerformanceMonitor, IDisposable
{
    private readonly PerformanceCounter? _cpuCounter;
    private readonly Process _currentProcess;
    private readonly Thread _monitoringThread;
    private readonly object _lockObject = new();
    
    private bool _isMonitoring;
    private double _peakMemoryUsageMB;
    private double _totalCpuUsage;
    private int _cpuReadings;
    private DateTime _startTime;
    
    public PerformanceMonitor()
    {
        _currentProcess = Process.GetCurrentProcess();
        
        try
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        }
        catch
        {
            // Fallback if performance counters are not available
            _cpuCounter = null;
        }
        
        _monitoringThread = new Thread(MonitoringLoop)
        {
            IsBackground = true
        };
    }
    
    public double GetCurrentMemoryUsageMB()
    {
        lock (_lockObject)
        {
            _currentProcess.Refresh();
            var memoryBytes = _currentProcess.WorkingSet64;
            var memoryMB = memoryBytes / (1024.0 * 1024.0);
            
            if (memoryMB > _peakMemoryUsageMB)
            {
                _peakMemoryUsageMB = memoryMB;
            }
            
            return memoryMB;
        }
    }
    
    public double GetCurrentCpuUsagePercentage()
    {
        if (_cpuCounter == null)
            return 0.0;
        
        try
        {
            lock (_lockObject)
            {
                var cpuUsage = _cpuCounter.NextValue();
                _totalCpuUsage += cpuUsage;
                _cpuReadings++;
                return cpuUsage;
            }
        }
        catch
        {
            return 0.0;
        }
    }
    
    public double GetPeakMemoryUsageMB()
    {
        lock (_lockObject)
        {
            return _peakMemoryUsageMB;
        }
    }
    
    public double GetAverageCpuUsagePercentage()
    {
        lock (_lockObject)
        {
            return _cpuReadings > 0 ? _totalCpuUsage / _cpuReadings : 0.0;
        }
    }
    
    public void StartMonitoring()
    {
        lock (_lockObject)
        {
            if (!_isMonitoring)
            {
                _isMonitoring = true;
                _startTime = DateTime.UtcNow;
                _peakMemoryUsageMB = 0;
                _totalCpuUsage = 0;
                _cpuReadings = 0;
                
                if (!_monitoringThread.IsAlive)
                {
                    _monitoringThread.Start();
                }
            }
        }
    }
    
    public void StopMonitoring()
    {
        lock (_lockObject)
        {
            _isMonitoring = false;
        }
    }
    
    public void Reset()
    {
        lock (_lockObject)
        {
            _peakMemoryUsageMB = 0;
            _totalCpuUsage = 0;
            _cpuReadings = 0;
            _startTime = DateTime.UtcNow;
        }
    }
    
    public PerformanceSnapshot GetSnapshot()
    {
        lock (_lockObject)
        {
            return new PerformanceSnapshot
            {
                CurrentMemoryUsageMB = GetCurrentMemoryUsageMB(),
                PeakMemoryUsageMB = _peakMemoryUsageMB,
                CurrentCpuUsagePercentage = GetCurrentCpuUsagePercentage(),
                AverageCpuUsagePercentage = GetAverageCpuUsagePercentage(),
                MonitoringDuration = DateTime.UtcNow - _startTime,
                Timestamp = DateTime.UtcNow
            };
        }
    }
    
    private void MonitoringLoop()
    {
        while (true)
        {
            lock (_lockObject)
            {
                if (!_isMonitoring)
                {
                    break;
                }
                
                // Update metrics
                GetCurrentMemoryUsageMB();
                GetCurrentCpuUsagePercentage();
            }
            
            Thread.Sleep(100); // Update every 100ms
        }
    }
    
    public void Dispose()
    {
        StopMonitoring();
        _cpuCounter?.Dispose();
        _currentProcess?.Dispose();
    }
} 