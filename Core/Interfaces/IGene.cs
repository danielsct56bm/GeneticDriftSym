namespace SimulationEvolucion.Core.Interfaces;

public interface IGene
{
    string Symbol { get;}
    IGene Mutate();
}