# Cutting Optimizer Pro

A professional-grade cutting optimization software built with C# .NET 9 and Google OR-Tools, designed to achieve OptiCut-level utilization rates (90%+) with advanced hybrid optimization algorithms.

## 🚀 Features

### Core Optimization
- **Hybrid Optimization**: Combines heuristic pre-arrangement with OR-Tools fine optimization
- **High Utilization**: Targets 95%+ material utilization rates
- **Rotation Support**: Full ±90° and ±180° rotation capabilities
- **Multi-threading**: Utilizes all CPU cores for maximum performance
- **Real-time Progress**: Live optimization progress and performance monitoring

### Input/Output Support
- **JSON Import**: Structured data import with validation
- **CSV Import**: Spreadsheet-friendly format support
- **SVG Export**: Vector graphics for high-quality visualization
- **PNG Export**: Raster images for documentation
- **PDF Export**: Professional reports with detailed layouts
- **JSON Export**: Machine-readable optimization results

### Professional GUI
- **Modern WPF Interface**: Clean, professional design
- **2D Visualization**: Real-time cutting plan visualization
- **Interactive Controls**: Zoom, pan, and drag functionality
- **Performance Monitoring**: Real-time CPU and memory usage
- **Progress Tracking**: Detailed optimization progress display

## 🏗️ Architecture

Built following **Clean Architecture** principles with clear separation of concerns:

### Domain Layer
- **Entities**: `Piece`, `Stock`, `PlacedPiece`, `CuttingPlan`
- **Interfaces**: Core business contracts
- **Value Objects**: Optimization settings and progress tracking

### Application Layer
- **Services**: `CuttingOptimizationService` orchestrates the entire process
- **Use Cases**: Business logic and optimization workflows
- **DTOs**: Data transfer objects for cross-layer communication

### Infrastructure Layer
- **OR-Tools Integration**: Google OR-Tools for constraint programming
- **File Handling**: JSON/CSV importers and multi-format exporters
- **Performance Monitoring**: Real-time system resource tracking
- **Heuristic Engine**: Fast pre-arrangement strategies

### Presentation Layer
- **WPF Application**: Modern desktop interface
- **2D Visualization**: Canvas-based cutting plan display
- **Real-time Updates**: Live progress and performance metrics

## 🛠️ Technology Stack

- **.NET 9**: Latest framework with performance improvements
- **Google OR-Tools**: Industry-leading optimization engine
- **WPF**: Rich desktop application framework
- **CsvHelper**: Robust CSV file handling
- **iText7**: Professional PDF generation
- **SOLID Principles**: Clean, maintainable code architecture

## 📦 Installation

### Prerequisites
- .NET 9 SDK
- Visual Studio 2022 or VS Code
- Windows 10/11 (for WPF application)

### Build Instructions

1. **Clone the repository**:
   ```bash
   git clone https://github.com/your-username/cutting-optimizer-pro.git
   cd cutting-optimizer-pro
   ```

2. **Restore dependencies**:
   ```bash
   dotnet restore
   ```

3. **Build the solution**:
   ```bash
   dotnet build
   ```

4. **Run the application**:
   ```bash
   dotnet run --project src/CuttingOptimizer.Presentation
   ```

## 🎯 Usage

### Quick Start

1. **Launch the application**
2. **Import Data**: Click "Import Data" to load stock and piece information
3. **Configure Settings**: Adjust optimization parameters in the settings panel
4. **Optimize**: Click "Optimize" to start the cutting optimization process
5. **View Results**: Monitor real-time progress and view the optimized layout
6. **Export**: Save results in SVG, PNG, PDF, or JSON format

### Input Data Format

#### JSON Format
```json
{
  "stocks": [
    {
      "name": "Plywood Sheet 1",
      "width": 2440,
      "height": 1220,
      "quantity": 2,
      "material": "Plywood",
      "thickness": 18,
      "cost": 25.0
    }
  ],
  "pieces": [
    {
      "name": "Shelf A",
      "width": 600,
      "height": 400,
      "quantity": 4,
      "allowRotation": true,
      "allowedRotations": "All"
    }
  ]
}
```

#### CSV Format
```csv
Name,Width,Height,Quantity,Material,Thickness,Cost
Plywood Sheet 1,2440,1220,2,Plywood,18,25.0
```

### Optimization Settings

