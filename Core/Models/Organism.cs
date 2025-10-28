using SimulationEvolucion.Core.Enums;
using SimulationEvolucion.Core.Interfaces;

namespace SimulationEvolucion.Core.Models;

/// <summary>
/// Implementación de un organismo con genes y fitness caching
/// </summary>
public class Organism : IOrganism
{
    public List<IGene> Genes { get; private set; }
    public string Id { get; set; }
    public int Position { get; set; }
    
    // Fitness caching for performance optimization
    private double? _cachedFitness;
    private bool _fitnessDirty = true;
    
    private static int _nextId = 1;
    
    public Organism(List<IGene> genes, int position = 0)
    {
        Genes = new List<IGene>(genes);
        Position = position;
        Id = $"Organism_{_nextId++}";
    }
    
    public Organism(int position, Random random, int geneCount = 10, int geneLength = 20, Dictionary<GeneType, string>? initialSequences = null) : this(new List<IGene>(), position)
    {
        var selectedCount = (int)(geneCount * 0.3);  // 30% genes seleccionados
        var neutralCount = geneCount - selectedCount; // 70% genes neutrales
        
        // Crear genes seleccionados con secuencia inicial común
        for (int i = 0; i < selectedCount; i++)
        {
            var initialSeq = initialSequences?[GeneType.Selected] ?? null;
            var gene = new Gene(GeneType.Selected, geneLength, random, initialSeq);
            Genes.Add(gene);
        }
        
        // Crear genes neutrales con secuencia inicial común
        for (int i = 0; i < neutralCount; i++)
        {
            var initialSeq = initialSequences?[GeneType.Neutral] ?? null;
            var gene = new Gene(GeneType.Neutral, geneLength, random, initialSeq);
            Genes.Add(gene);
        }
    }
    
    public double CalculateFitness()
    {
        // Return cached fitness if available and not dirty
        if (!_fitnessDirty && _cachedFitness.HasValue)
            return _cachedFitness.Value;
        
        // Calculate fitness total as weighted average of selected and neutral genes
        var selectedGenes = GetGenesByType(GeneType.Selected);
        var neutralGenes = GetGenesByType(GeneType.Neutral);
        
        var selectedFitness = selectedGenes.Any() ? selectedGenes.Average(g => g.CalculateFitness()) : 1.0;
        var neutralFitness = neutralGenes.Any() ? neutralGenes.Average(g => g.CalculateFitness()) : 1.0;
        
        // Selected genes have more weight in fitness
        var result = selectedFitness * 0.7 + neutralFitness * 0.3;
        
        // Cache the result
        _cachedFitness = result;
        _fitnessDirty = false;
        
        return result;
    }
    
    public IOrganism Reproduce(Random random, double mutationRate)
    {
        var offspringGenes = new List<IGene>();
        
        foreach (var gene in Genes)
        {
            var offspringGene = gene.Clone();
            
            // Apply mutations with mutationRate probability
            if (random.NextDouble() < mutationRate)
            {
                var mutationTypes = Enum.GetValues<MutationType>();
                var mutationType = mutationTypes[random.Next(mutationTypes.Length)];
                offspringGene.Mutate(mutationType, random);
            }
            
            offspringGenes.Add(offspringGene);
        }
        
        // Random initial position for offspring
        var newPosition = random.Next(0, 100); // World size 100 by default
        
        return new Organism(offspringGenes, newPosition);
    }
    
    /// <summary>
    /// Mark fitness as dirty when organism changes (for caching optimization)
    /// </summary>
    public void MarkFitnessDirty()
    {
        _fitnessDirty = true;
    }
    
    public List<IGene> GetGenesByType(GeneType type)
    {
        return Genes.Where(g => g.Type == type).ToList();
    }
    
    public override string ToString()
    {
        var selectedCount = GetGenesByType(GeneType.Selected).Count;
        var neutralCount = GetGenesByType(GeneType.Neutral).Count;
        return $"{Id} at pos {Position}: {selectedCount} selected, {neutralCount} neutral genes, fitness: {CalculateFitness():F2}";
    }
}
