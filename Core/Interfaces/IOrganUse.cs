using SimulationEvolucion.Core.Enums;

namespace SimulationEvolucion.Core.Interfaces;

public interface IOrganUse
{
    OrganUseType UseType { get; }
    char? SourceChar { get; }
    char? TargetChar { get; }
    int Power { get; }
    
    void Apply(IOrganism owner, ISimulationEnvironment environment);
}