- **Enable Rotation**: Allow piece rotation for better utilization
- **Multi-threading**: Utilize all CPU cores for faster optimization
- **Heuristic Pre-arrangement**: Fast initial placement for complex problems
- **Time Limit**: Maximum optimization time in seconds
- **Heuristic Strategy**: Choose from various placement strategies

## 📊 Performance

### Optimization Capabilities
- **Utilization Rate**: 90%+ typical, 95%+ achievable
- **Processing Speed**: Multi-threaded optimization for large datasets
- **Memory Efficiency**: Optimized algorithms for large cutting problems
- **Scalability**: Handles hundreds of pieces and multiple stock sheets

### System Requirements
- **CPU**: Multi-core processor recommended
- **Memory**: 4GB+ RAM for large optimization problems
- **Storage**: Minimal disk space required
- **OS**: Windows 10/11

## 🔧 Development

### Project Structure
```
CuttingOptimizer/
├── src/
│   ├── CuttingOptimizer.Domain/          # Core business logic
│   ├── CuttingOptimizer.Application/     # Application services
│   ├── CuttingOptimizer.Infrastructure/  # External integrations
│   └── CuttingOptimizer.Presentation/    # WPF user interface
├── tests/                                # Unit and integration tests
└── docs/                                 # Documentation
```

### Key Components

#### Domain Layer
- `Piece`: Represents cutting pieces with dimensions and rotation
- `Stock`: Material sheets with properties and constraints
- `PlacedPiece`: Positioned pieces with coordinates and rotation
- `CuttingPlan`: Complete optimization result with metrics

#### Application Layer
- `CuttingOptimizationService`: Main orchestration service
- `OptimizationSettings`: Configuration for optimization algorithms
- `ValidationResult`: Problem validation and feasibility checking

#### Infrastructure Layer
- `OrToolsOptimizationEngine`: Google OR-Tools integration
- `HeuristicEngine`: Fast pre-arrangement strategies
- `PerformanceMonitor`: Real-time system resource tracking
- `CuttingPlanExporter`: Multi-format export capabilities

#### Presentation Layer
- `MainWindow`: Modern WPF interface with real-time visualization
- `Progress Tracking`: Live optimization progress display
- `2D Visualization`: Interactive cutting plan display

### Adding New Features

1. **Domain Changes**: Start with domain entities and interfaces
2. **Application Logic**: Implement business logic in application layer
3. **Infrastructure**: Add external integrations and implementations
4. **Presentation**: Update UI to expose new functionality

## 🧪 Testing

### Unit Tests
```bash
dotnet test tests/CuttingOptimizer.Domain.Tests
dotnet test tests/CuttingOptimizer.Application.Tests
```

### Integration Tests
```bash
dotnet test tests/CuttingOptimizer.Infrastructure.Tests
```

### Performance Tests
```bash
dotnet test tests/CuttingOptimizer.Performance.Tests
```

## 📈 Performance Optimization

### Algorithm Improvements
- **Hybrid Approach**: Combines heuristic and exact optimization
- **Constraint Pruning**: Early elimination of infeasible solutions
- **Symmetry Breaking**: Reduces search space for faster results
- **Multi-threading**: Parallel processing for large problems

### Memory Management
- **Efficient Data Structures**: Optimized for large datasets
- **Lazy Loading**: Load data only when needed
- **Memory Monitoring**: Real-time memory usage tracking
- **Garbage Collection**: Optimized for .NET 9

## 🤝 Contributing

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/amazing-feature`
3. **Commit changes**: `git commit -m 'Add amazing feature'`
4. **Push to branch**: `git push origin feature/amazing-feature`
5. **Open a Pull Request**

### Development Guidelines
- Follow **SOLID** principles
- Maintain **DRY** (Don't Repeat Yourself)
- Apply **YAGNI** (You Aren't Gonna Need It)
- Write comprehensive unit tests
- Document all public APIs

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Google OR-Tools**: Powerful optimization engine
- **Microsoft .NET**: Excellent development platform
- **Open Source Community**: For inspiration and best practices

## 📞 Support

For support, questions, or feature requests:
- **Issues**: [GitHub Issues](https://github.com/your-username/cutting-optimizer-pro/issues)
- **Discussions**: [GitHub Discussions](https://github.com/your-username/cutting-optimizer-pro/discussions)
- **Email**: support@cuttingoptimizer.com

---

**Cutting Optimizer Pro** - Professional-grade cutting optimization software for the modern workshop. 