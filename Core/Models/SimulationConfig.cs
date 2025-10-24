namespace SimulationEvolucion.Core.Models;

/// <summary>
/// Configuración para la simulación de deriva genética
/// </summary>
public class SimulationConfig
{
    public int WorldSize { get; set; } = 100;
    public int InitialPopulationSize { get; set; } = 500;
    public int CarryingCapacity { get; set; } = 1000;
    public double MutationRate { get; set; } = 0.01;
    public double SelectionStrength { get; set; } = 0.5;
    public int GeneCount { get; set; } = 10;
    public int GeneLength { get; set; } = 20;
    public double SelectedGeneRatio { get; set; } = 0.3; // 30% genes seleccionados, 70% neutrales
    public int MaxGenerations { get; set; } = 1000;
    public bool LogProgress { get; set; } = true;
    public int LogInterval { get; set; } = 10;
    
    // Fossil record configuration
    public bool EnableFossilRecord { get; set; } = true;
    public double FossilizationProbability { get; set; } = 0.01;
    public int FossilHalfLife { get; set; } = 50;
}
