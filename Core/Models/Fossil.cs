using SimulationEvolucion.Core.Enums;

namespace SimulationEvolucion.Core.Models;

/// <summary>
/// Represents a fossilized organism with partial genetic information
/// </summary>
public class Fossil
{
    public int Position { get; set; }
    public int GenerationFormed { get; set; }
    public int MaxPreservedLength { get; set; }
    public List<FossilGene> Genes { get; set; }
    public string OrganismId { get; set; }
    
    public Fossil(int position, int generationFormed, int maxPreservedLength, string organismId)
    {
        Position = position;
        GenerationFormed = generationFormed;
        MaxPreservedLength = maxPreservedLength;
        OrganismId = organismId;
        Genes = new List<FossilGene>();
    }
    
    public override string ToString()
    {
        return $"Fossil {OrganismId} at pos {Position} (gen {GenerationFormed}): {Genes.Count} genes, max length {MaxPreservedLength}";
    }
}
