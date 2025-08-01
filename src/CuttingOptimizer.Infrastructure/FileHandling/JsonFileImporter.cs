using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CuttingOptimizer.Domain.Entities;
using CuttingOptimizer.Domain.Interfaces;

namespace CuttingOptimizer.Infrastructure.FileHandling;

/// <summary>
/// JSON file importer for cutting data
/// </summary>
public class JsonFileImporter : IFileImporter
{
    public async Task<List<Stock>> ImportStocksAsync(string filePath, FileFormat format)
    {
        if (format != FileFormat.JSON)
            throw new NotSupportedException($"Format {format} is not supported by JsonFileImporter");

        var jsonContent = await File.ReadAllTextAsync(filePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        try
        {
            var data = JsonSerializer.Deserialize<JsonImportData>(jsonContent, options);
            return data?.Stocks ?? new List<Stock>();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse JSON file: {ex.Message}", ex);
        }
    }

    public async Task<List<Piece>> ImportPiecesAsync(string filePath, FileFormat format)
    {
        if (format != FileFormat.JSON)
            throw new NotSupportedException($"Format {format} is not supported by JsonFileImporter");

        var jsonContent = await File.ReadAllTextAsync(filePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        try
        {
            var data = JsonSerializer.Deserialize<JsonImportData>(jsonContent, options);
            return data?.Pieces ?? new List<Piece>();
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse JSON file: {ex.Message}", ex);
        }
    }

    public async Task<CuttingData> ImportCuttingDataAsync(string filePath, FileFormat format)
    {
        if (format != FileFormat.JSON)
            throw new NotSupportedException($"Format {format} is not supported by JsonFileImporter");

        var jsonContent = await File.ReadAllTextAsync(filePath);
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        try
        {
            var data = JsonSerializer.Deserialize<JsonImportData>(jsonContent, options);
            return new CuttingData
            {
                Stocks = data?.Stocks ?? new List<Stock>(),
                Pieces = data?.Pieces ?? new List<Piece>(),
                Settings = data?.Settings
            };
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse JSON file: {ex.Message}", ex);
        }
    }

    public bool IsFormatSupported(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension == ".json";
    }

    public List<FileFormat> GetSupportedFormats()
    {
        return new List<FileFormat> { FileFormat.JSON };
    }

    private class JsonImportData
    {
        public List<Stock> Stocks { get; set; } = new();
        public List<Piece> Pieces { get; set; } = new();
        public OptimizationSettings? Settings { get; set; }
    }
} 