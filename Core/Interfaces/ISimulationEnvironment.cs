namespace SimulationEvolucion.Core.Interfaces;

public interface ISimulationEnvironment
{
    object[] World { get; }
    int Size { get; }
    
    bool IsPositionFree(int index);
    void PlaceOrganism(int index,IOrganism organism);
    IOrganism GetOrganismAt(int index);
    void RemoveOrganism(int index);
}