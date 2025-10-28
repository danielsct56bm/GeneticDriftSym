using SimulationEvolucion.Core.Interfaces;

namespace SimulationEvolucion.Core.Models;

/// <summary>
/// Represents a phylogenetic tree (cladogram) showing evolutionary relationships
/// </summary>
public class Cladogram
{
    public CladogramNode Root { get; private set; }
    public List<CladogramNode> AllNodes { get; private set; }
    public List<CladogramNode> LeafNodes { get; private set; }
    public List<CladogramNode> FossilNodes { get; private set; }
    public List<CladogramNode> LivingNodes { get; private set; }
    
    public int TotalNodes => AllNodes.Count;
    public int TotalLeaves => LeafNodes.Count;
    public int TotalFossils => FossilNodes.Count;
    public int TotalLiving => LivingNodes.Count;
    
    public Cladogram(CladogramNode root)
    {
        Root = root;
        AllNodes = new List<CladogramNode>();
        LeafNodes = new List<CladogramNode>();
        FossilNodes = new List<CladogramNode>();
        LivingNodes = new List<CladogramNode>();
        
        BuildNodeLists();
    }
    
    /// <summary>
    /// Builds all node lists by traversing the tree
    /// </summary>
    private void BuildNodeLists()
    {
        AllNodes.Clear();
        LeafNodes.Clear();
        FossilNodes.Clear();
        LivingNodes.Clear();
        
        TraverseTree(Root, node =>
        {
            AllNodes.Add(node);
            
            if (node.IsLeaf)
            {
                LeafNodes.Add(node);
                
                if (node.IsFossil)
                    FossilNodes.Add(node);
                else
                    LivingNodes.Add(node);
            }
        });
    }
    
    /// <summary>
    /// Traverses the tree and applies action to each node
    /// </summary>
    private void TraverseTree(CladogramNode node, Action<CladogramNode> action)
    {
        action(node);
        
        foreach (var child in node.Children)
        {
            TraverseTree(child, action);
        }
    }
    
    /// <summary>
    /// Calculates the total tree length (sum of all branch lengths)
    /// </summary>
    public double CalculateTreeLength()
    {
        double totalLength = 0;
        
        TraverseTree(Root, node =>
        {
            if (node.Parent != null)
            {
                totalLength += node.Distance;
            }
        });
        
        return totalLength;
    }
    
    /// <summary>
    /// Calculates the depth of the tree
    /// </summary>
    public int CalculateTreeDepth()
    {
        int maxDepth = 0;
        
        TraverseTree(Root, node =>
        {
            if (node.Depth > maxDepth)
                maxDepth = node.Depth;
        });
        
        return maxDepth;
    }
    
    /// <summary>
    /// Finds the most recent common ancestor of two nodes
    /// </summary>
    public CladogramNode? FindMRCA(CladogramNode node1, CladogramNode node2)
    {
        var path1 = node1.GetPathToRoot();
        var path2 = node2.GetPathToRoot();
        
        // Find the last common node in both paths
        CladogramNode? mrca = null;
        int minLength = Math.Min(path1.Count, path2.Count);
        
        for (int i = 0; i < minLength; i++)
        {
            if (path1[i].Id == path2[i].Id)
            {
                mrca = path1[i];
            }
            else
            {
                break;
            }
        }
        
        return mrca;
    }
    
    /// <summary>
    /// Calculates the distance between two nodes
    /// </summary>
    public double CalculateNodeDistance(CladogramNode node1, CladogramNode node2)
    {
        var mrca = FindMRCA(node1, node2);
        if (mrca == null) return double.MaxValue;
        
        double distance1 = 0;
        double distance2 = 0;
        
        var current1 = node1;
        while (current1 != mrca && current1.Parent != null)
        {
            distance1 += current1.Distance;
            current1 = current1.Parent;
        }
        
        var current2 = node2;
        while (current2 != mrca && current2.Parent != null)
        {
            distance2 += current2.Distance;
            current2 = current2.Parent;
        }
        
        return distance1 + distance2;
    }
    
