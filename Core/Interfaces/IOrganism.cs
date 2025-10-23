using SimulationEvolucion.Core.Enums;

namespace SimulationEvolucion.Core.Interfaces;

/// <summary>
/// Interfaz para organismos que contienen genes
/// </summary>
public interface IOrganism
{
    /// <summary>Lista de genes del organismo</summary>
    List<IGene> Genes { get; }
    
    /// <summary>ID único del organismo</summary>
    string Id { get; }
    
    /// <summary>Posición en el mundo 1D</summary>
    int Position { get; set; }
    
    /// <summary>Calcula la aptitud total del organismo</summary>
    double CalculateFitness();
    
    /// <summary>Crea un descendiente con mutaciones</summary>
    IOrganism Reproduce(Random random, double mutationRate);
    
    /// <summary>Obtiene genes por tipo</summary>
    List<IGene> GetGenesByType(GeneType type);
}
