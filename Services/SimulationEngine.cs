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
    public FossilManager? FossilManager { get; private set; }
    
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
        
        // Initialize fossil manager if fossil record is enabled
        if (config.EnableFossilRecord)
        {
            FossilManager = new FossilManager(config.FossilizationProbability, config.FossilHalfLife);
            World.FossilManager = FossilManager;
        }
    }
    
    public void Initialize(Random? random = null)
    {
        var rng = random ?? _random;
        
        // Secuencias iniciales comunes para todos los organismos
        var initialSequences = new Dictionary<GeneType, string>
        {
            { GeneType.Selected, "ATCGATCGATCGATCGATCG" },  // 20 nucleótidos
            { GeneType.Neutral, "GCTAGCTAGCTAGCTAGCTA" }    // 20 nucleótidos
        };
        
        // Crear población inicial
        for (int i = 0; i < Config.InitialPopulationSize; i++)
        {
            var position = rng.Next(Config.WorldSize);
            var organism = new Organism(position, rng, Config.GeneCount, Config.GeneLength, initialSequences);
            
            World.AddOrganism(organism);
        }
        
        // Registrar estadísticas iniciales
        var initialStats = World.GetStatistics();
        History.Add(initialStats);
        
        if (Config.LogProgress)
        {
            Console.WriteLine($"Simulación inicializada: {initialStats.TotalOrganisms} organismos");
            Console.WriteLine($"Todos los organismos empiezan con las mismas secuencias:");
            Console.WriteLine($"  Genes seleccionados: {initialSequences[GeneType.Selected]}");
            Console.WriteLine($"  Genes neutrales: {initialSequences[GeneType.Neutral]}");
            LogStatistics(0, initialStats);
        }
    }
    
    
    public void RunSimulation(int generations)
    {
        Console.WriteLine($"Iniciando simulación de {generations} generaciones...");
        
        for (int generation = 1; generation <= generations; generation++)
        {
            RunGeneration();
            
            if (Config.LogProgress && generation % Config.LogInterval == 0)
            {
                var stats = GetCurrentStatistics();
                LogStatistics(generation, stats);
            }
            
            // Log cada 5 generaciones para ver crecimiento
            if (generation % 5 == 0)
            {
                var stats = GetCurrentStatistics();
                Console.WriteLine($"Gen {generation}: {stats.TotalOrganisms} organismos, " +
                                $"diversidad neutral: {stats.GeneDiversity.GetValueOrDefault("neutral", 0):F3}, " +
                                $"diversidad seleccionada: {stats.GeneDiversity.GetValueOrDefault("selected", 0):F3}");
            }
        }
        
        Console.WriteLine("\nSimulación completada!");
        
        // Apply fossil decay if fossil record is enabled
        if (Config.EnableFossilRecord && FossilManager != null)
        {
            Console.WriteLine("Aplicando decadencia de fósiles...");
            FossilManager.ApplyDecay(_currentGeneration, _random);
            Console.WriteLine($"Fósiles después de decadencia: {FossilManager.TotalFossils}");
        }
    }
    
    public void RunGeneration()
    {
        _currentGeneration++;
        World.CurrentGeneration = _currentGeneration;
        World.Evolve(_random);
        var stats = World.GetStatistics();
        History.Add(stats);
    }
    
    public PopulationStatistics GetCurrentStatistics()
    {
        return World.GetStatistics();
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
            FossilManager.SaveToJson(filePath);
            Console.WriteLine($"Registro fósil exportado a: {filePath}");
        }
    }
    
    /// <summary>
    /// Gets fossil statistics
    /// </summary>
    public FossilStatistics? GetFossilStatistics()
    {
        if (Config.EnableFossilRecord && FossilManager != null)
        {
            return FossilManager.GetStatistics();
        }
        return null;
    }
}
