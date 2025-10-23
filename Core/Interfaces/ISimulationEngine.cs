using SimulationEvolucion.Core.Models;

namespace SimulationEvolucion.Core.Interfaces;

/// <summary>
/// Interfaz para el motor de simulación de deriva genética
/// </summary>
public interface ISimulationEngine
{
    /// <summary>Mundo donde ocurre la simulación</summary>
    IWorld1D World { get; }
    
    /// <summary>Configuración de la simulación</summary>
    SimulationConfig Config { get; }
    
    /// <summary>Historial de estadísticas por generación</summary>
    List<PopulationStatistics> History { get; }
    
    /// <summary>Ejecuta la simulación por un número específico de generaciones</summary>
    void RunSimulation(int generations);
    
    /// <summary>Ejecuta una generación de la simulación</summary>
    void RunGeneration();
    
    /// <summary>Inicializa la simulación con población inicial</summary>
    void Initialize(Random random);
    
    /// <summary>Exporta los resultados de la simulación</summary>
    void ExportResults(string filePath);
    
    /// <summary>Obtiene estadísticas actuales</summary>
    PopulationStatistics GetCurrentStatistics();
}
