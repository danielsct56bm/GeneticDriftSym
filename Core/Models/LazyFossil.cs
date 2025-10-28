using SimulationEvolucion.Core.Enums;
using SimulationEvolucion.Core.Interfaces;

namespace SimulationEvolucion.Core.Models;

/// <summary>
/// Optimized fossil that calculates damage only at the end of simulation
/// </summary>
public class LazyFossil
{
    public string OriginalSequence { get; private set; }
    public int GenerationFormed { get; private set; }
    public int Position { get; private set; }
    public string OrganismId { get; private set; }
    public string CurrentSequence { get; set; }
    public bool IsEliminated { get; private set; }
    
    // Configuration for damage calculation
    private readonly Random _random;
    private readonly double _decayProbabilityPerGeneration;
    private readonly double _physicalDamageProbability;
    private readonly int _fossilHalfLife;
    
    public LazyFossil(IOrganism organism, int generation, Random random, 
                     double decayProb, double physicalDamageProb, int halfLife)
    {
        OriginalSequence = CreateSequenceFromOrganism(organism);
        GenerationFormed = generation;
        Position = organism.Position;
        OrganismId = organism.Id;
        CurrentSequence = OriginalSequence;
        _random = random;
        _decayProbabilityPerGeneration = decayProb;
        _physicalDamageProbability = physicalDamageProb;
        _fossilHalfLife = halfLife;
    }
    
    /// <summary>
    /// Creates a single sequence string from all organism genes
    /// </summary>
    private string CreateSequenceFromOrganism(IOrganism organism)
    {
        var sequences = organism.Genes.Select(g => 
            string.Join("", g.Sequence.Select(n => n.ToString()))).ToList();
        return string.Join("|", sequences); // Use | as gene separator
    }
    
    /// <summary>
    /// Main method: calculate all accumulated damage at once
    /// </summary>
    public void ApplyAccumulatedDamage(int finalGeneration)
    {
        var totalAge = finalGeneration - GenerationFormed;
        if (totalAge <= 0) return;
        
        // 1. Apply accumulated chemical decay
        ApplyAccumulatedChemicalDecay(totalAge);
        
        // 2. Apply accumulated physical damage
        ApplyAccumulatedPhysicalDamage(totalAge);
        
        // 3. Check for elimination
        CheckForElimination();
    }
    
    private void ApplyAccumulatedChemicalDecay(int totalAge)
    {
        var sequenceArray = CurrentSequence.ToCharArray();
        
        for (int i = 0; i < sequenceArray.Length; i++)
        {
            if (sequenceArray[i] != '*' && sequenceArray[i] != '|')
            {
                // Calculate total decay probability for entire age
                var totalDecayProbability = 1.0 - Math.Pow(1.0 - _decayProbabilityPerGeneration, totalAge);
                
                if (_random.NextDouble() < totalDecayProbability)
                {
                    sequenceArray[i] = '*';
                }
            }
        }
        
        CurrentSequence = new string(sequenceArray);
    }
    
    private void ApplyAccumulatedPhysicalDamage(int totalAge)
    {
        // Calculate how many physical damage events occurred in total
        var expectedDamageEvents = totalAge * _physicalDamageProbability;
        var actualDamageEvents = SimulatePoissonDistribution(expectedDamageEvents, _random);
        
        for (int damageEvent = 0; damageEvent < actualDamageEvents; damageEvent++)
        {
            ApplySinglePhysicalDamageEvent();
        }
    }
    
    private void ApplySinglePhysicalDamageEvent()
    {
        if (CurrentSequence.All(c => c == '*' || c == '|')) return;
        
        var damageRoll = _random.NextDouble();
        
        if (damageRoll < 1.0 / 20.0) // 5%: No damage
        {
            return;
        }
        else if (damageRoll < 19.0 / 20.0) // 90%: Proportional damage
        {
            var totalNucleotides = CurrentSequence.Count(c => c != '*' && c != '|');
            var diceCount = (int)(totalNucleotides * damageRoll * 0.5);
            var totalDamage = SimulateD5Rolls(diceCount, _random);
            
            // Remove nucleotides from the end (but preserve gene separators)
            ApplyPhysicalDamageToSequence(totalDamage);
        }
        else // 5%: Total loss
        {
            CurrentSequence = "";
        }
    }
    
    private void ApplyPhysicalDamageToSequence(int damageAmount)
    {
        var sequenceArray = CurrentSequence.ToCharArray();
        var remainingDamage = damageAmount;
        
        // Remove from the end, skipping separators
        for (int i = sequenceArray.Length - 1; i >= 0 && remainingDamage > 0; i--)
        {
            if (sequenceArray[i] != '|' && sequenceArray[i] != '*')
            {
                sequenceArray[i] = '*';
                remainingDamage--;
            }
        }
        
        CurrentSequence = new string(sequenceArray);
    }
    
    private void CheckForElimination()
    {
        var nucleotides = CurrentSequence.Count(c => c != '*' && c != '|');
        IsEliminated = nucleotides == 0;
    }
    
    /// <summary>
    /// Simulate Poisson distribution for physical damage events
    /// </summary>
    private int SimulatePoissonDistribution(double lambda, Random random)
    {
        if (lambda <= 0) return 0;
        
        int k = 0;
        double p = 1.0;
        double L = Math.Exp(-lambda);
        
        do
        {
            k++;
            p *= random.NextDouble();
        } while (p > L);
        
        return k - 1;
    }
    
    /// <summary>
    /// Simulate rolling multiple d5 dice and returns the sum
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
    /// Get individual gene sequences from the combined sequence
    /// </summary>
    public List<string> GetGeneSequences()
    {
        return CurrentSequence.Split('|').ToList();
    }
    
    public override string ToString()
    {
        return $"LazyFossil(Gen{GenerationFormed}): {CurrentSequence}";
    }
}
