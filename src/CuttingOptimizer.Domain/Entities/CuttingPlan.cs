using System;
using System.Collections.Generic;
using System.Linq;

namespace CuttingOptimizer.Domain.Entities;

/// <summary>
/// Represents a complete cutting plan with placed pieces and optimization results
/// </summary>
public class CuttingPlan
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<Stock> Stocks { get; set; } = new();
    public List<Piece> Pieces { get; set; } = new();
    public List<PlacedPiece> PlacedPieces { get; set; } = new();
    public OptimizationSettings Settings { get; set; } = new();
    
    // Performance metrics
    public double TotalStockArea { get; set; }
    public double TotalPieceArea { get; set; }
    public double UtilizationPercentage { get; set; }
    public int PiecesPlaced { get; set; }
    public int PiecesRemaining { get; set; }
    public TimeSpan OptimizationTime { get; set; }
    public double MemoryUsageMB { get; set; }
    public double CpuUsagePercentage { get; set; }
    
    public CuttingPlan()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Calculates the utilization percentage
    /// </summary>
    public void CalculateUtilization()
    {
        TotalStockArea = Stocks.Sum(s => s.TotalArea);
        TotalPieceArea = PlacedPieces.Sum(p => p.Area);
        UtilizationPercentage = TotalStockArea > 0 ? (TotalPieceArea / TotalStockArea) * 100 : 0;
    }
    
    /// <summary>
    /// Gets all pieces that were successfully placed
    /// </summary>
    public List<Piece> GetPlacedPieces()
    {
        return PlacedPieces.Select(p => p.Piece).ToList();
    }
    
    /// <summary>
    /// Gets all pieces that could not be placed
    /// </summary>
    public List<Piece> GetUnplacedPieces()
    {
        var placedPieceIds = PlacedPieces.Select(p => p.Piece.Id).ToHashSet();
        return Pieces.Where(p => !placedPieceIds.Contains(p.Id)).ToList();
    }
    
    /// <summary>
    /// Gets placed pieces grouped by stock
    /// </summary>
    public Dictionary<Guid, List<PlacedPiece>> GetPiecesByStock()
    {
        return PlacedPieces.GroupBy(p => p.StockId)
                          .ToDictionary(g => g.Key, g => g.ToList());
    }
    
    /// <summary>
    /// Validates the cutting plan for overlaps and boundary violations
    /// </summary>
    public List<string> Validate()
    {
        var errors = new List<string>();
        
        foreach (var stock in Stocks)
        {
            var stockPieces = PlacedPieces.Where(p => p.StockId == stock.Id).ToList();
            
            // Check for overlaps
            for (int i = 0; i < stockPieces.Count; i++)
            {
                for (int j = i + 1; j < stockPieces.Count; j++)
                {
                    if (stockPieces[i].OverlapsWith(stockPieces[j]))
                    {
                        errors.Add($"Pieces '{stockPieces[i].Piece.Name}' and '{stockPieces[j].Piece.Name}' overlap on stock '{stock.Name}'");
                    }
                }
            }
            
            // Check for boundary violations
            foreach (var piece in stockPieces)
            {
                if (!piece.FitsInStock(stock.Width, stock.Height))
                {
                    errors.Add($"Piece '{piece.Piece.Name}' extends beyond stock '{stock.Name}' boundaries");
                }
            }
        }
        
        return errors;
    }
    
    /// <summary>
    /// Creates a copy of this cutting plan
    /// </summary>
    public CuttingPlan Clone()
    {
        return new CuttingPlan
        {
            Id = Id,
            Name = Name,
            CreatedAt = CreatedAt,
            Stocks = Stocks.Select(s => s.Clone()).ToList(),
            Pieces = Pieces.Select(p => p.Clone()).ToList(),
            PlacedPieces = PlacedPieces.Select(p => p.Clone()).ToList(),
            Settings = Settings.Clone(),
            TotalStockArea = TotalStockArea,
            TotalPieceArea = TotalPieceArea,
            UtilizationPercentage = UtilizationPercentage,
            PiecesPlaced = PiecesPlaced,
            PiecesRemaining = PiecesRemaining,
            OptimizationTime = OptimizationTime,
            MemoryUsageMB = MemoryUsageMB,
            CpuUsagePercentage = CpuUsagePercentage
        };
    }
} 