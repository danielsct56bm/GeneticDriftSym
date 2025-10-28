using SimulationEvolucion.Core.Models;

namespace SimulationEvolucion.Core.Models;

/// <summary>
/// Enhanced visualizer for cladograms with ASCII art representation
/// </summary>
public class CladogramVisualizer
{
    private const char HorizontalLine = '─';
    private const char VerticalLine = '│';
    private const char Corner = '└';
    private const char Branch = '├';
    private const char Root = '┌';
    
    /// <summary>
    /// Creates an ASCII art visualization of the cladogram
    /// </summary>
    public string CreateAsciiVisualization(Cladogram cladogram)
    {
        var output = new System.Text.StringBuilder();
        
        output.AppendLine("🌳 CLADOGRAMA FILOGENÉTICO - VISUALIZACIÓN ASCII");
        output.AppendLine(new string('═', 60));
        
        var stats = cladogram.GetStatistics();
        output.AppendLine($"📊 Estadísticas: {stats.TotalLeaves} hojas ({stats.TotalFossils} fósiles, {stats.TotalLiving} vivos)");
        output.AppendLine($"📏 Profundidad: {stats.TreeDepth} | Longitud total: {stats.TreeLength:F3}");
        output.AppendLine();
        
        // Create ASCII tree
        var treeLines = CreateAsciiTree(cladogram.Root, "", true, true);
        foreach (var line in treeLines)
        {
            output.AppendLine(line);
        }
        
        output.AppendLine();
        output.AppendLine("🔍 Leyenda:");
        output.AppendLine("  🟢 = Organismo vivo");
        output.AppendLine("  🪨 = Fósil");
        output.AppendLine("  📍 = Nodo interno (ancestro común)");
        
        return output.ToString();
    }
    
    /// <summary>
    /// Creates ASCII tree representation recursively
    /// </summary>
    private List<string> CreateAsciiTree(CladogramNode node, string prefix, bool isLast, bool isRoot)
    {
        var lines = new List<string>();
        
        // Current node representation
        string nodeSymbol = GetNodeSymbol(node);
        string nodeInfo = GetNodeInfo(node);
        
        if (isRoot)
        {
            lines.Add($"{nodeSymbol} {nodeInfo}");
        }
        else
        {
            string connector = isLast ? Corner.ToString() : Branch.ToString();
            lines.Add($"{prefix}{connector}{HorizontalLine} {nodeSymbol} {nodeInfo}");
        }
        
        // Children
        if (node.Children.Any())
        {
            string childPrefix = isRoot ? "" : prefix + (isLast ? "  " : VerticalLine + " ");
            
            for (int i = 0; i < node.Children.Count; i++)
            {
                bool isLastChild = (i == node.Children.Count - 1);
                var childLines = CreateAsciiTree(node.Children[i], childPrefix, isLastChild, false);
                lines.AddRange(childLines);
            }
        }
        
        return lines;
    }
    
    /// <summary>
    /// Gets the appropriate symbol for a node
    /// </summary>
    private string GetNodeSymbol(CladogramNode node)
    {
        if (node.IsLeaf)
        {
            return node.IsFossil ? "🪨" : "🟢";
        }
        else
        {
            return "📍";
        }
    }
    
    /// <summary>
    /// Gets formatted information about a node
    /// </summary>
    private string GetNodeInfo(CladogramNode node)
    {
        if (node.IsLeaf)
        {
            var type = node.IsFossil ? "Fósil" : "Vivo";
            var gen = node.Generation?.ToString() ?? "?";
            var pos = node.Position?.ToString() ?? "?";
            var dist = node.Distance.ToString("F3");
            
            return $"{type} Gen{gen} Pos{pos} (d:{dist})";
        }
        else
        {
            var dist = node.Distance.ToString("F3");
            var leafCount = node.GetLeafDescendants().Count;
            return $"Ancestro común (d:{dist}, {leafCount} descendientes)";
        }
    }
    
