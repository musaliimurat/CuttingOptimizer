using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CuttingOptimizer.Domain.Entities;
using CuttingOptimizer.Domain.Interfaces;

namespace CuttingOptimizer.Infrastructure.FileHandling;

/// <summary>
/// CSV file importer for cutting data
/// </summary>
public class CsvFileImporter : IFileImporter
{
    public Task<List<Stock>> ImportStocksAsync(string filePath, FileFormat format)
    {
        if (format != FileFormat.CSV)
            throw new NotSupportedException($"Format {format} is not supported by CsvFileImporter");

        var stocks = new List<Stock>();
        
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        });

        try
        {
            var records = csv.GetRecords<CsvStockRecord>();
            foreach (var record in records)
            {
                if (TryParseStock(record, out var stock))
                {
                    stocks.Add(stock);
                }
            }
        }
        catch (CsvHelperException ex)
        {
            throw new InvalidOperationException($"Failed to parse CSV file: {ex.Message}", ex);
        }

        return Task.FromResult(stocks);
    }

    public Task<List<Piece>> ImportPiecesAsync(string filePath, FileFormat format)
    {
        if (format != FileFormat.CSV)
            throw new NotSupportedException($"Format {format} is not supported by CsvFileImporter");

        var pieces = new List<Piece>();
        
        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null
        });

        try
        {
            var records = csv.GetRecords<CsvPieceRecord>();
            foreach (var record in records)
            {
                if (TryParsePiece(record, out var piece))
                {
                    pieces.Add(piece);
                }
            }
        }
        catch (CsvHelperException ex)
        {
            throw new InvalidOperationException($"Failed to parse CSV file: {ex.Message}", ex);
        }

        return Task.FromResult(pieces);
    }

    public Task<CuttingData> ImportCuttingDataAsync(string filePath, FileFormat format)
    {
        // CSV doesn't support combined data, so we'll return empty data
        // In a real implementation, you might have separate files for stocks and pieces
        return Task.FromResult(new CuttingData
        {
            Stocks = new List<Stock>(),
            Pieces = new List<Piece>()
        });
    }

    public bool IsFormatSupported(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension == ".csv";
    }

    public List<FileFormat> GetSupportedFormats()
    {
        return new List<FileFormat> { FileFormat.CSV };
    }

    private bool TryParseStock(CsvStockRecord record, out Stock stock)
    {
        stock = new Stock();
        
        try
        {
            stock.Name = record.Name ?? "Unnamed Stock";
            stock.Width = double.Parse(record.Width ?? "0", CultureInfo.InvariantCulture);
            stock.Height = double.Parse(record.Height ?? "0", CultureInfo.InvariantCulture);
            stock.Quantity = int.Parse(record.Quantity ?? "1", CultureInfo.InvariantCulture);
            stock.Material = record.Material ?? "";
            stock.Thickness = double.Parse(record.Thickness ?? "0", CultureInfo.InvariantCulture);
            stock.Cost = double.Parse(record.Cost ?? "0", CultureInfo.InvariantCulture);
            
            return stock.Width > 0 && stock.Height > 0;
        }
        catch
        {
            return false;
        }
    }

    private bool TryParsePiece(CsvPieceRecord record, out Piece piece)
    {
        piece = new Piece();
        
        try
        {
            piece.Name = record.Name ?? "Unnamed Piece";
            piece.Width = double.Parse(record.Width ?? "0", CultureInfo.InvariantCulture);
            piece.Height = double.Parse(record.Height ?? "0", CultureInfo.InvariantCulture);
            piece.Quantity = int.Parse(record.Quantity ?? "1", CultureInfo.InvariantCulture);
            piece.AllowRotation = bool.Parse(record.AllowRotation ?? "true");
            
            // Parse allowed rotations
            if (!string.IsNullOrEmpty(record.AllowedRotations))
            {
                piece.AllowedRotations = ParseRotationType(record.AllowedRotations);
            }
            
            return piece.Width > 0 && piece.Height > 0;
        }
        catch
        {
            return false;
        }
    }

    private RotationType ParseRotationType(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "none" => RotationType.None,
            "90" => RotationType.Rotate90,
            "180" => RotationType.Rotate180,
            "270" => RotationType.Rotate270,
            "all" => RotationType.All,
            _ => RotationType.All
        };
    }

    private class CsvStockRecord
    {
        public string? Name { get; set; }
        public string? Width { get; set; }
        public string? Height { get; set; }
        public string? Quantity { get; set; }
        public string? Material { get; set; }
        public string? Thickness { get; set; }
        public string? Cost { get; set; }
    }

    private class CsvPieceRecord
    {
        public string? Name { get; set; }
        public string? Width { get; set; }
        public string? Height { get; set; }
        public string? Quantity { get; set; }
        public string? AllowRotation { get; set; }
        public string? AllowedRotations { get; set; }
    }
} 