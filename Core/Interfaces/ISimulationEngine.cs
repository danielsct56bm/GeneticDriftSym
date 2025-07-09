namespace SimulationEvolucion.Core.Interfaces;

public interface ISimulationEngine
{
    void SeedInitialOrganisms(int organismCount, int geneCount);
    void Run(int steps);
    void ProcessStep();
    void ShowWorldDetailed();
}