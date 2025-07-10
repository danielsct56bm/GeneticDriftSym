using SimulationEvolucion.Core.Enums;
using SimulationEvolucion.Core.Interfaces;
using SimulationEvolucion.Services;
using SimulationEvolucion.Utils;

namespace SimulationEvolucion.Core.Models;

public class Organism: IOrganism
{
    public string Id { get; private set; }
    public IList<IGene> Genes { get; private set; }
    public IList<IOrgan> Organs { get; private set; }
    
    public IList<char> nutrients { get; private set; }
    public bool Consume(char nutrient)
    {
        if (nutrients.Contains(nutrient))
        {
            nutrients.Remove(nutrient);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void ProcessOrgans()
    {
        foreach (var organ in Organs)
        {
            if (!organ.IsDestroyed())
            {
                organ.ActivateRandomUse(this);
            }
        }
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        if (health < 0) health = 0;
    }

    public bool IsAlive()
    {
        return health > 0;
    }

    public IOrganism Reproduce(string offspringId, double mutationRate=0.01)
    {
        // Copia genética con posibilidad de mutaciones
        var newGenes = Genes
            .Select(g => _rng.NextDouble() < mutationRate ? g.Mutate() : g)
            .ToList();

        return new Organism(offspringId, newGenes, new List<IOrgan>());
    }

    private int health = 100;
    public Dictionary<char, int> Nutrients { get; set; }
    public int DefenseBonus { get; set; } = 0;
    
    private static Random _rng = new Random();

    public Organism(string id, IList<IGene> genes, IList<IOrgan> organs)
    {
        Id = id;
        Genes = genes;
        Organs = organs;
        Nutrients = new Dictionary<char, int>();
    }
    
    public bool CanAfford(Dictionary<char, int> costs)
    {
        foreach (var cost in costs)
        {
            if (!Nutrients.ContainsKey(cost.Key) || Nutrients[cost.Key] < cost.Value)
                return false;
        }
        return true;
    }
    
    public void PayCost(Dictionary<char, int> costs)
    {
        foreach (var cost in costs)
        {
            if (Nutrients.ContainsKey(cost.Key))
                Nutrients[cost.Key] -= cost.Value;
        }
    }
    
    public Organ SelectRandomOrganToBuild()
    {
        // Aquí deberías tener un catálogo de órganos disponibles.
        // Por ahora devolvemos un órgano de prueba.

        var testUses = new List<OrganUse>
        {
            new OrganUse(OrganUseType.Heal, new Dictionary<char, int> { { 'A', 1 } }, effectValue: 2)
        };

        return new Organ("LIVER", 5, 10, buildCost: 3, maintenanceCost: 1, residueChar: 'X', uses: testUses, space: 1);
    }

    public bool CanAffordOrganBuild(int buildCost)
    {
        return Nutrients.Values.Sum() >= buildCost;
    }
    
    public void PayBuildCost(int buildCost)
    {
        // Restar nutrientes hasta completar el costo
        int remainingCost = buildCost;

        foreach (var key in Nutrients.Keys.ToList())
        {
            if (remainingCost == 0)
                break;

            int used = Math.Min(Nutrients[key], remainingCost);
            Nutrients[key] -= used;
            remainingCost -= used;
        }
    }
    
    public IOrgan SelectRandomOrgan()
    {
        if (Organs == null || Organs.Count == 0)
            return null;

        return Organs[_rng.Next(Organs.Count)];
    }
    
    public void AddNutrient(char nutrient, int amount)
    {
        if (!Nutrients.ContainsKey(nutrient))
            Nutrients[nutrient] = 0;

        Nutrients[nutrient] += amount;

        Console.WriteLine($"[Organismo {Id}] Gana +{amount} de {nutrient}");
    }
    
    public void Synthesize(char source, char target, int amount)
    {
        if (!Nutrients.ContainsKey(source) || Nutrients[source] < amount)
        {
            Console.WriteLine($"[Organismo {Id}] No tiene suficiente {source} para sintetizar.");
            return;
        }

        Nutrients[source] -= amount;

        if (!Nutrients.ContainsKey(target))
            Nutrients[target] = 0;

        Nutrients[target] += amount;

        Console.WriteLine($"[Organismo {Id}] Sintetizó {amount} {source} → {target}");
    }
    
    public void Discard(char nutrient, int amount)
    {
        if (!Nutrients.ContainsKey(nutrient))
            return;

        Nutrients[nutrient] -= amount;

        if (Nutrients[nutrient] < 0)
            Nutrients[nutrient] = 0;

        Console.WriteLine($"[Organismo {Id}] Descarta {amount} de {nutrient}");
    }
    /*
    public void TryReproduce(ISimulationEngine engine, int currentPosition)
    {
        Console.WriteLine($"[Organismo {Id}] Intenta reproducirse.");

        var offspring = this.Reproduce(IdGenerator.GetNextId(), mutationRate: 0.05);
        
        // Buscar espacio
        int[] possiblePositions = {currentPosition +1, currentPosition - 1};
        foreach (var pos in possiblePositions)
        {
            if (engine.IsValidPosition(pos) && engine.IsEmptyPosition(pos))
            {
                engine.PlaceOrganismAt(pos, offspring);
                Console.WriteLine($"[Organismo {Id}] Reprodujo nuevo organismo en posición {pos}.");
                return;
            }
        }
        
        Console.WriteLine($"[Organismo {Id} no encontro espacio"); 
    }*/
    
    public void TryAttackNearby(int attackPower)
    {
        Console.WriteLine($"[Organismo {Id}] Ataca a un organismo cercano con poder {attackPower}.");
        // Aquí debes coordinar con el engine para seleccionar organismos vecinos
    }
    /*
    public static Organism GenerateOrganism(string id, int geneCount)
    {
        var genes = new IList<IGene>();

        for (int i = 0; i < geneCount; i++)
        {
            genes.Add(Gene.GenerateRandom());
        }
        return new Organism(id, genes, new List<IOrgan>());
    }*/
/*
    public Organism Reproduce(string offspring, double mutationRate = 0.01)
    {
        var newGenes = new List<Gene>();

        foreach (var gene in Genes)
        {
            var newGene = new Gene(gene.Symbol);

            if (_rng.NextDouble() < mutationRate)
            {
                newGene.Mutate();
            }
            
            newGenes.Add(newGene);
        }
        
        return new Organism(offspring, newGenes, new List<Organ>());
    }*/

    public string GetGenomeText()
    {
        var genome = "";

        for (int i = 0; i < Genes.Count; i+=3)
        {
            if (i+2 >= Genes.Count) break;
            
            string triplet = $"{Genes[i].Symbol}{Genes[i + 1].Symbol}{Genes[i + 2].Symbol}";
            genome += GeneTranslator.Translate(triplet);
        }
        
        return genome;
    }

    public string GetGeneSequence()
    {
        return string.Join("", Genes.Select(g => g.Symbol));
    }
}