using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CuttingOptimizer.Domain.Entities;

namespace CuttingOptimizer.Domain.Interfaces;

/// <summary>
/// Interface for heuristic pre-arrangement strategies
/// </summary>
public interface IHeuristicEngine
{
    /// <summary>
    /// Performs heuristic pre-arrangement of pieces
    /// </summary>
    /// <param name="stocks">Available stock materials</param>
    /// <param name="pieces">Pieces to be cut</param>
    /// <param name="settings">Optimization settings</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of pre-arranged pieces</returns>
    Task<List<PlacedPiece>> PreArrangeAsync(
        List<Stock> stocks,
        List<Piece> pieces,
        OptimizationSettings settings,
        CancellationToken cancellationToken = default);
} 