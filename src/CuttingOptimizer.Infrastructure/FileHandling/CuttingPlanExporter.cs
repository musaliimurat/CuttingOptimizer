using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CuttingOptimizer.Domain.Entities;
using CuttingOptimizer.Domain.Interfaces;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace CuttingOptimizer.Infrastructure.FileHandling;

/// <summary>
/// Exporter for cutting plans to various formats
/// </summary>
public class CuttingPlanExporter : IFileExporter
{
    public async Task ExportAsync(CuttingPlan cuttingPlan, string filePath, ExportFormat format, ExportOptions? options = null)
    {
        options ??= new ExportOptions();
        
        switch (format)
        {
            case ExportFormat.SVG:
                var svgContent = await ExportToSvgAsync(cuttingPlan, options);
                await File.WriteAllTextAsync(filePath, svgContent);
                break;
                
            case ExportFormat.PNG:
                var pngData = await ExportToPngAsync(cuttingPlan, options);
                await File.WriteAllBytesAsync(filePath, pngData);
                break;
                
            case ExportFormat.PDF:
                var pdfData = await ExportToPdfAsync(cuttingPlan, options);
                await File.WriteAllBytesAsync(filePath, pdfData);
                break;
                
            case ExportFormat.JSON:
                var jsonContent = await ExportToJsonAsync(cuttingPlan, options);
                await File.WriteAllTextAsync(filePath, jsonContent);
                break;
                
            default:
                throw new NotSupportedException($"Export format {format} is not supported");
        }
    }

    public async Task<string> ExportToSvgAsync(CuttingPlan cuttingPlan, ExportOptions? options = null)
    {
        options ??= new ExportOptions();
        
        var svg = new StringBuilder();
        svg.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        svg.AppendLine("<svg xmlns=\"http://www.w3.org/2000/svg\" version=\"1.1\"");
        svg.AppendLine($"  width=\"{options.ImageWidth}\" height=\"{options.ImageHeight}\">");
        
        // Add title and description
        if (options.IncludeMetadata)
        {
            svg.AppendLine($"  <title>{options.Title}</title>");
            svg.AppendLine($"  <desc>{options.Description}</desc>");
        }
        
        // Add grid if requested
        if (options.ShowGrid)
        {
            AddGridToSvg(svg, cuttingPlan, options);
        }
        
        // Add pieces
        foreach (var stock in cuttingPlan.Stocks)
        {
            var stockPieces = cuttingPlan.PlacedPieces.Where(p => p.StockId == stock.Id).ToList();
            AddStockToSvg(svg, stock, stockPieces, options);
        }
        
        // Add utilization info if requested
        if (options.ShowUtilization)
        {
            AddUtilizationToSvg(svg, cuttingPlan, options);
        }
        
        svg.AppendLine("</svg>");
        
        await Task.CompletedTask; // Allow for async operations in the future
        return svg.ToString();
    }

    public async Task<byte[]> ExportToPngAsync(CuttingPlan cuttingPlan, ExportOptions? options = null)
    {
        // For simplicity, we'll return a placeholder PNG
        // In a real implementation, you would use a graphics library like SkiaSharp or System.Drawing
        var placeholderPng = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }; // PNG header
        
