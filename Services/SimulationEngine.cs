using SimulationEvolucion.Core.Enums;
using SimulationEvolucion.Core.Interfaces;
using SimulationEvolucion.Core.Models;

namespace SimulationEvolucion.Services;

/// <summary>
/// Motor de simulación de deriva genética
/// </summary>
public class SimulationEngine : ISimulationEngine
{
    public IWorld1D World { get; private set; }
    public SimulationConfig Config { get; private set; }
    public List<PopulationStatistics> History { get; private set; }
    public OptimizedFossilManager? FossilManager { get; private set; }
    public CladogramBuilder? CladogramBuilder { get; private set; }
    
    private Random _random;
    private int _currentGeneration;
    
    public SimulationEngine(SimulationConfig config, int? seed = null)
    {
        Config = config;
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
        History = new List<PopulationStatistics>();
        _currentGeneration = 0;
        
        World = new World1D(
            config.WorldSize,
            config.MutationRate,
            config.SelectionStrength,
            config.CarryingCapacity
        );
        
        // Initialize optimized fossil manager if fossil record is enabled
        if (config.EnableFossilRecord)
        {
            FossilManager = new OptimizedFossilManager(config.FossilizationProbability, config.FossilHalfLife);
            World.FossilManager = FossilManager;
        }
        
        // Initialize cladogram builder
        CladogramBuilder = new CladogramBuilder(seed);
    }
    
    public void Initialize(Random? random = null)
    {
        var rng = random ?? _random;
        
        // Crear población inicial con genes aleatorios por organismo
        for (int i = 0; i < Config.InitialPopulationSize; i++)
        {
            var position = rng.Next(Config.WorldSize);
            var organism = new Organism(position, rng, Config.GeneCount, Config.GeneLength, initialSequences: null);
            
            World.AddOrganism(organism);
        }
        
        // Registrar estadísticas iniciales
        var initialStats = World.GetStatistics();
        History.Add(initialStats);
        
        if (Config.LogProgress)
        {
            Console.WriteLine($"Simulación inicializada: {initialStats.TotalOrganisms} organismos");
            Console.WriteLine($"Genes iniciales generados aleatoriamente por organismo (secuencias distintas).");
            LogStatistics(0, initialStats);
        }
    }
    
    
    public void RunSimulation(int generations)
    {
        Console.WriteLine($"Starting simulation of {generations} generations...");
        
        for (int generation = 1; generation <= generations; generation++)
        {
            RunGeneration();
            
            // Only calculate expensive statistics when needed for logging
            if (Config.LogProgress && generation % Config.LogInterval == 0)
            {
                var stats = GetCurrentStatistics();
                LogStatistics(generation, stats);
            }
            
            // Log every 5 generations for growth monitoring (lightweight stats only)
            if (generation % 5 == 0)
            {
                var stats = GetCurrentStatistics();
                Console.WriteLine($"Gen {generation}: {stats.TotalOrganisms} organisms, " +
                                $"diversidad neutral: {stats.GeneDiversity.GetValueOrDefault("neutral", 0):F3}, " +
                                $"diversidad seleccionada: {stats.GeneDiversity.GetValueOrDefault("selected", 0):F3}");
            }
        }
        
        Console.WriteLine("\nSimulation completed!");
        
        // Apply fossil decay if fossil record is enabled
        if (Config.EnableFossilRecord && FossilManager != null)
        {
            Console.WriteLine("Applying final fossil damage...");
            FossilManager.ApplyFinalDamage(_currentGeneration);
            Console.WriteLine($"Fossils after final damage: {FossilManager.TotalFossils}");
        }
    }
    
    public void RunGeneration()
    {
        _currentGeneration++;
        World.CurrentGeneration = _currentGeneration;
        World.Evolve(_random);
        
        // Only calculate full statistics when needed (for history)
        // Use lightweight stats for frequent monitoring
        var stats = GetLightweightStatistics();
        History.Add(stats);
    }
    
    public PopulationStatistics GetCurrentStatistics()
    {
        return World.GetStatistics();
    }
    
