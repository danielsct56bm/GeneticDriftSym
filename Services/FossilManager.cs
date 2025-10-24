using SimulationEvolucion.Core.Enums;
using SimulationEvolucion.Core.Interfaces;
using SimulationEvolucion.Core.Models;
using System.Text.Json;

namespace SimulationEvolucion.Services;

/// <summary>
/// Manages fossil record creation, decay, and persistence
/// </summary>
public class FossilManager
{
    private readonly List<Fossil> _fossils;
    private readonly double _fossilizationProbability;
    private readonly int _fossilHalfLife;
    private readonly double _physicalDamageProbability;
    
    public IReadOnlyList<Fossil> Fossils => _fossils.AsReadOnly();
    public int TotalFossils => _fossils.Count;
    
    public FossilManager(double fossilizationProbability = 0.01, int fossilHalfLife = 50)
    {
        _fossils = new List<Fossil>();
        _fossilizationProbability = fossilizationProbability;
        _fossilHalfLife = fossilHalfLife;
        
        // Physical damage probability per fossil
        _physicalDamageProbability = 0.001; // 0.1% chance of physical damage per fossil
    }
    
    /// <summary>
    /// Records the death of an organism with a chance of fossilization
    /// </summary>
    public void RecordDeath(IOrganism organism, int generation, Random random)
    {
        if (random.NextDouble() < _fossilizationProbability)
        {
            var fossil = CreateFossil(organism, generation, random);
            _fossils.Add(fossil);
        }
    }
    
    /// <summary>
    /// Creates a fossil from an organism using sediment coverage simulation
    /// </summary>
    private Fossil CreateFossil(IOrganism organism, int generation, Random random)
    {
        // Determine sediment coverage using binomial distribution (40 trials, p=0.5)
        // This simulates how much of the organism gets covered by sediments
        var sedimentCoverage = SimulateBinomialDistribution(40, 0.5, random);
        
        var fossil = new Fossil(organism.Position, generation, sedimentCoverage, organism.Id);
        
        // Create fossil genes with the same max preserved length for all genes
        foreach (var gene in organism.Genes)
        {
            var fossilGene = CreateFossilGene(gene, sedimentCoverage);
            if (fossilGene != null) // Only add genes that were actually preserved
            {
                fossil.Genes.Add(fossilGene);
            }
        }
        
        return fossil;
    }
    
    /// <summary>
    /// Creates a fossil gene with preserved sequence based on sediment coverage
    /// </summary>
    private FossilGene? CreateFossilGene(IGene gene, int maxPreservedLength)
    {
        // If sediment coverage is 0, the gene is not preserved at all
        if (maxPreservedLength == 0)
        {
            return null; // Gene not preserved, don't include in fossil
        }
        
        var preservedSequence = new List<Nucleotide?>();
        
        // Preserve only the first N nucleotides based on sediment coverage
        for (int i = 0; i < Math.Min(maxPreservedLength, gene.Sequence.Count); i++)
        {
            preservedSequence.Add(gene.Sequence[i]);
        }
        
        // Don't fill with null - genes should have their natural preserved length
        
        return new FossilGene(preservedSequence);
    }
    
    /// <summary>
    /// Simulates binomial distribution (sum of n Bernoulli trials with probability p)
    /// </summary>
    private int SimulateBinomialDistribution(int n, double p, Random random)
    {
        int successes = 0;
        for (int i = 0; i < n; i++)
        {
            if (random.NextDouble() < p)
                successes++;
        }
        return successes;
    }
    
