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
    private readonly double _totalLossProbability;
    private readonly double _geneLossProbability;
    private readonly double _partialDamageProbability;
    
    public IReadOnlyList<Fossil> Fossils => _fossils.AsReadOnly();
    public int TotalFossils => _fossils.Count;
    
    public FossilManager(double fossilizationProbability = 0.01, int fossilHalfLife = 50)
    {
        _fossils = new List<Fossil>();
        _fossilizationProbability = fossilizationProbability;
        _fossilHalfLife = fossilHalfLife;
        
        // Decay probabilities (per generation of age)
        _totalLossProbability = 0.00001;
        _geneLossProbability = 0.0001;
        _partialDamageProbability = 0.001;
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
        
        // Fill remaining slots with null to represent unpreserved nucleotides
        while (preservedSequence.Count < maxPreservedLength)
        {
            preservedSequence.Add(null);
        }
        
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
            
            // Apply total fossil loss
            if (ShouldApplyTotalLoss(age, random))
            {
                fossilsToRemove.Add(fossil);
                continue;
            }
            
            // Apply decay to each gene
            var genesToRemove = new List<FossilGene>();
            
            foreach (var gene in fossil.Genes)
            {
                if (ShouldApplyGeneLoss(age, random))
                {
                    genesToRemove.Add(gene);
                    continue;
                }
                
                // Apply nucleotide decay and partial damage
                ApplyNucleotideDecay(gene, age, random);
                ApplyPartialDamage(gene, age, random);
            }
            
            // Remove lost genes from fossil
            foreach (var gene in genesToRemove)
            {
                fossil.Genes.Remove(gene);
            }
        }
        
        // Remove completely lost fossils
        foreach (var fossil in fossilsToRemove)
        {
            _fossils.Remove(fossil);
        }
    }
    
    /// <summary>
    /// Determines if a fossil should be completely lost
    /// </summary>
    private bool ShouldApplyTotalLoss(int age, Random random)
    {
        var probability = 1.0 - Math.Pow(1.0 - _totalLossProbability, age);
        return random.NextDouble() < probability;
    }
    
    /// <summary>
    /// Determines if a gene should be completely lost
    /// </summary>
    private bool ShouldApplyGeneLoss(int age, Random random)
    {
        var probability = 1.0 - Math.Pow(1.0 - _geneLossProbability, age);
        return random.NextDouble() < probability;
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
    /// Applies partial damage (removes nucleotides from the end)
    /// </summary>
    private void ApplyPartialDamage(FossilGene gene, int age, Random random)
    {
        if (gene.PreservedSequence.All(c => c == '*')) return;
        
        var probability = 1.0 - Math.Pow(1.0 - _partialDamageProbability, age);
        
        if (random.NextDouble() < probability)
        {
            // Find the last non-* nucleotide and replace it with *
            var sequenceArray = gene.PreservedSequence.ToCharArray();
            for (int i = sequenceArray.Length - 1; i >= 0; i--)
            {
                if (sequenceArray[i] != '*')
                {
                    sequenceArray[i] = '*';
                    break;
                }
            }
            gene.PreservedSequence = new string(sequenceArray);
        }
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
                generationFormed = f.GenerationFormed,
                maxPreservedLength = f.MaxPreservedLength,
                organismId = f.OrganismId,
                genes = f.Genes.Select(g => new
                {
                    preservedSequence = g.PreservedSequence
                }).ToArray()
            }).ToArray(),
            metadata = new
            {
                totalFossils = _fossils.Count,
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
                var generationFormed = fossilElement.GetProperty("generationFormed").GetInt32();
                var maxPreservedLength = fossilElement.GetProperty("maxPreservedLength").GetInt32();
                var organismId = fossilElement.GetProperty("organismId").GetString() ?? "";
                
                var fossil = new Fossil(position, generationFormed, maxPreservedLength, organismId);
                
                if (fossilElement.TryGetProperty("genes", out var genesArray))
                {
                    foreach (var geneElement in genesArray.EnumerateArray())
                    {
                        var preservedSequence = "";
                        if (geneElement.TryGetProperty("preservedSequence", out var sequenceProperty))
                        {
                            preservedSequence = sequenceProperty.GetString() ?? "";
                        }
                        
                        var fossilGene = new FossilGene(preservedSequence);
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