    /// <summary>
    /// Creates a compact tree visualization
    /// </summary>
    public string CreateCompactVisualization(Cladogram cladogram)
    {
        var output = new System.Text.StringBuilder();
        
        output.AppendLine("🌳 CLADOGRAMA COMPACTO");
        output.AppendLine(new string('─', 40));
        
        var leaves = cladogram.LeafNodes.OrderBy(n => n.Generation).ThenBy(n => n.Position).ToList();
        
        // Group by generation
        var byGeneration = leaves.GroupBy(n => n.Generation ?? 0).OrderBy(g => g.Key);
        
        foreach (var genGroup in byGeneration)
        {
            if (genGroup.Key == 0)
            {
                output.AppendLine("🟢 ORGANISMOS VIVOS:");
            }
            else
            {
                output.AppendLine($"🪨 FÓSILES GENERACIÓN {genGroup.Key}:");
            }
            
            foreach (var node in genGroup)
            {
                var symbol = node.IsFossil ? "🪨" : "🟢";
                var pos = node.Position?.ToString() ?? "?";
                var dist = node.Distance.ToString("F3");
                
                output.AppendLine($"  {symbol} Pos {pos} (dist: {dist})");
            }
            output.AppendLine();
        }
        
        return output.ToString();
    }
    
    /// <summary>
    /// Creates a distance matrix visualization
    /// </summary>
    public string CreateDistanceMatrix(Cladogram cladogram)
    {
        var output = new System.Text.StringBuilder();
        
        output.AppendLine("📊 MATRIZ DE DISTANCIAS GENÉTICAS");
        output.AppendLine(new string('─', 50));
        
        var leaves = cladogram.LeafNodes.ToList();
        
        if (leaves.Count <= 10) // Only show matrix for small trees
        {
            // Header
            output.Append("     ");
            foreach (var leaf in leaves)
            {
                var label = GetShortLabel(leaf);
                output.Append($"{label,8}");
            }
            output.AppendLine();
            
            // Rows
            for (int i = 0; i < leaves.Count; i++)
            {
                var label = GetShortLabel(leaves[i]);
                output.Append($"{label,4} ");
                
                for (int j = 0; j < leaves.Count; j++)
                {
                    if (i == j)
                    {
                        output.Append("   0.000");
                    }
                    else
                    {
                        var distance = cladogram.CalculateNodeDistance(leaves[i], leaves[j]);
                        output.Append($"{distance,8:F3}");
                    }
                }
                output.AppendLine();
            }
        }
        else
        {
            output.AppendLine("Matriz demasiado grande para mostrar (más de 10 hojas)");
            output.AppendLine($"Total de hojas: {leaves.Count}");
        }
        
        return output.ToString();
    }
    
    /// <summary>
    /// Gets a short label for a node
    /// </summary>
    private string GetShortLabel(CladogramNode node)
    {
        if (node.IsLeaf)
        {
            var type = node.IsFossil ? "F" : "L";
            var gen = node.Generation?.ToString() ?? "?";
            var pos = node.Position?.ToString() ?? "?";
            return $"{type}{gen}_{pos}";
        }
        else
        {
            return node.Id.Length > 7 ? node.Id.Substring(0, 7) : node.Id;
        }
    }
    
    /// <summary>
    /// Creates a phylogenetic timeline visualization
    /// </summary>
    public string CreateTimelineVisualization(Cladogram cladogram)
    {
        var output = new System.Text.StringBuilder();
        
        output.AppendLine("⏰ LÍNEA DE TIEMPO FILOGENÉTICA");
        output.AppendLine(new string('─', 50));
        
        var stats = cladogram.GetStatistics();
        
        if (stats.MinGeneration.HasValue && stats.MaxGeneration.HasValue)
        {
            var minGen = stats.MinGeneration.Value;
            var maxGen = stats.MaxGeneration.Value;
            
            // Create timeline
            for (int gen = minGen; gen <= maxGen; gen++)
            {
                var nodesInGen = cladogram.LeafNodes.Where(n => n.Generation == gen).ToList();
                
                if (nodesInGen.Any())
                {
                    output.Append($"Gen {gen,2}: ");
                    
                    foreach (var node in nodesInGen)
                    {
                        var symbol = node.IsFossil ? "🪨" : "🟢";
                        var pos = node.Position?.ToString() ?? "?";
                        output.Append($"{symbol}{pos} ");
                    }
                    output.AppendLine();
                }
            }
        }
        else
        {
            output.AppendLine("No hay información temporal disponible");
        }
        
        return output.ToString();
    }
    
    /// <summary>
    /// Creates a comprehensive visualization combining all methods
    /// </summary>
    public string CreateComprehensiveVisualization(Cladogram cladogram)
    {
        var output = new System.Text.StringBuilder();
        
        output.AppendLine(CreateAsciiVisualization(cladogram));
        output.AppendLine();
        output.AppendLine(CreateCompactVisualization(cladogram));
        output.AppendLine();
        output.AppendLine(CreateDistanceMatrix(cladogram));
        output.AppendLine();
        output.AppendLine(CreateTimelineVisualization(cladogram));
        
        return output.ToString();
    }
}
