using SimulationEvolucion.Core.Enums;
using SimulationEvolucion.Core.Interfaces;

namespace SimulationEvolucion.Core.Models;

/// <summary>
/// Implementación de un organismo con genes
/// </summary>
public class Organism : IOrganism
{
    public List<IGene> Genes { get; private set; }
    public string Id { get; private set; }
    public int Position { get; set; }
    
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
        // Fitness total es el promedio ponderado de genes seleccionados y neutrales
        var selectedGenes = GetGenesByType(GeneType.Selected);
        var neutralGenes = GetGenesByType(GeneType.Neutral);
        
        var selectedFitness = selectedGenes.Any() ? selectedGenes.Average(g => g.CalculateFitness()) : 1.0;
        var neutralFitness = neutralGenes.Any() ? neutralGenes.Average(g => g.CalculateFitness()) : 1.0;
        
        // Los genes seleccionados tienen más peso en el fitness
        return selectedFitness * 0.7 + neutralFitness * 0.3;
    }
    
    public IOrganism Reproduce(Random random, double mutationRate)
    {
        var offspringGenes = new List<IGene>();
        
        foreach (var gene in Genes)
        {
            var offspringGene = gene.Clone();
            
            // Aplicar mutaciones con probabilidad mutationRate
            if (random.NextDouble() < mutationRate)
            {
                var mutationTypes = Enum.GetValues<MutationType>();
                var mutationType = mutationTypes[random.Next(mutationTypes.Length)];
                offspringGene.Mutate(mutationType, random);
            }
            
            offspringGenes.Add(offspringGene);
        }
        
        // Posición inicial aleatoria para el descendiente
        var newPosition = random.Next(0, 100); // Mundo de tamaño 100 por defecto
        
        return new Organism(offspringGenes, newPosition);
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