    /// <summary>
    /// Applies decay to all fossils based on their age
    /// </summary>
    public void ApplyDecay(int currentGeneration, Random random)
    {
        var fossilsToRemove = new List<Fossil>();
        
        foreach (var fossil in _fossils)
        {
            var age = currentGeneration - fossil.GenerationFormed;
            
            // Apply nucleotide decay (chemical decay)
            foreach (var gene in fossil.Genes)
            {
                ApplyNucleotideDecay(gene, age, random);
            }
            
            // Apply physical damage (0.1% chance per fossil)
            if (random.NextDouble() < _physicalDamageProbability)
            {
                ApplyPhysicalDamage(fossil, random);
            }
            
            // Remove fossils with no genes left
            if (fossil.Genes.Count == 0)
            {
                fossilsToRemove.Add(fossil);
            }
        }
        
        // Remove completely lost fossils
        foreach (var fossil in fossilsToRemove)
        {
            _fossils.Remove(fossil);
        }
    }
    
    /// <summary>
    /// Applies physical damage to a fossil
    /// </summary>
    private void ApplyPhysicalDamage(Fossil fossil, Random random)
    {
        // Count total nucleotides in all preserved sequences (including decayed ones)
        var totalNucleotides = fossil.Genes.Sum(g => g.PreservedSequence.Length);
        
        if (totalNucleotides == 0) return;
        
        // Roll for damage type
        var damageRoll = random.NextDouble();
        
        if (damageRoll < 1.0 / 20.0) // 0 ~ 1/20: No damage
        {
            return; // No damage
        }
        else if (damageRoll < 19.0 / 20.0) // 1/20 ~ 19/20: Proportional damage
        {
            // Calculate number of d5 dice to roll: nucleotides * damageRoll * 0.5
            var diceCount = (int)(totalNucleotides * damageRoll * 0.5);
            
            // Simulate rolling diceCount d5 dice and sum the results
            var totalDamage = SimulateD5Rolls(diceCount, random);
            
            // Apply damage by removing nucleotides from the end of genes
            ApplyPhysicalDamageToGenes(fossil, totalDamage, random);
        }
        else // 19/20 ~ 1: Total fossil loss
        {
            fossil.Genes.Clear();
        }
        
        // Remove fossil if no nucleotides left
        var remainingNucleotides = fossil.Genes.Sum(g => g.PreservedSequence.Length);
        if (remainingNucleotides == 0)
        {
            fossil.Genes.Clear();
        }
    }
    
    /// <summary>
    /// Simulates rolling multiple d5 dice and returns the sum
    /// d5 values: [0, 1, 1, 2, 3]
    /// </summary>
    private int SimulateD5Rolls(int diceCount, Random random)
    {
        int totalDamage = 0;
        var d5Values = new int[] { 0, 1, 1, 2, 3 };
        
        for (int i = 0; i < diceCount; i++)
        {
            totalDamage += d5Values[random.Next(d5Values.Length)];
        }
        return totalDamage;
    }
    
    /// <summary>
    /// Applies physical damage by removing nucleotides from the end of genes
    /// </summary>
    private void ApplyPhysicalDamageToGenes(Fossil fossil, int damageAmount, Random random)
    {
        var remainingDamage = damageAmount;
        
        // Process genes in random order
        var genesToProcess = fossil.Genes.OrderBy(x => random.Next()).ToList();
        
        foreach (var gene in genesToProcess)
        {
            if (remainingDamage <= 0) break;
            
            var currentLength = gene.PreservedSequence.Length;
            var nucleotidesToRemove = Math.Min(remainingDamage, currentLength);
            
            if (nucleotidesToRemove > 0)
            {
                // Remove nucleotides from the end
                gene.PreservedSequence = gene.PreservedSequence.Substring(0, currentLength - nucleotidesToRemove);
                remainingDamage -= nucleotidesToRemove;
            }
        }
    }
    