    /// <summary>
    /// Get lightweight statistics for frequent monitoring (without expensive diversity calculations)
    /// </summary>
    public PopulationStatistics GetLightweightStatistics()
    {
        var stats = new PopulationStatistics
        {
            TotalOrganisms = World.Organisms.Count
        };
        
        if (World.Organisms.Any())
        {
            var fitnesses = World.Organisms.Select(o => o.CalculateFitness()).ToList();
            stats.AverageFitness = fitnesses.Average();
            stats.FitnessVariance = fitnesses.Variance();
            
            // Count genes by type (fast operations)
            stats.SelectedGenesCount = World.Organisms.SelectMany(o => o.GetGenesByType(GeneType.Selected)).Count();
            stats.NeutralGenesCount = World.Organisms.SelectMany(o => o.GetGenesByType(GeneType.Neutral)).Count();
            
            // Skip expensive diversity calculations for lightweight stats
        }
        
        return stats;
    }
    
    private void LogStatistics(int generation, PopulationStatistics stats)
    {
        Console.WriteLine($"Generación {generation}:");
        Console.WriteLine($"  Organismos: {stats.TotalOrganisms}");
        Console.WriteLine($"  Fitness promedio: {stats.AverageFitness:F3}");
        Console.WriteLine($"  Varianza fitness: {stats.FitnessVariance:F3}");
        Console.WriteLine($"  Genes seleccionados: {stats.SelectedGenesCount}");
        Console.WriteLine($"  Genes neutrales: {stats.NeutralGenesCount}");
        
        if (stats.GeneDiversity.ContainsKey("neutral"))
        {
            Console.WriteLine($"  Diversidad genes neutrales: {stats.GeneDiversity["neutral"]:F3}");
        }
        
        if (stats.GeneDiversity.ContainsKey("selected"))
        {
            Console.WriteLine($"  Diversidad genes seleccionados: {stats.GeneDiversity["selected"]:F3}");
        }
        
        Console.WriteLine();
    }
    
    public void ExportResults(string filePath)
    {
        // Asegurar que la carpeta existe
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var writer = new StreamWriter(filePath);
        
        // Escribir encabezado
        writer.WriteLine("Generation,TotalOrganisms,AverageFitness,FitnessVariance,SelectedGenes,NeutralGenes,NeutralDiversity,SelectedDiversity");
        
        // Escribir datos
        for (int i = 0; i < History.Count; i++)
        {
            var stats = History[i];
            var neutralDiversity = stats.GeneDiversity.ContainsKey("neutral") ? stats.GeneDiversity["neutral"] : 0.0;
            var selectedDiversity = stats.GeneDiversity.ContainsKey("selected") ? stats.GeneDiversity["selected"] : 0.0;
            
            writer.WriteLine($"{i},{stats.TotalOrganisms},{stats.AverageFitness:F6},{stats.FitnessVariance:F6}," +
                           $"{stats.SelectedGenesCount},{stats.NeutralGenesCount},{neutralDiversity:F6},{selectedDiversity:F6}");
        }
        
        Console.WriteLine($"✅ Resultados exportados a {filePath}");
    }
    
    public void AnalyzeGeneticDrift()
    {
        Console.WriteLine("=== ANÁLISIS DE DERIVA GENÉTICA ===");
        Console.WriteLine($"Total de generaciones simuladas: {History.Count - 1}");
        
        if (History.Count < 2)
        {
            Console.WriteLine("No hay suficientes generaciones para analizar deriva genética.");
            return;
        }
        
        var initialStats = History[0];
        var finalStats = History[^1];
        
        Console.WriteLine($"Diversidad inicial genes neutrales: {initialStats.GeneDiversity.GetValueOrDefault("neutral", 0):F3}");
        Console.WriteLine($"Diversidad final genes neutrales: {finalStats.GeneDiversity.GetValueOrDefault("neutral", 0):F3}");
        
        var neutralChange = finalStats.GeneDiversity.GetValueOrDefault("neutral", 0) - initialStats.GeneDiversity.GetValueOrDefault("neutral", 0);
        Console.WriteLine($"Cambio en diversidad neutral: {neutralChange:F3}");
        
        Console.WriteLine($"Diversidad inicial genes seleccionados: {initialStats.GeneDiversity.GetValueOrDefault("selected", 0):F3}");
        Console.WriteLine($"Diversidad final genes seleccionados: {finalStats.GeneDiversity.GetValueOrDefault("selected", 0):F3}");
        
        var selectedChange = finalStats.GeneDiversity.GetValueOrDefault("selected", 0) - initialStats.GeneDiversity.GetValueOrDefault("selected", 0);
        Console.WriteLine($"Cambio en diversidad seleccionados: {selectedChange:F3}");
        
        Console.WriteLine($"Población inicial: {initialStats.TotalOrganisms}");
        Console.WriteLine($"Población final: {finalStats.TotalOrganisms}");
        
        // Analizar tendencias
        AnalyzeTrends();
    }
    
