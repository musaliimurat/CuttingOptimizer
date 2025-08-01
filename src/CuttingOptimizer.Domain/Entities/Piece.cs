using System;

namespace CuttingOptimizer.Domain.Entities;

/// <summary>
/// Represents a piece to be cut from the stock material
/// </summary>
public class Piece
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Width { get; set; }
    public double Height { get; set; }
    public int Quantity { get; set; }
    public bool AllowRotation { get; set; } = true;
    public RotationType AllowedRotations { get; set; } = RotationType.All;
    
    /// <summary>
    /// Current rotation angle in degrees (0, 90, 180, 270)
    /// </summary>
    public int RotationAngle { get; set; }
    
    /// <summary>
    /// Gets the effective width after rotation
    /// </summary>
    public double EffectiveWidth => RotationAngle == 90 || RotationAngle == 270 ? Height : Width;
    
    /// <summary>
    /// Gets the effective height after rotation
    /// </summary>
    public double EffectiveHeight => RotationAngle == 90 || RotationAngle == 270 ? Width : Height;
    
    /// <summary>
    /// Gets the area of the piece
    /// </summary>
    public double Area => Width * Height;
    
    /// <summary>
    /// Gets the effective area after rotation
    /// </summary>
    public double EffectiveArea => EffectiveWidth * EffectiveHeight;
    
    public Piece()
    {
        Id = Guid.NewGuid();
    }
    
    public Piece(string name, double width, double height, int quantity = 1)
        : this()
    {
        Name = name;
        Width = width;
        Height = height;
        Quantity = quantity;
    }
    
    /// <summary>
    /// Rotates the piece by the specified angle
    /// </summary>
    public void Rotate(int angle)
    {
        if (!AllowRotation) return;
        
        var normalizedAngle = angle % 360;
        if (normalizedAngle < 0) normalizedAngle += 360;
        
        // Only allow specific rotation angles
        if (normalizedAngle != 0 && normalizedAngle != 90 && normalizedAngle != 180 && normalizedAngle != 270)
            return;
            
        // Check if this rotation is allowed
        var rotationType = normalizedAngle switch
        {
            0 => RotationType.None,
            90 => RotationType.Rotate90,
            180 => RotationType.Rotate180,
            270 => RotationType.Rotate270,
            _ => RotationType.None
        };
        
        if ((AllowedRotations & rotationType) == rotationType)
        {
            RotationAngle = normalizedAngle;
        }
    }
    
    /// <summary>
    /// Creates a copy of this piece
    /// </summary>
    public Piece Clone()
    {
        return new Piece
        {
            Id = Id,
            Name = Name,
            Width = Width,
            Height = Height,
            Quantity = Quantity,
            AllowRotation = AllowRotation,
            AllowedRotations = AllowedRotations,
            RotationAngle = RotationAngle
        };
    }
}

/// <summary>
/// Defines the types of rotations allowed for a piece
/// </summary>
[Flags]
public enum RotationType
{
    None = 0,
    Rotate90 = 1,
    Rotate180 = 2,
    Rotate270 = 4,
    All = Rotate90 | Rotate180 | Rotate270
} 