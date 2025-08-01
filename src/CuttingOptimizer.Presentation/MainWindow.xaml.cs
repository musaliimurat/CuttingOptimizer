using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using CuttingOptimizer.Application.Services;
using CuttingOptimizer.Domain.Entities;
using CuttingOptimizer.Domain.Interfaces;
using CuttingOptimizer.Infrastructure.FileHandling;
using CuttingOptimizer.Infrastructure.Monitoring;
using CuttingOptimizer.Infrastructure.Optimization;
using Microsoft.Win32;

namespace CuttingOptimizer.Presentation;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly CuttingOptimizationService _optimizationService;
    private CuttingPlan? _currentCuttingPlan;
    private CancellationTokenSource? _cancellationTokenSource;

    public MainWindow()
    {
        InitializeComponent();
        
        // Initialize services (in a real app, this would be done via DI container)
        _optimizationService = new CuttingOptimizationService(
            new OrToolsOptimizationEngine(new PerformanceMonitor(), new HeuristicEngine()),
            new JsonFileImporter(), // You could create a composite importer that supports multiple formats
            new CuttingPlanExporter(),
            new PerformanceMonitor()
        );
    }

    private async void ImportData_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Cutting Data File",
                Filter = "JSON files (*.json)|*.json|CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                StatusTextBlock.Text = "Importing data...";
                
                // For simplicity, we'll create sample data
                // In a real implementation, you would import from the selected file
                var stocks = CreateSampleStocks();
                var pieces = CreateSamplePieces();
                
                // Validate the data
                var validation = _optimizationService.ValidateProblem(stocks, pieces);
                if (!validation.IsValid)
                {
                    MessageBox.Show($"Validation failed:\n{string.Join("\n", validation.Errors)}", 
                                  "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                StatusTextBlock.Text = $"Data imported successfully. {stocks.Count} stocks, {pieces.Count} piece types.";
                LogMessage($"Imported {stocks.Count} stocks and {pieces.Count} piece types");
                
                // Store the data for optimization
                _currentCuttingPlan = new CuttingPlan
                {
                    Stocks = stocks,
                    Pieces = pieces
                };
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error importing data: {ex.Message}", "Import Error", 
                          MessageBoxButton.OK, MessageBoxImage.Error);
            LogMessage($"Import error: {ex.Message}");
        }
    }

    private async void Optimize_Click(object sender, RoutedEventArgs e)
    {
        if (_currentCuttingPlan?.Stocks == null || _currentCuttingPlan?.Pieces == null)
        {
            MessageBox.Show("Please import data first.", "No Data", 
                          MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            // Create optimization settings from UI
            var settings = CreateOptimizationSettings();
            
            // Create progress callback
            var progress = new Progress<OptimizationProgress>(UpdateProgress);
            
            // Create cancellation token
            _cancellationTokenSource = new CancellationTokenSource();
            
            // Update UI
            StatusTextBlock.Text = "Optimizing...";
            OptimizationProgressBar.Visibility = Visibility.Visible;
            OptimizationProgressBar.Value = 0;
            
            // Perform optimization
            _currentCuttingPlan = await _optimizationService.OptimizeAsync(
                _currentCuttingPlan.Stocks,
                _currentCuttingPlan.Pieces,
                settings,
                _cancellationTokenSource.Token,
                progress
            );
            
            // Update results
            UpdateResults(_currentCuttingPlan);
            UpdateVisualization(_currentCuttingPlan);
            
            StatusTextBlock.Text = "Optimization completed successfully!";
            OptimizationProgressBar.Visibility = Visibility.Collapsed;
            
            LogMessage($"Optimization completed. Utilization: {_currentCuttingPlan.UtilizationPercentage:F1}%");
        }
        catch (OperationCanceledException)
        {
            StatusTextBlock.Text = "Optimization cancelled.";
            OptimizationProgressBar.Visibility = Visibility.Collapsed;
            LogMessage("Optimization cancelled by user");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error during optimization: {ex.Message}", "Optimization Error", 
                          MessageBoxButton.OK, MessageBoxImage.Error);
            StatusTextBlock.Text = "Optimization failed.";
            OptimizationProgressBar.Visibility = Visibility.Collapsed;
            LogMessage($"Optimization error: {ex.Message}");
        }
    }

    private async void Export_Click(object sender, RoutedEventArgs e)
    {
        if (_currentCuttingPlan == null)
        {
            MessageBox.Show("No cutting plan to export.", "No Data", 
                          MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Export Cutting Plan",
                Filter = "SVG files (*.svg)|*.svg|PNG files (*.png)|*.png|PDF files (*.pdf)|*.pdf|JSON files (*.json)|*.json",
                DefaultExt = "svg"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var format = GetExportFormat(saveFileDialog.FilterIndex);
                var options = CreateExportOptions();
                
                await _optimizationService.ExportCuttingPlanAsync(
                    _currentCuttingPlan,
                    saveFileDialog.FileName,
                    format,
                    options
                );
                
                MessageBox.Show("Export completed successfully!", "Export Success", 
                              MessageBoxButton.OK, MessageBoxImage.Information);
                LogMessage($"Exported cutting plan to {saveFileDialog.FileName}");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error during export: {ex.Message}", "Export Error", 
                          MessageBoxButton.OK, MessageBoxImage.Error);
            LogMessage($"Export error: {ex.Message}");
        }
    }

    private OptimizationSettings CreateOptimizationSettings()
    {
        return new OptimizationSettings
        {
            EnableRotation = EnableRotationCheckBox.IsChecked ?? true,
            EnableMultiThreading = EnableMultiThreadingCheckBox.IsChecked ?? true,
            EnableHeuristicPreArrangement = EnableHeuristicCheckBox.IsChecked ?? true,
            TimeLimitSeconds = int.TryParse(TimeLimitTextBox.Text, out var timeLimit) ? timeLimit : 300,
            MaxThreads = int.TryParse(MaxThreadsTextBox.Text, out var maxThreads) ? maxThreads : Environment.ProcessorCount,
            HeuristicStrategy = GetHeuristicStrategy(),
            EnablePerformanceMonitoring = EnablePerformanceMonitoringCheckBox.IsChecked ?? true,
            EnableDetailedLogging = EnableDetailedLoggingCheckBox.IsChecked ?? false,
            GenerateVisualization = GenerateVisualizationCheckBox.IsChecked ?? true,
            GenerateReport = GenerateReportCheckBox.IsChecked ?? true,
            ExportFormat = GetExportFormat(ExportFormatComboBox.SelectedIndex)
        };
    }

    private HeuristicStrategy GetHeuristicStrategy()
    {
        return HeuristicStrategyComboBox.SelectedIndex switch
        {
            0 => HeuristicStrategy.LargestFirst,
            1 => HeuristicStrategy.SmallestFirst,
            2 => HeuristicStrategy.AreaDescending,
            3 => HeuristicStrategy.AreaAscending,
            4 => HeuristicStrategy.PerimeterDescending,
            5 => HeuristicStrategy.PerimeterAscending,
            6 => HeuristicStrategy.Random,
            _ => HeuristicStrategy.LargestFirst
        };
    }

    private ExportFormat GetExportFormat(int index)
    {
        return index switch
        {
            0 => ExportFormat.SVG,
            1 => ExportFormat.PNG,
            2 => ExportFormat.PDF,
            3 => ExportFormat.JSON,
            _ => ExportFormat.SVG
        };
    }

    private ExportOptions CreateExportOptions()
    {
        return new ExportOptions
        {
            ShowGrid = true,
            ShowLabels = true,
            ShowRotations = true,
            ShowUtilization = true,
            IncludeMetadata = true,
            Title = "Cutting Optimizer Pro - Optimization Result",
            Description = $"Generated on {DateTime.Now:yyyy-MM-dd HH:mm:ss}"
        };
    }

    private void UpdateProgress(OptimizationProgress progress)
    {
        Dispatcher.Invoke(() =>
        {
            OptimizationProgressBar.Value = progress.ProgressPercentage;
            StatusTextBlock.Text = progress.PhaseDescription;
            UtilizationTextBlock.Text = $"Utilization: {progress.CurrentUtilization:F1}%";
            PiecesTextBlock.Text = $"Pieces: {progress.PiecesPlaced}/{progress.TotalPieces}";
            
            LogMessage($"Phase {progress.CurrentPhase}/{progress.TotalPhases}: {progress.PhaseDescription} - {progress.ProgressPercentage:F1}%");
        });
    }

    private void UpdateResults(CuttingPlan cuttingPlan)
    {
        ResultsStackPanel.Children.Clear();
        
        var results = new[]
        {
            $"Utilization: {cuttingPlan.UtilizationPercentage:F1}%",
            $"Pieces Placed: {cuttingPlan.PiecesPlaced}",
            $"Pieces Remaining: {cuttingPlan.PiecesRemaining}",
            $"Optimization Time: {cuttingPlan.OptimizationTime}",
            $"Memory Usage: {cuttingPlan.MemoryUsageMB:F1} MB",
            $"CPU Usage: {cuttingPlan.CpuUsagePercentage:F1}%"
        };
        
        foreach (var result in results)
        {
            ResultsStackPanel.Children.Add(new TextBlock
            {
                Text = result,
                Margin = new Thickness(0, 2, 0, 2),
                FontSize = 12
            });
        }
    }

    private void UpdateVisualization(CuttingPlan cuttingPlan)
    {
        VisualizationCanvas.Children.Clear();
        
        if (cuttingPlan.PlacedPieces.Count == 0)
            return;
        
        var scale = 2.0; // Scale factor for visualization
        var offsetX = 50.0;
        var offsetY = 50.0;
        
        foreach (var stock in cuttingPlan.Stocks)
        {
            var stockPieces = cuttingPlan.PlacedPieces.Where(p => p.StockId == stock.Id).ToList();
            
            // Draw stock outline
            var stockRect = new Rectangle
            {
                Width = stock.Width * scale,
                Height = stock.Height * scale,
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                Fill = Brushes.Transparent
            };
            
            Canvas.SetLeft(stockRect, offsetX);
            Canvas.SetTop(stockRect, offsetY);
            VisualizationCanvas.Children.Add(stockRect);
            
            // Draw pieces
            foreach (var piece in stockPieces)
            {
                var pieceRect = new Rectangle
                {
                    Width = piece.EffectiveWidth * scale,
                    Height = piece.EffectiveHeight * scale,
                    Stroke = Brushes.Blue,
                    StrokeThickness = 1,
                    Fill = GetPieceBrush(piece.Piece.Name)
                };
                
                Canvas.SetLeft(pieceRect, offsetX + piece.X * scale);
                Canvas.SetTop(pieceRect, offsetY + piece.Y * scale);
                VisualizationCanvas.Children.Add(pieceRect);
                
                // Add piece label
                var label = new TextBlock
                {
                    Text = piece.Piece.Name,
                    FontSize = 10,
                    Foreground = Brushes.Black,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                
                Canvas.SetLeft(label, offsetX + piece.X * scale + piece.EffectiveWidth * scale / 2);
                Canvas.SetTop(label, offsetY + piece.Y * scale + piece.EffectiveHeight * scale / 2);
                VisualizationCanvas.Children.Add(label);
            }
            
            offsetY += stock.Height * scale + 20;
        }
        
        // Update canvas size
        VisualizationCanvas.Width = cuttingPlan.Stocks.Max(s => s.Width) * scale + 100;
        VisualizationCanvas.Height = offsetY;
    }

    private Brush GetPieceBrush(string pieceName)
    {
        var colors = new[]
        {
            Brushes.LightBlue,
            Brushes.LightGreen,
            Brushes.LightYellow,
            Brushes.LightPink,
            Brushes.LightCoral,
            Brushes.LightCyan,
            Brushes.LightGray,
            Brushes.LightSalmon
        };
        
        var index = Math.Abs(pieceName.GetHashCode()) % colors.Length;
        return colors[index];
    }

    private void LogMessage(string message)
    {
        Dispatcher.Invoke(() =>
        {
            var logEntry = new TextBlock
            {
                Text = $"[{DateTime.Now:HH:mm:ss}] {message}",
                FontSize = 11,
                Margin = new Thickness(0, 1, 0, 1)
            };
            
            LogsStackPanel.Children.Add(logEntry);
            
            // Keep only last 50 log entries
            while (LogsStackPanel.Children.Count > 50)
            {
                LogsStackPanel.Children.RemoveAt(0);
            }
        });
    }

    private List<Stock> CreateSampleStocks()
    {
        return new List<Stock>
        {
            new Stock("Plywood Sheet 1", 2440, 1220, 2) { Material = "Plywood", Thickness = 18, Cost = 25.0 },
            new Stock("Plywood Sheet 2", 2440, 1220, 1) { Material = "Plywood", Thickness = 18, Cost = 25.0 }
        };
    }

    private List<Piece> CreateSamplePieces()
    {
        return new List<Piece>
        {
            new Piece("Shelf A", 600, 400, 4),
            new Piece("Shelf B", 800, 300, 2),
            new Piece("Side Panel", 800, 600, 4),
            new Piece("Back Panel", 600, 800, 2),
            new Piece("Drawer Front", 400, 200, 6),
            new Piece("Drawer Side", 400, 150, 12),
            new Piece("Drawer Bottom", 380, 380, 6)
        };
    }
}