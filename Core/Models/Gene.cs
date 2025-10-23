using SimulationEvolucion.Core.Enums;
using SimulationEvolucion.Core.Interfaces;

namespace SimulationEvolucion.Core.Models;

/// <summary>
/// Implementación de un gen con capacidad de mutación
/// </summary>
public class Gene : IGene
{
    public List<Nucleotide> Sequence { get; private set; }
    public GeneType Type { get; private set; }
    public string Id { get; private set; }
    
    private static int _nextId = 1;
    
    public Gene(List<Nucleotide> sequence, GeneType type)
    {
        Sequence = new List<Nucleotide>(sequence);
        Type = type;
        Id = $"Gene_{_nextId++}";
    }
    
    public Gene(GeneType type, int length, Random random, string? initialSequence = null) 
        : this(initialSequence != null ? ParseSequence(initialSequence) : GenerateRandomSequence(length, random), type)
    {
    }
    
    private static List<Nucleotide> ParseSequence(string sequence)
    {
        var result = new List<Nucleotide>();
        foreach (char c in sequence.ToUpper())
        {
            switch (c)
            {
                case 'A': result.Add(Nucleotide.A); break;
                case 'T': result.Add(Nucleotide.T); break;
                case 'G': result.Add(Nucleotide.G); break;
                case 'C': result.Add(Nucleotide.C); break;
                default: throw new ArgumentException($"Nucleótido inválido: {c}");
            }
        }
        return result;
    }
    
    private static List<Nucleotide> GenerateRandomSequence(int length, Random random)
    {
        var nucleotides = Enum.GetValues<Nucleotide>();
        var sequence = new List<Nucleotide>();
        
        for (int i = 0; i < length; i++)
        {
            sequence.Add(nucleotides[random.Next(nucleotides.Length)]);
        }
        
        return sequence;
    }
    
    public void Mutate(MutationType mutationType, Random random)
    {
        switch (mutationType)
        {
            case MutationType.Insertion:
                PerformInsertion(random);
                break;
            case MutationType.Duplication:
                PerformDuplication(random);
                break;
            case MutationType.Rotation:
                PerformRotation(random);
                break;
            case MutationType.Deletion:
                PerformDeletion(random);
                break;
        }
    }
    
    private void PerformInsertion(Random random)
    {
        if (Sequence.Count == 0) return;
        
        var nucleotides = Enum.GetValues<Nucleotide>();
        var newNucleotide = nucleotides[random.Next(nucleotides.Length)];
        var position = random.Next(Sequence.Count + 1);
        
        Sequence.Insert(position, newNucleotide);
    }
    
    private void PerformDuplication(Random random)
    {
        if (Sequence.Count == 0) return;
        
        var startPos = random.Next(Sequence.Count);
        var length = random.Next(1, Math.Min(Sequence.Count - startPos + 1, 5)); // Máximo 4 nucleótidos
        
        var segment = Sequence.GetRange(startPos, length);
        var insertPos = random.Next(Sequence.Count + 1);
        
        Sequence.InsertRange(insertPos, segment);
    }
    
    private void PerformRotation(Random random)
    {
        if (Sequence.Count < 2) return;
        
        var startPos = random.Next(Sequence.Count - 1);
        var length = random.Next(2, Math.Min(Sequence.Count - startPos + 1, 6)); // Máximo 5 nucleótidos
        
        var segment = Sequence.GetRange(startPos, length);
        var rotated = new List<Nucleotide>(segment);
        
        // Rotar 1 posición hacia la derecha
        var last = rotated[^1];
        rotated.RemoveAt(rotated.Count - 1);
        rotated.Insert(0, last);
        
        // Reemplazar en la secuencia original
        for (int i = 0; i < length; i++)
        {
            Sequence[startPos + i] = rotated[i];
        }
    }
    
    private void PerformDeletion(Random random)
    {
        if (Sequence.Count <= 1) return;
        
        var length = random.Next(1, Math.Min(Sequence.Count, 4)); // Máximo 3 nucleótidos
        var startPos = random.Next(Sequence.Count - length + 1);
        
        Sequence.RemoveRange(startPos, length);
    }
    
    public double CalculateFitness()
    {
        // Para genes seleccionados, calculamos fitness basado en patrones
        // Para genes neutrales, fitness constante
        if (Type == GeneType.Neutral)
        {
            return 1.0; // Fitness neutral
        }
        
        // Fitness para genes seleccionados basado en contenido de GC y patrones
        var gcCount = Sequence.Count(n => n == Nucleotide.G || n == Nucleotide.C);
        var gcContent = (double)gcCount / Sequence.Count;
        
        // Fitness óptimo alrededor de 50% GC
        var fitness = 1.0 - Math.Abs(gcContent - 0.5) * 2.0;
        
        // Bonus por secuencias repetitivas (simulando elementos funcionales)
        var repeats = CountRepeats();
        fitness += repeats * 0.1;
        
        return Math.Max(0.1, fitness); // Fitness mínimo de 0.1
    }
    
    private int CountRepeats()
    {
        int repeats = 0;
        for (int i = 0; i < Sequence.Count - 2; i++)
        {
            if (Sequence[i] == Sequence[i + 1] && Sequence[i + 1] == Sequence[i + 2])
            {
                repeats++;
            }
        }
        return repeats;
    }
    
    public IGene Clone()
    {
        return new Gene(Sequence, Type);
    }
    
    public override string ToString()
    {
        var sequenceStr = string.Join("", Sequence.Select(n => n.ToString()));
        return $"{Id}({Type}): {sequenceStr}";
    }
}
