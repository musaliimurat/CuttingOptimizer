namespace CuttingOptimizer.Domain.Entities;

/// <summary>
/// Represents a stock material (sheet, board, etc.) to cut pieces from
/// </summary>
public class Stock
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Width { get; set; }
    public double Height { get; set; }
    public int Quantity { get; set; } = 1;
    public string Material { get; set; } = string.Empty;
    public double Thickness { get; set; }
    public double Cost { get; set; }
    
    /// <summary>
    /// Gets the total area of the stock
    /// </summary>
    public double Area => Width * Height;
    
    /// <summary>
    /// Gets the total area available for all stock pieces
    /// </summary>
    public double TotalArea => Area * Quantity;
    
    public Stock()
    {
        Id = Guid.NewGuid();
    }
    
    public Stock(string name, double width, double height, int quantity = 1)
        : this()
    {
        Name = name;
        Width = width;
        Height = height;
        Quantity = quantity;
    }
    
    /// <summary>
    /// Creates a copy of this stock
    /// </summary>
    public Stock Clone()
    {
        return new Stock
        {
            Id = Id,
            Name = Name,
            Width = Width,
            Height = Height,
            Quantity = Quantity,
            Material = Material,
            Thickness = Thickness,
            Cost = Cost
        };
    }
} 