    private void AnalyzeTrends()
    {
        if (History.Count < 10) return;
        
        var neutralDiversityValues = History.Select(h => h.GeneDiversity.GetValueOrDefault("neutral", 0)).ToList();
        var selectedDiversityValues = History.Select(h => h.GeneDiversity.GetValueOrDefault("selected", 0)).ToList();
        
        // Calcular correlación entre diversidad neutral y seleccionada
        var correlation = CalculateCorrelation(neutralDiversityValues, selectedDiversityValues);
        
        Console.WriteLine($"Correlación diversidad neutral vs seleccionada: {correlation:F3}");
        
        if (correlation < 0.5)
        {
            Console.WriteLine("Los genes neutrales muestran deriva genética independiente de la selección natural.");
        }
        else
        {
            Console.WriteLine("Los genes neutrales están correlacionados con la selección, posiblemente debido a linkage.");
        }
    }
    
    private double CalculateCorrelation(List<double> x, List<double> y)
    {
        if (x.Count != y.Count || x.Count == 0) return 0;
        
        var n = x.Count;
        var sumX = x.Sum();
        var sumY = y.Sum();
        var sumXY = x.Zip(y, (xi, yi) => xi * yi).Sum();
        var sumX2 = x.Sum(xi => xi * xi);
        var sumY2 = y.Sum(yi => yi * yi);
        
        var numerator = n * sumXY - sumX * sumY;
        var denominator = Math.Sqrt((n * sumX2 - sumX * sumX) * (n * sumY2 - sumY * sumY));
        
        return denominator == 0 ? 0 : numerator / denominator;
    }
    
    /// <summary>
    /// Exports fossil record to JSON file
    /// </summary>
    public void ExportFossils(string filePath)
    {
        if (Config.EnableFossilRecord && FossilManager != null)
        {
            // Asegurar que la carpeta existe
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            FossilManager.SaveToJson(filePath);
            Console.WriteLine($"✅ Registro fósil exportado a: {filePath}");
        }
    }
    
    /// <summary>
    /// Gets fossil statistics
    /// </summary>
    public OptimizedFossilStatistics? GetFossilStatistics()
    {
        if (Config.EnableFossilRecord && FossilManager != null)
        {
            return FossilManager.GetStatistics();
        }
        return null;
    }
    
    /// <summary>
    /// Builds a cladogram from current living organisms and fossils
    /// </summary>
    public Cladogram BuildCladogram()
    {
        if (CladogramBuilder == null)
        {
            throw new InvalidOperationException("CladogramBuilder not initialized");
        }
        
        var livingOrganisms = World.Organisms.ToList();
        var fossils = FossilManager?.GetAllFossils() ?? new List<LazyFossil>();
        
        return CladogramBuilder.BuildCladogram(livingOrganisms, fossils, _currentGeneration);
    }
    
    /// <summary>
    /// Builds a cladogram from only living organisms
    /// </summary>
    public Cladogram BuildLivingOrganismsCladogram()
    {
        if (CladogramBuilder == null)
        {
            throw new InvalidOperationException("CladogramBuilder not initialized");
        }
        
        var livingOrganisms = World.Organisms.ToList();
        return CladogramBuilder.BuildLivingOrganismsCladogram(livingOrganisms, _currentGeneration);
    }
    
    /// <summary>
    /// Builds a cladogram from only fossils
    /// </summary>
    public Cladogram BuildFossilsCladogram()
    {
        if (CladogramBuilder == null)
        {
            throw new InvalidOperationException("CladogramBuilder not initialized");
        }
        
        var fossils = FossilManager?.GetAllFossils() ?? new List<LazyFossil>();
        return CladogramBuilder.BuildFossilsCladogram(fossils);
    }
    
