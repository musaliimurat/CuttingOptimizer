using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CuttingOptimizer.Domain.Entities;
using CuttingOptimizer.Domain.Interfaces;

namespace CuttingOptimizer.Application.Services;

/// <summary>
/// Main service for cutting optimization that orchestrates the entire process
/// </summary>
public class CuttingOptimizationService
{
    private readonly IOptimizationEngine _optimizationEngine;
    private readonly IFileImporter _fileImporter;
    private readonly IFileExporter _fileExporter;
    private readonly IPerformanceMonitor _performanceMonitor;

    public CuttingOptimizationService(
        IOptimizationEngine optimizationEngine,
        IFileImporter fileImporter,
        IFileExporter fileExporter,
        IPerformanceMonitor performanceMonitor)
    {
        _optimizationEngine = optimizationEngine ?? throw new ArgumentNullException(nameof(optimizationEngine));
        _fileImporter = fileImporter ?? throw new ArgumentNullException(nameof(fileImporter));
        _fileExporter = fileExporter ?? throw new ArgumentNullException(nameof(fileExporter));
        _performanceMonitor = performanceMonitor ?? throw new ArgumentNullException(nameof(performanceMonitor));
    }

    /// <summary>
    /// Optimizes cutting from file input
    /// </summary>
    public async Task<CuttingPlan> OptimizeFromFileAsync(
        string stocksFilePath,
        string piecesFilePath,
        OptimizationSettings settings,
        CancellationToken cancellationToken = default,
        IProgress<OptimizationProgress>? progressCallback = null)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Import data
            var stocks = await _fileImporter.ImportStocksAsync(stocksFilePath, GetFileFormat(stocksFilePath));
            var pieces = await _fileImporter.ImportPiecesAsync(piecesFilePath, GetFileFormat(piecesFilePath));
            
            // Validate problem
            var validation = _optimizationEngine.ValidateProblem(stocks, pieces);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException($"Problem validation failed: {string.Join(", ", validation.Errors)}");
            }
            
            // Perform optimization
            var cuttingPlan = await _optimizationEngine.OptimizeAsync(
                stocks, pieces, settings, cancellationToken, progressCallback);
            
            // Calculate final metrics
            cuttingPlan.CalculateUtilization();
            cuttingPlan.OptimizationTime = stopwatch.Elapsed;
            cuttingPlan.MemoryUsageMB = _performanceMonitor.GetCurrentMemoryUsageMB();
            cuttingPlan.CpuUsagePercentage = _performanceMonitor.GetCurrentCpuUsagePercentage();
            
            return cuttingPlan;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    /// <summary>
    /// Optimizes cutting from in-memory data
    /// </summary>
    public async Task<CuttingPlan> OptimizeAsync(
        List<Stock> stocks,
        List<Piece> pieces,
        OptimizationSettings settings,
        CancellationToken cancellationToken = default,
        IProgress<OptimizationProgress>? progressCallback = null)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Validate problem
            var validation = _optimizationEngine.ValidateProblem(stocks, pieces);
            if (!validation.IsValid)
            {
                throw new InvalidOperationException($"Problem validation failed: {string.Join(", ", validation.Errors)}");
            }
            
            // Perform optimization
            var cuttingPlan = await _optimizationEngine.OptimizeAsync(
                stocks, pieces, settings, cancellationToken, progressCallback);
            
            // Calculate final metrics
            cuttingPlan.CalculateUtilization();
            cuttingPlan.OptimizationTime = stopwatch.Elapsed;
            cuttingPlan.MemoryUsageMB = _performanceMonitor.GetCurrentMemoryUsageMB();
            cuttingPlan.CpuUsagePercentage = _performanceMonitor.GetCurrentCpuUsagePercentage();
            
            return cuttingPlan;
        }
        finally
        {
            stopwatch.Stop();
        }
    }

    /// <summary>
    /// Exports a cutting plan to file
    /// </summary>
    public async Task ExportCuttingPlanAsync(
        CuttingPlan cuttingPlan,
        string filePath,
        ExportFormat format,
        ExportOptions? options = null)
    {
        await _fileExporter.ExportAsync(cuttingPlan, filePath, format, options);
    }

    /// <summary>
    /// Estimates optimization time for the given problem
    /// </summary>
    public double EstimateOptimizationTime(List<Stock> stocks, List<Piece> pieces, OptimizationSettings settings)
    {
        return _optimizationEngine.EstimateOptimizationTime(stocks, pieces, settings);
    }

    /// <summary>
    /// Validates a cutting problem
    /// </summary>
    public ValidationResult ValidateProblem(List<Stock> stocks, List<Piece> pieces)
    {
        return _optimizationEngine.ValidateProblem(stocks, pieces);
    }

    /// <summary>
    /// Gets supported file formats for import
    /// </summary>
    public List<FileFormat> GetSupportedImportFormats()
    {
        return _fileImporter.GetSupportedFormats();
    }

    /// <summary>
    /// Gets supported file formats for export
    /// </summary>
    public List<ExportFormat> GetSupportedExportFormats()
    {
        return _fileExporter.GetSupportedFormats();
    }

    private FileFormat GetFileFormat(string filePath)
    {
        var extension = System.IO.Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".json" => FileFormat.JSON,
            ".csv" => FileFormat.CSV,
            ".xml" => FileFormat.XML,
            _ => throw new NotSupportedException($"File format not supported: {extension}")
        };
    }
} 