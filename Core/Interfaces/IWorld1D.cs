using SimulationEvolucion.Core.Models;
using SimulationEvolucion.Services;

namespace SimulationEvolucion.Core.Interfaces;

/// <summary>
/// Interfaz para el mundo 1D donde viven los organismos
/// </summary>
public interface IWorld1D
{
    /// <summary>Tamaño del mundo</summary>
    int Size { get; }
    
    /// <summary>Organismos en el mundo</summary>
    List<IOrganism> Organisms { get; }
    
    /// <summary>Fossil manager for recording deaths</summary>
    FossilManager? FossilManager { get; set; }
    
    /// <summary>Current generation number</summary>
    int CurrentGeneration { get; set; }
    
    /// <summary>Agrega un organismo al mundo</summary>
    void AddOrganism(IOrganism organism);
    
    /// <summary>Elimina un organismo del mundo</summary>
    void RemoveOrganism(string organismId);
    
    /// <summary>Obtiene organismos en una posición específica</summary>
    List<IOrganism> GetOrganismsAt(int position);
    
    /// <summary>Ejecuta una generación de evolución</summary>
    void Evolve(Random random);
    
    /// <summary>Calcula estadísticas de la población</summary>
    PopulationStatistics GetStatistics();
}
