using SimulationEvolucion.Core.Enums;
using SimulationEvolucion.Core.Interfaces;
using SimulationEvolucion.Services;

namespace SimulationEvolucion.Core.Models;

/// <summary>
/// Mundo 1D donde evolucionan los organismos
/// </summary>
public class World1D : IWorld1D
{
    public int Size { get; private set; }
    public List<IOrganism> Organisms { get; private set; }
    
    private readonly double _mutationRate;
    private readonly double _selectionStrength;
    private readonly int _carryingCapacity;
    
    // Reference to fossil manager for recording deaths
    public FossilManager? FossilManager { get; set; }
    public int CurrentGeneration { get; set; }
    
    public World1D(int size, double mutationRate = 0.01, double selectionStrength = 0.5, int carryingCapacity = 1000)
    {
        Size = size;
        Organisms = new List<IOrganism>();
        _mutationRate = mutationRate;
        _selectionStrength = selectionStrength;
        _carryingCapacity = carryingCapacity;
    }
    
    public void AddOrganism(IOrganism organism)
    {
        if (organism.Position >= 0 && organism.Position < Size)
        {
            Organisms.Add(organism);
        }
    }
    
    public void RemoveOrganism(string organismId)
    {
        Organisms.RemoveAll(o => o.Id == organismId);
    }
    
    public List<IOrganism> GetOrganismsAt(int position)
    {
        return Organisms.Where(o => o.Position == position).ToList();
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
        // Selección basada en fitness, pero solo afecta genes seleccionados
        var organismsToRemove = new List<IOrganism>();
        
        foreach (var organism in Organisms)
        {
            var fitness = organism.CalculateFitness();
            var selectionPressure = CalculateSelectionPressure(organism);
            
            // Probabilidad de supervivencia basada en fitness y presión selectiva
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
        var reproducingOrganisms = Organisms.Where(o => random.NextDouble() < 0.5).ToList();
        
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
        foreach (var organism in Organisms)
        {
            foreach (var gene in organism.Genes)
            {
                if (random.NextDouble() < _mutationRate)
                {
                    var mutationTypes = Enum.GetValues<MutationType>();
                    var mutationType = mutationTypes[random.Next(mutationTypes.Length)];
                    gene.Mutate(mutationType, random);
                }
            }
        }
    }
    
    private void Migrate(Random random)
    {
        foreach (var organism in Organisms)
        {
            // Movimiento aleatorio en el mundo 1D
            if (random.NextDouble() < 0.1) // 10% probabilidad de migración
            {
                var direction = random.Next(2) == 0 ? -1 : 1;
                var distance = random.Next(1, 4); // Moverse 1-3 posiciones
                
                var newPosition = organism.Position + (direction * distance);
                
                // Mantener dentro de los límites del mundo
                organism.Position = Math.Max(0, Math.Min(Size - 1, newPosition));
            }
        }
    }
    
    private void RegulatePopulation(Random random)
    {
        if (Organisms.Count > _carryingCapacity)
        {
            // Eliminar organismos aleatoriamente para mantener capacidad de carga
            var toRemove = Organisms.Count - _carryingCapacity;
            var organismsToRemove = Organisms.OrderBy(x => random.Next()).Take(toRemove).ToList();
            
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
            TotalOrganisms = Organisms.Count
        };
        
        if (Organisms.Any())
        {
            var fitnesses = Organisms.Select(o => o.CalculateFitness()).ToList();
            stats.AverageFitness = fitnesses.Average();
            stats.FitnessVariance = fitnesses.Variance();
            
            // Contar genes por tipo
            stats.SelectedGenesCount = Organisms.SelectMany(o => o.GetGenesByType(GeneType.Selected)).Count();
            stats.NeutralGenesCount = Organisms.SelectMany(o => o.GetGenesByType(GeneType.Neutral)).Count();
            
            // Análisis de diversidad genética
            AnalyzeGeneDiversity(stats);
        }
        
        return stats;
    }
    
    private void AnalyzeGeneDiversity(PopulationStatistics stats)
    {
        // Análizar diversidad en genes neutrales (deriva genética)
        var neutralGenes = Organisms.SelectMany(o => o.GetGenesByType(GeneType.Neutral)).ToList();
        
        if (neutralGenes.Any())
        {
            var neutralDiversity = CalculateGeneticDiversity(neutralGenes);
            stats.GeneDiversity["neutral"] = neutralDiversity;
        }
        
        // Análizar diversidad en genes seleccionados
        var selectedGenes = Organisms.SelectMany(o => o.GetGenesByType(GeneType.Selected)).ToList();
        
        if (selectedGenes.Any())
        {
            var selectedDiversity = CalculateGeneticDiversity(selectedGenes);
            stats.GeneDiversity["selected"] = selectedDiversity;
        }
    }
    
    private double CalculateGeneticDiversity(List<IGene> genes)
    {
        if (genes.Count < 2) return 0;
        
        double totalDistance = 0;
        int comparisons = 0;
        
        for (int i = 0; i < genes.Count; i++)
        {
            for (int j = i + 1; j < genes.Count; j++)
            {
                totalDistance += CalculateHammingDistance(genes[i].Sequence, genes[j].Sequence);
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
        return $"World1D: {Organisms.Count} organisms, size: {Size}, carrying capacity: {_carryingCapacity}";
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
