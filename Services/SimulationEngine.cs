using SimulationEvolucion.Core.Models;
using SimulationEvolucion.Models;
using SimulationEvolucion.Utils;

namespace SimulationEvolucion.Services;

public class SimulationEngine
{
    private readonly Enviroment _environment;
    private readonly Random _rng = new Random();

    public SimulationEngine(int worldSize)
    {
        _environment = new Enviroment(worldSize);
    }

    public void SeedInitialOrganisms(int organismCount, int geneCount)
    {
        if (organismCount < 1 || organismCount>_environment.world.Length)
        {
            return;
        }
        for (int i = 0; i < organismCount; i++)
        {
            var organism = Organism.GenerateOrganism(IdGenerator.GetNextId(), geneCount);
            
            int position;
            do
            {
                position = _rng.Next(_environment.world.Length);

            } while (_environment.world[position] == organism);
            
            _environment.world[position] = organism;
        }
    }

    public void Run(int steps)
    {
        for (int i = 0; i < steps; i++)
        {
            Console.WriteLine($"--- Step {i} ---");
            _environment.display();
            Console.WriteLine();

            ProcessStep();
        }
    }

    public void ProcessStep()
    {
        var newOrganisms = new List<(int position, Organism organism)>();

        for (int i = 0; i < _environment.world.Length; i++)
        {
            if (_environment.world[i] is Organism organism)
            {
                if (_rng.NextDouble() < 0.3)
                {
                    var offspring = organism.Reproduce(IdGenerator.GetNextId(),mutationRate: 0.05);
                    newOrganisms.Add((i, offspring));
                }
            }
        }

        foreach (var (position, organism) in newOrganisms)
        {
            if (position+1 <_environment.world.Length &&_environment.world[position+1] is not Organism organism2)
            {
                _environment.world[position + 1] = organism;
            }
            else if (position-1 >=0 && _environment.world[position - 1] is not Organism organism3)
            {
                _environment.world[position - 1] = organism;
            }
            else
            {
                if (_rng.NextDouble() < 0.3)
                {
                    _environment.world[position] = organism;
                }
            }
        }
    }

    public void ShowWorldDetailed()
    {
        _environment.display();
    }
}