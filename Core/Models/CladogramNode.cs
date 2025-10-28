using SimulationEvolucion.Core.Enums;
using SimulationEvolucion.Core.Interfaces;

namespace SimulationEvolucion.Core.Models;

/// <summary>
/// Represents a node in the phylogenetic tree (cladogram)
/// </summary>
public class CladogramNode
{
    public string Id { get; set; }
    public string? OrganismId { get; set; } // null for internal nodes
    public int? Generation { get; set; } // null for internal nodes
    public int? Position { get; set; } // null for internal nodes
    public List<string>? GeneSequences { get; set; } // null for internal nodes
    public bool IsLeaf { get; set; }
    public bool IsFossil { get; set; }
    
    // Tree structure
    public CladogramNode? Parent { get; set; }
    public List<CladogramNode> Children { get; set; }
    public double Distance { get; set; } // Distance to parent
    
    // Visualization properties
    public int Depth { get; set; }
    public double X { get; set; } // X position for visualization
    public double Y { get; set; } // Y position for visualization
    
    public CladogramNode(string id, bool isLeaf = false, bool isFossil = false)
    {
        Id = id;
        IsLeaf = isLeaf;
        IsFossil = isFossil;
        Children = new List<CladogramNode>();
        Distance = 0;
        Depth = 0;
        X = 0;
        Y = 0;
    }
    
    /// <summary>
    /// Creates a leaf node for a living organism
    /// </summary>
    public static CladogramNode CreateLivingOrganismNode(IOrganism organism, int generation)
    {
        var node = new CladogramNode(organism.Id, isLeaf: true, isFossil: false)
        {
            OrganismId = organism.Id,
            Generation = generation,
            Position = organism.Position,
            GeneSequences = organism.Genes.Select(g => 
                string.Join("", g.Sequence.Select(n => n.ToString()))).ToList()
        };
        return node;
    }
    
    /// <summary>
    /// Creates a leaf node for a fossil
    /// </summary>
    public static CladogramNode CreateFossilNode(LazyFossil fossil)
    {
        var node = new CladogramNode(fossil.OrganismId, isLeaf: true, isFossil: true)
        {
            OrganismId = fossil.OrganismId,
            Generation = fossil.GenerationFormed,
            Position = fossil.Position,
            GeneSequences = fossil.GetGeneSequences()
        };
        return node;
    }
    
    /// <summary>
    /// Creates an internal node (ancestor)
    /// </summary>
    public static CladogramNode CreateInternalNode(string id, double distance = 0)
    {
        var node = new CladogramNode(id, isLeaf: false, isFossil: false)
        {
            Distance = distance
        };
        return node;
    }
    
    /// <summary>
    /// Adds a child node and sets up parent-child relationship
    /// </summary>
    public void AddChild(CladogramNode child)
    {
        child.Parent = this;
        child.Depth = this.Depth + 1;
        Children.Add(child);
    }
    
    /// <summary>
    /// Calculates genetic distance to another node using Hamming distance
    /// </summary>
    public double CalculateGeneticDistance(CladogramNode other)
    {
        if (!IsLeaf || !other.IsLeaf || GeneSequences == null || other.GeneSequences == null)
            return double.MaxValue;
        
        if (GeneSequences.Count != other.GeneSequences.Count)
            return double.MaxValue;
        
        double totalDistance = 0;
        int totalComparisons = 0;
        
        for (int i = 0; i < GeneSequences.Count; i++)
        {
            var seq1 = GeneSequences[i];
            var seq2 = other.GeneSequences[i];
            
            if (seq1.Length != seq2.Length)
                continue;
            
            int differences = 0;
            int validPositions = 0;
            
            for (int j = 0; j < seq1.Length; j++)
            {
                // Skip damaged positions (*) in fossils
                if (seq1[j] == '*' || seq2[j] == '*')
                    continue;
                
                validPositions++;
                if (seq1[j] != seq2[j])
                    differences++;
            }
            
            if (validPositions > 0)
            {
                totalDistance += (double)differences / validPositions;
                totalComparisons++;
            }
        }
        
        // Return a reasonable distance value, capped at 1.0
        return totalComparisons > 0 ? Math.Min(totalDistance / totalComparisons, 1.0) : 1.0;
    }
    
    /// <summary>
    /// Gets all leaf descendants
    /// </summary>
    public List<CladogramNode> GetLeafDescendants()
    {
        var leaves = new List<CladogramNode>();
        
        if (IsLeaf)
        {
            leaves.Add(this);
        }
        else
        {
            foreach (var child in Children)
            {
                leaves.AddRange(child.GetLeafDescendants());
            }
        }
        
        return leaves;
    }
    
    /// <summary>
    /// Gets the path from root to this node
    /// </summary>
    public List<CladogramNode> GetPathToRoot()
    {
        var path = new List<CladogramNode>();
        var current = this;
        
        while (current != null)
        {
            path.Add(current);
            current = current.Parent;
        }
        
        path.Reverse();
        return path;
    }
    
    public override string ToString()
    {
        if (IsLeaf)
        {
            var type = IsFossil ? "Fossil" : "Living";
            return $"{type} {Id} (Gen {Generation}, Pos {Position})";
        }
        else
        {
            return $"Internal Node {Id} (Depth {Depth})";
        }
    }
}
