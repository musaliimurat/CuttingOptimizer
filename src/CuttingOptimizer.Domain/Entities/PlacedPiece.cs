using System;

namespace CuttingOptimizer.Domain.Entities;

/// <summary>
/// Represents a piece that has been placed on a stock at a specific position
/// </summary>
public class PlacedPiece
{
    public Guid Id { get; set; }
    public Piece Piece { get; set; } = null!;
    public double X { get; set; }
    public double Y { get; set; }
    public int RotationAngle { get; set; }
    public Guid StockId { get; set; }
    
    /// <summary>
    /// Gets the effective width after rotation
    /// </summary>
    public double EffectiveWidth => RotationAngle == 90 || RotationAngle == 270 ? Piece.Height : Piece.Width;
    
    /// <summary>
    /// Gets the effective height after rotation
    /// </summary>
    public double EffectiveHeight => RotationAngle == 90 || RotationAngle == 270 ? Piece.Width : Piece.Height;
    
    /// <summary>
    /// Gets the right edge X coordinate
    /// </summary>
    public double Right => X + EffectiveWidth;
    
    /// <summary>
    /// Gets the bottom edge Y coordinate
    /// </summary>
    public double Bottom => Y + EffectiveHeight;
    
    /// <summary>
    /// Gets the area of the placed piece
    /// </summary>
    public double Area => EffectiveWidth * EffectiveHeight;
    
    public PlacedPiece()
    {
        Id = Guid.NewGuid();
    }
    
    public PlacedPiece(Piece piece, double x, double y, int rotationAngle = 0, Guid stockId = default)
        : this()
    {
        Piece = piece;
        X = x;
        Y = y;
        RotationAngle = rotationAngle;
        StockId = stockId;
    }
    
    /// <summary>
    /// Checks if this piece overlaps with another placed piece
    /// </summary>
    public bool OverlapsWith(PlacedPiece other)
    {
        return X < other.Right && Right > other.X && Y < other.Bottom && Bottom > other.Y;
    }
    
    /// <summary>
    /// Checks if this piece fits within the given stock dimensions
    /// </summary>
    public bool FitsInStock(double stockWidth, double stockHeight)
    {
        return X >= 0 && Y >= 0 && Right <= stockWidth && Bottom <= stockHeight;
    }
    
    /// <summary>
    /// Creates a copy of this placed piece
    /// </summary>
    public PlacedPiece Clone()
    {
        return new PlacedPiece
        {
            Id = Id,
            Piece = Piece.Clone(),
            X = X,
            Y = Y,
            RotationAngle = RotationAngle,
            StockId = StockId
        };
    }
} 