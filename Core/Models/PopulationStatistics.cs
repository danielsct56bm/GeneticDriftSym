namespace SimulationEvolucion.Core.Models;

/// <summary>
/// Estadísticas de la población para análisis de deriva genética
/// </summary>
public class PopulationStatistics
{
    public int TotalOrganisms { get; set; }
    public double AverageFitness { get; set; }
    public double FitnessVariance { get; set; }
    public Dictionary<string, int> GeneFrequency { get; set; } = new();
    public Dictionary<string, double> GeneDiversity { get; set; } = new();
    public int SelectedGenesCount { get; set; }
    public int NeutralGenesCount { get; set; }
}