    /// <summary>
    /// Exports cladogram to text file
    /// </summary>
    public void ExportCladogramToText(string filePath)
    {
        // Asegurar que la carpeta existe
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        var cladogram = BuildCladogram();
        var text = cladogram.ExportToText();
        
        File.WriteAllText(filePath, text);
        Console.WriteLine($"✅ Cladograma exportado a: {filePath}");
    }
    
    /// <summary>
    /// Exports cladogram to Newick format file
    /// </summary>
    public void ExportCladogramToNewick(string filePath)
    {
        // Asegurar que la carpeta existe
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        var cladogram = BuildCladogram();
        var newick = cladogram.ExportToNewick();
        
        File.WriteAllText(filePath, newick);
        Console.WriteLine($"✅ Cladograma en formato Newick exportado a: {filePath}");
    }
    
    /// <summary>
    /// Exports living organisms cladogram to text file
    /// </summary>
    public void ExportLivingOrganismsCladogramToText(string filePath)
    {
        // Asegurar que la carpeta existe
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        var cladogram = BuildLivingOrganismsCladogram();
        var text = cladogram.ExportToText();
        
        File.WriteAllText(filePath, text);
        Console.WriteLine($"✅ Cladograma de organismos vivos exportado a: {filePath}");
    }
    
    /// <summary>
    /// Exports fossils cladogram to text file
    /// </summary>
    public void ExportFossilsCladogramToText(string filePath)
    {
        // Asegurar que la carpeta existe
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        var cladogram = BuildFossilsCladogram();
        var text = cladogram.ExportToText();
        
        File.WriteAllText(filePath, text);
        Console.WriteLine($"✅ Cladograma de fósiles exportado a: {filePath}");
    }
    
    /// <summary>
    /// Analyzes and displays cladogram statistics
    /// </summary>
    public void AnalyzeCladogram()
    {
        Console.WriteLine("=== ANÁLISIS DEL CLADOGRAMA ===");
        
        try
        {
            var cladogram = BuildCladogram();
            var stats = cladogram.GetStatistics();
            
            Console.WriteLine($"Total nodos: {stats.TotalNodes}");
            Console.WriteLine($"Total hojas: {stats.TotalLeaves}");
            Console.WriteLine($"  - Fósiles: {stats.TotalFossils}");
            Console.WriteLine($"  - Organismos vivos: {stats.TotalLiving}");
            Console.WriteLine($"Profundidad del árbol: {stats.TreeDepth}");
            Console.WriteLine($"Longitud total del árbol: {stats.TreeLength:F3}");
            
            if (stats.MinGeneration.HasValue && stats.MaxGeneration.HasValue)
            {
                Console.WriteLine($"Rango de generaciones: {stats.MinGeneration} - {stats.MaxGeneration} (span: {stats.GenerationSpan})");
            }
            
            if (stats.MinPosition.HasValue && stats.MaxPosition.HasValue)
            {
                Console.WriteLine($"Rango de posiciones: {stats.MinPosition} - {stats.MaxPosition} (span: {stats.PositionSpan})");
            }
            
            Console.WriteLine();
            
            // Display tree structure
            Console.WriteLine("Estructura del árbol:");
            Console.WriteLine(cladogram.ExportToText());
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al construir el cladograma: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Analyzes and displays cladogram with enhanced visualization
    /// </summary>
    public void AnalyzeCladogramWithVisualization()
    {
        Console.WriteLine("=== ANÁLISIS DEL CLADOGRAMA CON VISUALIZACIÓN ===");
        
        try
        {
            var cladogram = BuildCladogram();
            var visualizer = new CladogramVisualizer();
            
            // Show comprehensive visualization
            Console.WriteLine(visualizer.CreateComprehensiveVisualization(cladogram));
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al construir el cladograma: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Exports cladogram with enhanced visualization to text file
    /// </summary>
    public void ExportCladogramWithVisualization(string filePath)
    {
        // Asegurar que la carpeta existe
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        var cladogram = BuildCladogram();
        var visualizer = new CladogramVisualizer();
        var visualization = visualizer.CreateComprehensiveVisualization(cladogram);
        
        File.WriteAllText(filePath, visualization);
        Console.WriteLine($"✅ Cladograma con visualización exportado a: {filePath}");
    }
}
