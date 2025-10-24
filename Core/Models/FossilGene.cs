using SimulationEvolucion.Core.Enums;

namespace SimulationEvolucion.Core.Models;

/// <summary>
/// Represents a gene within a fossil with partial sequence preservation
/// Note: GeneType is not preserved in fossils - only the actual preserved sequence
/// </summary>
public class FossilGene
{
    public string PreservedSequence { get; set; }
    
    public FossilGene(List<Nucleotide?> preservedSequence)
    {
        // Convert to string format where * represents decayed nucleotides
        PreservedSequence = string.Join("", preservedSequence.Select(n => n?.ToString() ?? "*"));
    }
    
    public FossilGene(string preservedSequence)
    {
        PreservedSequence = preservedSequence ?? "";
    }
    
    public override string ToString()
    {
        return $"Gene: {PreservedSequence}";
    }
}