    /// <summary>
    /// Gets all nodes at a specific depth
    /// </summary>
    public List<CladogramNode> GetNodesAtDepth(int depth)
    {
        var nodesAtDepth = new List<CladogramNode>();
        
        TraverseTree(Root, node =>
        {
            if (node.Depth == depth)
                nodesAtDepth.Add(node);
        });
        
        return nodesAtDepth;
    }
    
    /// <summary>
    /// Gets statistics about the cladogram
    /// </summary>
    public CladogramStatistics GetStatistics()
    {
        var stats = new CladogramStatistics
        {
            TotalNodes = TotalNodes,
            TotalLeaves = TotalLeaves,
            TotalFossils = TotalFossils,
            TotalLiving = TotalLiving,
            TreeDepth = CalculateTreeDepth(),
            TreeLength = CalculateTreeLength()
        };
        
        // Calculate generation range
        var generations = LeafNodes.Where(n => n.Generation.HasValue).Select(n => n.Generation!.Value).ToList();
        if (generations.Any())
        {
            stats.MinGeneration = generations.Min();
            stats.MaxGeneration = generations.Max();
            stats.GenerationSpan = stats.MaxGeneration - stats.MinGeneration;
        }
        
        // Calculate position range
        var positions = LeafNodes.Where(n => n.Position.HasValue).Select(n => n.Position!.Value).ToList();
        if (positions.Any())
        {
            stats.MinPosition = positions.Min();
            stats.MaxPosition = positions.Max();
            stats.PositionSpan = stats.MaxPosition - stats.MinPosition;
        }
        
        return stats;
    }
    
    /// <summary>
    /// Exports the cladogram to a simple text format
    /// </summary>
    public string ExportToText()
    {
        var output = new System.Text.StringBuilder();
        output.AppendLine("=== CLADOGRAMA FILOGENÉTICO ===");
        output.AppendLine($"Total nodos: {TotalNodes}");
        output.AppendLine($"Total hojas: {TotalLeaves} ({TotalFossils} fósiles, {TotalLiving} vivos)");
        output.AppendLine($"Profundidad del árbol: {CalculateTreeDepth()}");
        output.AppendLine($"Longitud total del árbol: {CalculateTreeLength():F3}");
        output.AppendLine();
        
        // Export tree structure
        ExportNodeToText(Root, output, 0);
        
        return output.ToString();
    }
    
    private void ExportNodeToText(CladogramNode node, System.Text.StringBuilder output, int indent)
    {
        var indentStr = new string(' ', indent * 2);
        var nodeInfo = node.IsLeaf ? 
            $"{node} (Dist: {node.Distance:F3})" : 
            $"Nodo interno {node.Id} (Dist: {node.Distance:F3})";
        
        output.AppendLine($"{indentStr}{nodeInfo}");
        
        foreach (var child in node.Children)
        {
            ExportNodeToText(child, output, indent + 1);
        }
    }
    
    /// <summary>
    /// Exports the cladogram to Newick format
    /// </summary>
    public string ExportToNewick()
    {
        return ExportNodeToNewick(Root) + ";";
    }
    
    private string ExportNodeToNewick(CladogramNode node)
    {
        if (node.IsLeaf)
        {
            var label = node.IsFossil ? $"F_{node.Generation}_{node.Position}" : $"L_{node.Generation}_{node.Position}";
            return $"{label}:{node.Distance:F3}";
        }
        else
        {
            var children = node.Children.Select(child => ExportNodeToNewick(child)).ToList();
            return $"({string.Join(",", children)}):{node.Distance:F3}";
        }
    }
}

/// <summary>
/// Statistics about a cladogram
/// </summary>
public class CladogramStatistics
{
    public int TotalNodes { get; set; }
    public int TotalLeaves { get; set; }
    public int TotalFossils { get; set; }
    public int TotalLiving { get; set; }
    public int TreeDepth { get; set; }
    public double TreeLength { get; set; }
    public int? MinGeneration { get; set; }
    public int? MaxGeneration { get; set; }
    public int? GenerationSpan { get; set; }
    public int? MinPosition { get; set; }
    public int? MaxPosition { get; set; }
    public int? PositionSpan { get; set; }
}
