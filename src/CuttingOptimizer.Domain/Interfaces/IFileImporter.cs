using System.Collections.Generic;
using System.Threading.Tasks;
using CuttingOptimizer.Domain.Entities;

namespace CuttingOptimizer.Domain.Interfaces;

/// <summary>
/// Interface for importing cutting data from various file formats
/// </summary>
public interface IFileImporter
{
    /// <summary>
    /// Imports stock data from a file
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <param name="format">File format</param>
    /// <returns>List of stock materials</returns>
    Task<List<Stock>> ImportStocksAsync(string filePath, FileFormat format);
    
    /// <summary>
    /// Imports piece data from a file
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <param name="format">File format</param>
    /// <returns>List of pieces</returns>
    Task<List<Piece>> ImportPiecesAsync(string filePath, FileFormat format);
    
    /// <summary>
    /// Imports complete cutting data from a file
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <param name="format">File format</param>
    /// <returns>Cutting data containing both stocks and pieces</returns>
    Task<CuttingData> ImportCuttingDataAsync(string filePath, FileFormat format);
    
    /// <summary>
    /// Validates if the file format is supported
    /// </summary>
    /// <param name="filePath">Path to the file</param>
    /// <returns>True if the format is supported</returns>
    bool IsFormatSupported(string filePath);
    
    /// <summary>
    /// Gets the supported file formats
    /// </summary>
    /// <returns>List of supported formats</returns>
    List<FileFormat> GetSupportedFormats();
}

/// <summary>
/// Represents file formats supported by the importer
/// </summary>
public enum FileFormat
{
    JSON,
    CSV,
    XML
}

/// <summary>
/// Represents complete cutting data with stocks and pieces
/// </summary>
public class CuttingData
{
    public List<Stock> Stocks { get; set; } = new();
    public List<Piece> Pieces { get; set; } = new();
    public OptimizationSettings? Settings { get; set; }
} 