    /// <summary>
    /// Applies nucleotide decay based on half-life
    /// </summary>
    private void ApplyNucleotideDecay(FossilGene gene, int age, Random random)
    {
        if (gene.PreservedSequence.All(c => c == '*')) return;
        
        // Calculate decay probability per generation: p = 1 - 0.5^(1/halfLife)
        var decayProbabilityPerGeneration = 1.0 - Math.Pow(0.5, 1.0 / _fossilHalfLife);
        
        var sequenceArray = gene.PreservedSequence.ToCharArray();
        for (int i = 0; i < sequenceArray.Length; i++)
        {
            if (sequenceArray[i] != '*')
            {
                // Apply decay for each generation of age
                var totalDecayProbability = 1.0 - Math.Pow(1.0 - decayProbabilityPerGeneration, age);
                
                if (random.NextDouble() < totalDecayProbability)
                {
                    sequenceArray[i] = '*'; // Nucleotide decayed
                }
            }
        }
        gene.PreservedSequence = new string(sequenceArray);
    }
    
    
    /// <summary>
    /// Saves fossils to JSON file
    /// </summary>
    public void SaveToJson(string filePath)
    {
        var fossilData = new
        {
            fossils = _fossils.Select(f => new
            {
                position = f.Position,
                generation = f.GenerationFormed,
                sequences = f.Genes.Select(g => g.PreservedSequence).ToArray()
            }).ToArray(),
            metadata = new
            {
                totalFossils = _fossils.Count,
                totalGenes = _fossils.SelectMany(f => f.Genes).Count(),
                fossilizationProbability = _fossilizationProbability,
                fossilHalfLife = _fossilHalfLife,
                decayApplied = true
            }
        };
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var json = JsonSerializer.Serialize(fossilData, options);
        File.WriteAllText(filePath, json);
    }
    
    /// <summary>
    /// Loads fossils from JSON file
    /// </summary>
    public void LoadFromJson(string filePath)
    {
        if (!File.Exists(filePath))
            return;
            
        var json = File.ReadAllText(filePath);
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var fossilData = JsonSerializer.Deserialize<JsonElement>(json, options);
        
        _fossils.Clear();
        
        if (fossilData.TryGetProperty("fossils", out var fossilsArray))
        {
            foreach (var fossilElement in fossilsArray.EnumerateArray())
            {
                var position = fossilElement.GetProperty("position").GetInt32();
                var generation = fossilElement.GetProperty("generation").GetInt32();
                
                var fossil = new Fossil(position, generation, 0, $"LoadedFossil_{position}_{generation}");
                
                if (fossilElement.TryGetProperty("sequences", out var sequencesArray))
                {
                    foreach (var sequenceElement in sequencesArray.EnumerateArray())
                    {
                        var sequence = sequenceElement.GetString() ?? "";
                        var fossilGene = new FossilGene(sequence);
                        fossil.Genes.Add(fossilGene);
                    }
                }
                
                _fossils.Add(fossil);
            }
        }
    }
    
    /// <summary>
    /// Gets statistics about the fossil record
    /// </summary>
    public FossilStatistics GetStatistics()
    {
        var stats = new FossilStatistics
        {
            TotalFossils = _fossils.Count,
            AverageGenesPerFossil = _fossils.Any() ? _fossils.Average(f => f.Genes.Count) : 0,
            AveragePreservedLength = _fossils.Any() ? _fossils.Average(f => f.MaxPreservedLength) : 0
        };
        
        // Count genes (no type information available in fossils)
        var allGenes = _fossils.SelectMany(f => f.Genes).ToList();
        stats.SelectedGenesCount = 0; // Not available in fossil record
        stats.NeutralGenesCount = 0; // Not available in fossil record
        stats.EmptyGenesCount = allGenes.Count(g => g.PreservedSequence.All(c => c == '*'));
        
        // Calculate preservation rates
        if (allGenes.Any())
        {
            stats.PreservationRate = (double)(allGenes.Count - stats.EmptyGenesCount) / allGenes.Count;
        }
        
        return stats;
    }
}

/// <summary>
/// Statistics about the fossil record
/// </summary>
public class FossilStatistics
{
    public int TotalFossils { get; set; }
    public double AverageGenesPerFossil { get; set; }
    public double AveragePreservedLength { get; set; }
    public int SelectedGenesCount { get; set; }
    public int NeutralGenesCount { get; set; }
    public int EmptyGenesCount { get; set; }
    public double PreservationRate { get; set; }
}
