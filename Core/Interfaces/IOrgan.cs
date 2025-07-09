namespace SimulationEvolucion.Core.Interfaces;

public interface IOrgan
{
    string Gene { get; }
    int Hp { get; }
    int MaintenanceCost { get; }
    char? ResidueChar { get; }
    
    IList<IOrganUse> Uses { get; }
    
    void ActivateRandomUse(IOrganism owner);
    void ActivateUse(int useIndex, IOrganism owner);
    void TakeDamage(int amount);
}