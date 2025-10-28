using SimulationEvolucion.Core.Enums;
using SimulationEvolucion.Core.Interfaces;
using SimulationEvolucion.Services;
using SimulationEvolucion.Utils;

namespace SimulationEvolucion.Core.Models;

/// <summary>
/// Optimized 1D world where organisms evolve
/// </summary>
public class World1D : IWorld1D
{
    public int Size { get; private set; }
    
    // Optimized data structure: Dictionary for O(1) removal operations
    private Dictionary<string, IOrganism> _organismsDict;
    public List<IOrganism> Organisms => _organismsDict.Values.ToList();
    
    private readonly double _mutationRate;
    private readonly double _selectionStrength;
    private readonly int _carryingCapacity;
    
    // Reference to optimized fossil manager for recording deaths
    public OptimizedFossilManager? FossilManager { get; set; }
    public int CurrentGeneration { get; set; }
    
    public World1D(int size, double mutationRate = 0.01, double selectionStrength = 0.5, int carryingCapacity = 1000)
    {
        Size = size;
        _organismsDict = new Dictionary<string, IOrganism>();
        _mutationRate = mutationRate;
        _selectionStrength = selectionStrength;
        _carryingCapacity = carryingCapacity;
    }
    
    public void AddOrganism(IOrganism organism)
    {
        if (organism.Position >= 0 && organism.Position < Size)
        {
            _organismsDict[organism.Id] = organism;
        }
    }
    
    public void RemoveOrganism(string organismId)
    {
        _organismsDict.Remove(organismId);
    }
    
    public List<IOrganism> GetOrganismsAt(int position)
    {
        return _organismsDict.Values.Where(o => o.Position == position).ToList();
    }
    
    public void Evolve(Random random)
    {
        // 1. Selección natural
        ApplySelection(random);
        
        // 2. Reproducción
        Reproduce(random);
        
        // 3. Mutación
        Mutate(random);
        
        // 4. Migración (movimiento en el mundo 1D)
        Migrate(random);
        
        // 5. Regulación de población
        RegulatePopulation(random);
    }
    
    private void ApplySelection(Random random)
    {
        // Natural selection based on fitness, but only affects selected genes
        var organismsToRemove = new List<IOrganism>();
        
        foreach (var organism in _organismsDict.Values)
        {
            var fitness = organism.CalculateFitness();
            var selectionPressure = CalculateSelectionPressure(organism);
            
            // Survival probability based on fitness and selection pressure
            var survivalProbability = fitness * (1.0 + selectionPressure);
            
            if (random.NextDouble() > survivalProbability)
            {
                organismsToRemove.Add(organism);
            }
        }
        
        foreach (var organism in organismsToRemove)
        {
            // Record death for fossilization before removing
            if (FossilManager != null)
            {
                FossilManager.RecordDeath(organism, CurrentGeneration, new Random());
            }
            RemoveOrganism(organism.Id);
        }
    }
    
    private double CalculateSelectionPressure(IOrganism organism)
    {
        // La presión selectiva depende de la densidad de población en la posición
        var organismsAtPosition = GetOrganismsAt(organism.Position).Count;
        var density = (double)organismsAtPosition / _carryingCapacity;
        
        // Solo los genes seleccionados contribuyen a la presión selectiva
        var selectedGenes = organism.GetGenesByType(GeneType.Selected);
        var selectedFitness = selectedGenes.Any() ? selectedGenes.Average(g => g.CalculateFitness()) : 1.0;
        
        return _selectionStrength * density * (selectedFitness - 1.0);
    }
    
    private void Reproduce(Random random)
    {
        var offspring = new List<IOrganism>();
        var reproducingOrganisms = _organismsDict.Values.Where(o => random.NextDouble() < 0.5).ToList();
        
        foreach (var organism in reproducingOrganisms)
        {
            var child = organism.Reproduce(random, _mutationRate);
            offspring.Add(child);
        }
        
        foreach (var child in offspring)
        {
            AddOrganism(child);
        }
    }
    
    private void Mutate(Random random)
    {
        // Use batch processing for better memory management
        BatchProcessor.ProcessOrganismsInBatches(_organismsDict.Values, organism =>
        {
            bool organismChanged = false;
            foreach (var gene in organism.Genes)
            {
                if (random.NextDouble() < _mutationRate)
                {
                    var mutationTypes = Enum.GetValues<MutationType>();
                    var mutationType = mutationTypes[random.Next(mutationTypes.Length)];
                    gene.Mutate(mutationType, random);
                    organismChanged = true;
                }
            }
            
            // Mark fitness as dirty if organism mutated
            if (organismChanged)
            {
                organism.MarkFitnessDirty();
            }
        }, batchSize: 50);
    }
    
