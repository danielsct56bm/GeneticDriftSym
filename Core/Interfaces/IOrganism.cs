namespace SimulationEvolucion.Core.Interfaces;

public interface IOrganism
{
    string Id { get; }
    IList<IGene> Genes { get; }
    IList<IOrgan> Organs { get; }
    
    bool Consume(char nutrient);
    void ProcessOrgans();
    void TakeDamage(int amount);
    bool IsAlive();
    IOrganism Reproduce(string offspringId, double mutationRate = 0.01);
}