        await Task.CompletedTask;
        return placeholderPng;
    }

    public async Task<byte[]> ExportToPdfAsync(CuttingPlan cuttingPlan, ExportOptions? options = null)
    {
        options ??= new ExportOptions();
        
        using var memoryStream = new MemoryStream();
        var writer = new PdfWriter(memoryStream);
        var pdf = new PdfDocument(writer);
        var document = new Document(pdf);
        
        // Add title
        if (!string.IsNullOrEmpty(options.Title))
        {
            var titleParagraph = new Paragraph(options.Title);
            titleParagraph.SetFontSize(20);
            // Note: SetBold() might not be available in this version of iText7
            // We'll use a different approach or skip bold formatting
            document.Add(titleParagraph);
        }
        
        // Add description
        if (!string.IsNullOrEmpty(options.Description))
        {
            var descParagraph = new Paragraph(options.Description);
            descParagraph.SetFontSize(12);
            document.Add(descParagraph);
        }
        
        // Add cutting plan details
        var utilizationParagraph = new Paragraph($"Utilization: {cuttingPlan.UtilizationPercentage:F1}%");
        utilizationParagraph.SetFontSize(14);
        document.Add(utilizationParagraph);
        
        var piecesPlacedParagraph = new Paragraph($"Pieces placed: {cuttingPlan.PiecesPlaced}");
        piecesPlacedParagraph.SetFontSize(12);
        document.Add(piecesPlacedParagraph);
        
        var piecesRemainingParagraph = new Paragraph($"Pieces remaining: {cuttingPlan.PiecesRemaining}");
        piecesRemainingParagraph.SetFontSize(12);
        document.Add(piecesRemainingParagraph);
        
        var optimizationTimeParagraph = new Paragraph($"Optimization time: {cuttingPlan.OptimizationTime}");
        optimizationTimeParagraph.SetFontSize(12);
        document.Add(optimizationTimeParagraph);
        
        // Add stock information
        foreach (var stock in cuttingPlan.Stocks)
        {
            var stockPieces = cuttingPlan.PlacedPieces.Where(p => p.StockId == stock.Id).ToList();
            var stockParagraph = new Paragraph($"Stock: {stock.Name} ({stock.Width}x{stock.Height})");
            stockParagraph.SetFontSize(14);
            // Note: SetBold() might not be available in this version of iText7
            document.Add(stockParagraph);
            
            foreach (var piece in stockPieces)
            {
                var pieceParagraph = new Paragraph($"  - {piece.Piece.Name}: {piece.X},{piece.Y} (rot: {piece.RotationAngle}°)");
                pieceParagraph.SetFontSize(10);
                document.Add(pieceParagraph);
            }
        }
        
        document.Close();
        
        await Task.CompletedTask;
        return memoryStream.ToArray();
    }

    public async Task<string> ExportToJsonAsync(CuttingPlan cuttingPlan, ExportOptions? options = null)
    {
        options ??= new ExportOptions();
        
        var exportData = new
        {
            cuttingPlan.Id,
            cuttingPlan.Name,
            cuttingPlan.CreatedAt,
            cuttingPlan.UtilizationPercentage,
            cuttingPlan.PiecesPlaced,
            cuttingPlan.PiecesRemaining,
            cuttingPlan.OptimizationTime,
            cuttingPlan.MemoryUsageMB,
            cuttingPlan.CpuUsagePercentage,
            Stocks = cuttingPlan.Stocks,
            Pieces = cuttingPlan.Pieces,
            PlacedPieces = cuttingPlan.PlacedPieces.Select(p => new
            {
                p.Piece.Name,
                p.X,
                p.Y,
                p.RotationAngle,
                p.EffectiveWidth,
                p.EffectiveHeight,
                p.Area,
                StockId = p.StockId
            }).ToList()
        };
        
        var options2 = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        
        var json = JsonSerializer.Serialize(exportData, options2);
        
        await Task.CompletedTask;
        return json;
    }

    public List<ExportFormat> GetSupportedFormats()
    {
        return new List<ExportFormat> { ExportFormat.SVG, ExportFormat.PNG, ExportFormat.PDF, ExportFormat.JSON };
    }

    private void AddGridToSvg(StringBuilder svg, CuttingPlan cuttingPlan, ExportOptions options)
    {
        var gridSize = 10;
        var strokeColor = "#CCCCCC";
        
        svg.AppendLine($"  <defs>");
        svg.AppendLine($"    <pattern id=\"grid\" width=\"{gridSize}\" height=\"{gridSize}\" patternUnits=\"userSpaceOnUse\">");
        svg.AppendLine($"      <path d=\"M {gridSize} 0 L 0 0 0 {gridSize}\" fill=\"none\" stroke=\"{strokeColor}\" stroke-width=\"0.5\"/>");
        svg.AppendLine($"    </pattern>");
        svg.AppendLine($"  </defs>");
        svg.AppendLine($"  <rect width=\"100%\" height=\"100%\" fill=\"url(#grid)\"/>");
    }

    private void AddStockToSvg(StringBuilder svg, Stock stock, List<PlacedPiece> pieces, ExportOptions options)
    {
        var scale = options.Scale;
        var stockX = 50;
        var stockY = 50;
        var stockWidth = stock.Width * scale;
        var stockHeight = stock.Height * scale;
        
        // Stock outline
        svg.AppendLine($"  <rect x=\"{stockX}\" y=\"{stockY}\" width=\"{stockWidth}\" height=\"{stockHeight}\"");
        svg.AppendLine($"    fill=\"none\" stroke=\"#000000\" stroke-width=\"2\"/>");
        
        // Stock label
        if (options.ShowLabels)
        {
            svg.AppendLine($"  <text x=\"{stockX + stockWidth/2}\" y=\"{stockY - 10}\" text-anchor=\"middle\" font-size=\"12\">");
            svg.AppendLine($"    {stock.Name} ({stock.Width}x{stock.Height})");
            svg.AppendLine($"  </text>");
        }
        
        // Pieces
        foreach (var piece in pieces)
        {
            AddPieceToSvg(svg, piece, stockX, stockY, scale, options);
        }
    }

    private void AddPieceToSvg(StringBuilder svg, PlacedPiece piece, double stockX, double stockY, double scale, ExportOptions options)
    {
        var pieceX = stockX + piece.X * scale;
        var pieceY = stockY + piece.Y * scale;
        var pieceWidth = piece.EffectiveWidth * scale;
        var pieceHeight = piece.EffectiveHeight * scale;
        
        // Generate color based on piece name
        var color = GetColorForPiece(piece.Piece.Name);
        
        // Piece rectangle
        svg.AppendLine($"  <rect x=\"{pieceX}\" y=\"{pieceY}\" width=\"{pieceWidth}\" height=\"{pieceHeight}\"");
        svg.AppendLine($"    fill=\"{color}\" stroke=\"#000000\" stroke-width=\"1\"/>");
        
        // Piece label
        if (options.ShowLabels)
        {
            svg.AppendLine($"  <text x=\"{pieceX + pieceWidth/2}\" y=\"{pieceY + pieceHeight/2}\" text-anchor=\"middle\" dominant-baseline=\"middle\" font-size=\"10\">");
            svg.AppendLine($"    {piece.Piece.Name}");
            svg.AppendLine($"  </text>");
        }
        
        // Rotation indicator
        if (options.ShowRotations && piece.RotationAngle != 0)
        {
            var centerX = pieceX + pieceWidth / 2;
            var centerY = pieceY + pieceHeight / 2;
            var radius = Math.Min(pieceWidth, pieceHeight) / 4;
            
            svg.AppendLine($"  <circle cx=\"{centerX}\" cy=\"{centerY}\" r=\"{radius}\" fill=\"none\" stroke=\"#FF0000\" stroke-width=\"1\"/>");
            svg.AppendLine($"  <line x1=\"{centerX}\" y1=\"{centerY}\" x2=\"{centerX + radius * Math.Cos(piece.RotationAngle * Math.PI / 180)}\" y2=\"{centerY + radius * Math.Sin(piece.RotationAngle * Math.PI / 180)}\" stroke=\"#FF0000\" stroke-width=\"2\"/>");
        }
    }

    private void AddUtilizationToSvg(StringBuilder svg, CuttingPlan cuttingPlan, ExportOptions options)
    {
        var infoX = 50;
        var infoY = 50 + cuttingPlan.Stocks.Max(s => s.Height * options.Scale) + 30;
        
        svg.AppendLine($"  <text x=\"{infoX}\" y=\"{infoY}\" font-size=\"14\" font-weight=\"bold\">");
        svg.AppendLine($"    Utilization: {cuttingPlan.UtilizationPercentage:F1}%");
        svg.AppendLine($"  </text>");
        svg.AppendLine($"  <text x=\"{infoX}\" y=\"{infoY + 20}\" font-size=\"12\">");
        svg.AppendLine($"    Pieces placed: {cuttingPlan.PiecesPlaced}");
        svg.AppendLine($"  </text>");
        svg.AppendLine($"  <text x=\"{infoX}\" y=\"{infoY + 35}\" font-size=\"12\">");
        svg.AppendLine($"    Pieces remaining: {cuttingPlan.PiecesRemaining}");
        svg.AppendLine($"  </text>");
    }

    private string GetColorForPiece(string pieceName)
    {
        // Simple hash-based color generation
        var hash = pieceName.GetHashCode();
        var hue = Math.Abs(hash) % 360;
        return $"hsl({hue}, 70%, 80%)";
    }
} 