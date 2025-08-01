using System.Collections.Generic;
using System.Threading.Tasks;
using CuttingOptimizer.Domain.Entities;

namespace CuttingOptimizer.Domain.Interfaces;

/// <summary>
/// Interface for exporting cutting plans to various file formats
/// </summary>
public interface IFileExporter
{
    /// <summary>
    /// Exports a cutting plan to a file
    /// </summary>
    /// <param name="cuttingPlan">The cutting plan to export</param>
    /// <param name="filePath">Output file path</param>
    /// <param name="format">Export format</param>
    /// <param name="options">Export options</param>
    /// <returns>Task representing the export operation</returns>
    Task ExportAsync(CuttingPlan cuttingPlan, string filePath, ExportFormat format, ExportOptions? options = null);
    
    /// <summary>
    /// Exports a cutting plan to SVG format
    /// </summary>
    /// <param name="cuttingPlan">The cutting plan to export</param>
    /// <param name="options">Export options</param>
    /// <returns>SVG content as string</returns>
    Task<string> ExportToSvgAsync(CuttingPlan cuttingPlan, ExportOptions? options = null);
    
    /// <summary>
    /// Exports a cutting plan to PNG format
    /// </summary>
    /// <param name="cuttingPlan">The cutting plan to export</param>
    /// <param name="options">Export options</param>
    /// <returns>PNG data as byte array</returns>
    Task<byte[]> ExportToPngAsync(CuttingPlan cuttingPlan, ExportOptions? options = null);
    
    /// <summary>
    /// Exports a cutting plan to PDF format
    /// </summary>
    /// <param name="cuttingPlan">The cutting plan to export</param>
    /// <param name="options">Export options</param>
    /// <returns>PDF data as byte array</returns>
    Task<byte[]> ExportToPdfAsync(CuttingPlan cuttingPlan, ExportOptions? options = null);
    
    /// <summary>
    /// Exports a cutting plan to JSON format
    /// </summary>
    /// <param name="cuttingPlan">The cutting plan to export</param>
    /// <param name="options">Export options</param>
    /// <returns>JSON content as string</returns>
    Task<string> ExportToJsonAsync(CuttingPlan cuttingPlan, ExportOptions? options = null);
    
    /// <summary>
    /// Gets the supported export formats
    /// </summary>
    /// <returns>List of supported formats</returns>
    List<ExportFormat> GetSupportedFormats();
}

/// <summary>
/// Options for exporting cutting plans
/// </summary>
public class ExportOptions
{
    public bool ShowGrid { get; set; } = true;
    public bool ShowLabels { get; set; } = true;
    public bool ShowRotations { get; set; } = true;
    public bool ShowUtilization { get; set; } = true;
    public string ColorScheme { get; set; } = "Default";
    public int ImageWidth { get; set; } = 1920;
    public int ImageHeight { get; set; } = 1080;
    public double Scale { get; set; } = 1.0;
    public bool IncludeMetadata { get; set; } = true;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
} 