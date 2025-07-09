using SimulationEvolucion.Models;

namespace SimulationEvolucion.Services;

public class Enviroment
{
    public object[] world { get; private set; }

    public Enviroment(int size)
    {
        world = new object[size];
    }

    public void display()
    {
        for (int i = 0; i < world.Length; i++)
        {
            if (world[i] == null)
                Console.Write("- ");
            else if (world[i] is char c)
                Console.Write(c+"");
            else if (world[i] is Organism organism)
                Console.Write(organism.Id+"");
        }
    }
}