    private void Migrate(Random random)
    {
        // Use batch processing for migration
        BatchProcessor.ProcessOrganismsInBatches(_organismsDict.Values, organism =>
        {
            // Random movement in 1D world
            if (random.NextDouble() < 0.1) // 10% migration probability
            {
                var direction = random.Next(2) == 0 ? -1 : 1;
                var distance = random.Next(1, 4); // Move 1-3 positions
                
                var newPosition = organism.Position + (direction * distance);
                
                // Keep within world boundaries
                organism.Position = Math.Max(0, Math.Min(Size - 1, newPosition));
            }
        }, batchSize: 100);
    }
    
    private void RegulatePopulation(Random random)
    {
        if (_organismsDict.Count > _carryingCapacity)
        {
            // Remove organisms randomly to maintain carrying capacity
            var toRemove = _organismsDict.Count - _carryingCapacity;
            var organismsToRemove = _organismsDict.Values.OrderBy(x => random.Next()).Take(toRemove).ToList();
            
            foreach (var organism in organismsToRemove)
            {
                // Record death for fossilization before removing
                if (FossilManager != null)
                {
                    FossilManager.RecordDeath(organism, CurrentGeneration, new Random());
                }
                RemoveOrganism(organism.Id);
            }
        }
    }
    
    public PopulationStatistics GetStatistics()
    {
        var stats = new PopulationStatistics
        {
            TotalOrganisms = _organismsDict.Count
        };
        
        if (_organismsDict.Any())
        {
            var fitnesses = _organismsDict.Values.Select(o => o.CalculateFitness()).ToList();
            stats.AverageFitness = fitnesses.Average();
            stats.FitnessVariance = fitnesses.Variance();
            
            // Count genes by type
            stats.SelectedGenesCount = _organismsDict.Values.SelectMany(o => o.GetGenesByType(GeneType.Selected)).Count();
            stats.NeutralGenesCount = _organismsDict.Values.SelectMany(o => o.GetGenesByType(GeneType.Neutral)).Count();
            
            // Genetic diversity analysis
            AnalyzeGeneDiversity(stats);
        }
        
        return stats;
    }
    
    private void AnalyzeGeneDiversity(PopulationStatistics stats)
    {
        // Analyze diversity in neutral genes (genetic drift)
        var neutralGenes = _organismsDict.Values.SelectMany(o => o.GetGenesByType(GeneType.Neutral)).ToList();
        
        if (neutralGenes.Any())
        {
            var neutralDiversity = CalculateGeneticDiversity(neutralGenes);
            stats.GeneDiversity["neutral"] = neutralDiversity;
        }
        
        // Analyze diversity in selected genes
        var selectedGenes = _organismsDict.Values.SelectMany(o => o.GetGenesByType(GeneType.Selected)).ToList();
        
        if (selectedGenes.Any())
        {
            var selectedDiversity = CalculateGeneticDiversity(selectedGenes);
            stats.GeneDiversity["selected"] = selectedDiversity;
        }
    }
    
    private double CalculateGeneticDiversity(List<IGene> genes)
    {
        if (genes.Count < 2) return 0;
        
        // Use sampling for large gene sets to improve performance
        const int maxSampleSize = 100;
        var sampleGenes = genes.Count > maxSampleSize 
            ? genes.OrderBy(x => Random.Shared.Next()).Take(maxSampleSize).ToList()
            : genes;
        
        double totalDistance = 0;
        int comparisons = 0;
        
        for (int i = 0; i < sampleGenes.Count; i++)
        {
            for (int j = i + 1; j < sampleGenes.Count; j++)
            {
                totalDistance += CalculateHammingDistance(sampleGenes[i].Sequence, sampleGenes[j].Sequence);
                comparisons++;
            }
        }
        
        return comparisons > 0 ? totalDistance / comparisons : 0;
    }
    
    private int CalculateHammingDistance(List<Nucleotide> seq1, List<Nucleotide> seq2)
    {
        int distance = 0;
        int minLength = Math.Min(seq1.Count, seq2.Count);
        
        for (int i = 0; i < minLength; i++)
        {
            if (seq1[i] != seq2[i])
                distance++;
        }
        
        return distance;
    }
    
    public override string ToString()
    {
        return $"World1D: {_organismsDict.Count} organisms, size: {Size}, carrying capacity: {_carryingCapacity}";
    }
}

// Extensión para calcular varianza
public static class StatisticsExtensions
{
    public static double Variance(this IEnumerable<double> values)
    {
        var valueList = values.ToList();
        if (!valueList.Any()) return 0;
        
        var mean = valueList.Average();
        var sumSquaredDiffs = valueList.Sum(x => Math.Pow(x - mean, 2));
        return sumSquaredDiffs / valueList.Count;
    }
}
