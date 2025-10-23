using SimulationEvolucion.Core.Enums;

namespace SimulationEvolucion.Core.Interfaces;

/// <summary>
/// Interfaz para genes que pueden mutar y ser seleccionados
/// </summary>
public interface IGene
{
    /// <summary>Secuencia de nucleótidos del gen</summary>
    List<Nucleotide> Sequence { get; }
    
    /// <summary>Tipo de gen (seleccionado o neutral)</summary>
    GeneType Type { get; }
    
    /// <summary>ID único del gen</summary>
    string Id { get; }
    
    /// <summary>Aplica una mutación al gen</summary>
    void Mutate(MutationType mutationType, Random random);
    
    /// <summary>Calcula la aptitud del gen</summary>
    double CalculateFitness();
    
    /// <summary>Crea una copia del gen</summary>
    IGene Clone();
}
