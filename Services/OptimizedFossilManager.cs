using SimulationEvolucion.Core.Enums;
using SimulationEvolucion.Core.Interfaces;
using SimulationEvolucion.Core.Models;
using System.Text.Json;

namespace SimulationEvolucion.Services;

/// <summary>
/// Optimized fossil manager that calculates damage only at the end of simulation
/// </summary>
public class OptimizedFossilManager
{
    private readonly List<LazyFossil> _fossils;
    private readonly double _fossilizationProbability;
    private readonly int _fossilHalfLife;
    private readonly double _physicalDamageProbability;
    
    public IReadOnlyList<LazyFossil> Fossils => _fossils.AsReadOnly();
    public int TotalFossils => _fossils.Count;
    
    public OptimizedFossilManager(double fossilizationProbability = 0.01, int fossilHalfLife = 50)
    {
        _fossils = new List<LazyFossil>();
        _fossilizationProbability = fossilizationProbability;
        _fossilHalfLife = fossilHalfLife;
        _physicalDamageProbability = 0.001; // 0.1% chance of physical damage per fossil
    }
    
    /// <summary>
    /// Records the death of an organism with a chance of fossilization
    /// During simulation: only creates fossils, no damage calculation
    /// </summary>
    public void RecordDeath(IOrganism organism, int generation, Random random)
    {
        if (random.NextDouble() < _fossilizationProbability)
        {
            // Calculate decay probability per generation: p = 1 - 0.5^(1/halfLife)
            var decayProbabilityPerGeneration = 1.0 - Math.Pow(0.5, 1.0 / _fossilHalfLife);
            
            var fossil = new LazyFossil(organism, generation, random, 
                                       decayProbabilityPerGeneration, 
                                       _physicalDamageProbability, 
                                       _fossilHalfLife);
            _fossils.Add(fossil);
        }
    }
    
    /// <summary>
    /// Apply all accumulated damage to fossils at the end of simulation
    /// This is where all the heavy computation happens - only once!
    /// </summary>
    public void ApplyFinalDamage(int finalGeneration)
    {
        Console.WriteLine($"Applying final damage to {_fossils.Count} fossils...");
        
        foreach (var fossil in _fossils)
        {
            fossil.ApplyAccumulatedDamage(finalGeneration);
        }
        
        // Remove eliminated fossils
        var eliminatedCount = _fossils.Count(f => f.IsEliminated);
        _fossils.RemoveAll(f => f.IsEliminated);
        
        Console.WriteLine($"Fossil damage applied. {eliminatedCount} fossils eliminated, {_fossils.Count} remaining.");
    }
    
    /// <summary>
    /// During simulation: do nothing (super fast!)
    /// </summary>
    public void UpdateFossils(int currentGeneration)
    {
        // No damage calculation during simulation - everything is calculated at the end
        // This method exists for compatibility but does nothing
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
                organismId = f.OrganismId,
                sequences = f.GetGeneSequences().ToArray()
            }).ToArray(),
            metadata = new
            {
                totalFossils = _fossils.Count,
                totalGenes = _fossils.SelectMany(f => f.GetGeneSequences()).Count(),
                fossilizationProbability = _fossilizationProbability,
                fossilHalfLife = _fossilHalfLife,
                optimized = true,
                damageApplied = true
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
                var organismId = fossilElement.TryGetProperty("organismId", out var idProp) 
                    ? idProp.GetString() ?? $"LoadedFossil_{position}_{generation}"
                    : $"LoadedFossil_{position}_{generation}";
                
                // Create a dummy organism for the fossil
                var dummyOrganism = CreateDummyOrganism(position, organismId);
                var fossil = new LazyFossil(dummyOrganism, generation, new Random(), 
                                         0.01, 0.001, _fossilHalfLife);
                
                if (fossilElement.TryGetProperty("sequences", out var sequencesArray))
                {
                    var sequences = sequencesArray.EnumerateArray()
                        .Select(s => s.GetString() ?? "")
                        .ToArray();
                    fossil.CurrentSequence = string.Join("|", sequences);
                }
                
                _fossils.Add(fossil);
            }
        }
    }
    
    private IOrganism CreateDummyOrganism(int position, string id)
    {
        // Create a minimal organism for loading fossils
        var genes = new List<IGene>();
        var dummyGene = new Gene(GeneType.Neutral, 20, new Random());
        genes.Add(dummyGene);
        
        return new Organism(genes, position) { Id = id };
    }
    
    /// <summary>
    /// Gets all fossils as a list
    /// </summary>
    public List<LazyFossil> GetAllFossils()
    {
        return new List<LazyFossil>(_fossils);
    }
    
    /// <summary>
    /// Gets statistics about the fossil record
    /// </summary>
    public OptimizedFossilStatistics GetStatistics()
    {
        var stats = new OptimizedFossilStatistics
        {
            TotalFossils = _fossils.Count,
            AverageGenesPerFossil = _fossils.Any() ? _fossils.Average(f => f.GetGeneSequences().Count) : 0,
            AveragePreservedLength = _fossils.Any() ? _fossils.Average(f => f.CurrentSequence.Length) : 0
        };
        
        // Count genes and preservation rates
        var allGenes = _fossils.SelectMany(f => f.GetGeneSequences()).ToList();
        stats.EmptyGenesCount = allGenes.Count(g => g.All(c => c == '*'));
        
        if (allGenes.Any())
        {
            stats.PreservationRate = (double)(allGenes.Count - stats.EmptyGenesCount) / allGenes.Count;
        }
        
        return stats;
    }
}

/// <summary>
/// Statistics about the optimized fossil record
/// </summary>
public class OptimizedFossilStatistics
{
    public int TotalFossils { get; set; }
    public double AverageGenesPerFossil { get; set; }
    public double AveragePreservedLength { get; set; }
    public int SelectedGenesCount { get; set; }
    public int NeutralGenesCount { get; set; }
    public int EmptyGenesCount { get; set; }
    public double PreservationRate { get; set; }
    public bool Optimized { get; set; } = true;
}
