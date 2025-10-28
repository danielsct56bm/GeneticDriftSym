using SimulationEvolucion.Core.Interfaces;

namespace SimulationEvolucion.Core.Models;

/// <summary>
/// Builder for constructing phylogenetic trees (cladograms) from organisms and fossils
/// </summary>
public class CladogramBuilder
{
    private readonly Random _random;
    
    public CladogramBuilder(int? seed = null)
    {
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }
    
    /// <summary>
    /// Builds a cladogram from living organisms and fossils using a simple hierarchical clustering
    /// </summary>
    public Cladogram BuildCladogram(List<IOrganism> livingOrganisms, List<LazyFossil> fossils, int currentGeneration)
    {
        var allNodes = new List<CladogramNode>();
        
        // Create nodes for living organisms
        foreach (var organism in livingOrganisms)
        {
            var node = CladogramNode.CreateLivingOrganismNode(organism, currentGeneration);
            allNodes.Add(node);
        }
        
        // Create nodes for fossils
        foreach (var fossil in fossils)
        {
            var node = CladogramNode.CreateFossilNode(fossil);
            allNodes.Add(node);
        }
        
        if (allNodes.Count == 0)
        {
            throw new InvalidOperationException("No organisms or fossils provided for cladogram construction");
        }
        
        if (allNodes.Count == 1)
        {
            return new Cladogram(allNodes[0]);
        }
        
        // Use simple hierarchical clustering instead of UPGMA for better reliability
        return BuildHierarchicalTree(allNodes);
    }
    
    /// <summary>
    /// Builds a simple hierarchical tree using distance-based clustering
    /// </summary>
    private Cladogram BuildHierarchicalTree(List<CladogramNode> initialNodes)
    {
        var nextNodeId = 1;
        var clusters = initialNodes.Select(node => new List<CladogramNode> { node }).ToList();
        
        while (clusters.Count > 1)
        {
            // Find the two closest clusters
            double minDistance = double.MaxValue;
            int bestI = 0, bestJ = 1;
            
            for (int i = 0; i < clusters.Count; i++)
            {
                for (int j = i + 1; j < clusters.Count; j++)
                {
                    var distance = CalculateClusterDistance(clusters[i], clusters[j]);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        bestI = i;
                        bestJ = j;
                    }
                }
            }
            
            // Ensure distance is reasonable
            if (minDistance >= double.MaxValue)
            {
                minDistance = 0.1;
            }
            
            // Create internal node
            var internalNode = CladogramNode.CreateInternalNode($"Internal_{nextNodeId++}");
            
            // Merge clusters
            var mergedCluster = new List<CladogramNode>();
            mergedCluster.AddRange(clusters[bestI]);
            mergedCluster.AddRange(clusters[bestJ]);
            
            // Add children to internal node
            foreach (var node in mergedCluster)
            {
                internalNode.AddChild(node);
                node.Distance = Math.Min(minDistance / 2.0, 1.0);
            }
            
            // Update clusters list
            var newClusters = new List<List<CladogramNode>>();
            for (int k = 0; k < clusters.Count; k++)
            {
                if (k != bestI && k != bestJ)
                {
                    newClusters.Add(clusters[k]);
                }
            }
            newClusters.Add(new List<CladogramNode> { internalNode });
            
            clusters = newClusters;
        }
        
        return new Cladogram(clusters[0][0]);
    }
    
    /// <summary>
    /// Calculates distance between two clusters using average linkage
    /// </summary>
    private double CalculateClusterDistance(List<CladogramNode> cluster1, List<CladogramNode> cluster2)
    {
        double totalDistance = 0;
        int comparisons = 0;
        
        foreach (var node1 in cluster1)
        {
            foreach (var node2 in cluster2)
            {
                var distance = node1.CalculateGeneticDistance(node2);
                if (distance < double.MaxValue)
                {
                    totalDistance += distance;
                    comparisons++;
                }
            }
        }
        
        return comparisons > 0 ? totalDistance / comparisons : 1.0;
    }
    
    /// <summary>
    /// Builds a cladogram using only living organisms
    /// </summary>
    public Cladogram BuildLivingOrganismsCladogram(List<IOrganism> livingOrganisms, int currentGeneration)
    {
        return BuildCladogram(livingOrganisms, new List<LazyFossil>(), currentGeneration);
    }
    
    /// <summary>
    /// Builds a cladogram using only fossils
    /// </summary>
    public Cladogram BuildFossilsCladogram(List<LazyFossil> fossils)
    {
        return BuildCladogram(new List<IOrganism>(), fossils, 0);
    }
    
    /// <summary>
    /// Implements UPGMA (Unweighted Pair Group Method with Arithmetic Mean) algorithm
    /// </summary>
    private Cladogram BuildUPGMATree(List<CladogramNode> initialNodes)
    {
        try
        {
            var clusters = new List<Cluster>();
            var nextNodeId = 1;
            
            // Initialize clusters with single nodes
            foreach (var node in initialNodes)
            {
                clusters.Add(new Cluster(node));
            }
            
            // Build distance matrix
            var distanceMatrix = BuildDistanceMatrix(clusters);
            
            // UPGMA iterations
            while (clusters.Count > 1)
            {
                // Find the pair of clusters with minimum distance
                var (i, j, minDistance) = FindMinimumDistance(distanceMatrix);
                
                // Create new internal node
                var internalNode = CladogramNode.CreateInternalNode($"Internal_{nextNodeId++}");
                
                // Calculate distances to children
                var clusterI = clusters[i];
                var clusterJ = clusters[j];
                
                internalNode.AddChild(clusterI.Representative);
                internalNode.AddChild(clusterJ.Representative);
                
                // Set distances
                clusterI.Representative.Distance = minDistance / 2.0;
                clusterJ.Representative.Distance = minDistance / 2.0;
                
                // Create new cluster
                var newCluster = new Cluster(internalNode);
                
                // Update clusters list
                var newClusters = new List<Cluster>();
                for (int k = 0; k < clusters.Count; k++)
                {
                    if (k != i && k != j)
                    {
                        newClusters.Add(clusters[k]);
                    }
                }
                newClusters.Add(newCluster);
                
                // Update distance matrix
                distanceMatrix = UpdateDistanceMatrix(distanceMatrix, clusters, i, j, newCluster);
                clusters = newClusters;
            }
            
            // The last cluster contains the root
            return new Cladogram(clusters[0].Representative);
        }
        catch (Exception ex)
        {
            // Fallback to simple binary tree if UPGMA fails
            Console.WriteLine($"UPGMA algorithm failed: {ex.Message}");
            Console.WriteLine("Falling back to simple binary tree construction...");
            return BuildSimpleBinaryTree(initialNodes);
        }
    }
    
    /// <summary>
    /// Fallback method: builds a simple binary tree when UPGMA fails
    /// </summary>
    private Cladogram BuildSimpleBinaryTree(List<CladogramNode> nodes)
    {
        if (nodes.Count == 1)
        {
            return new Cladogram(nodes[0]);
        }
        
        var nextNodeId = 1;
        var workingNodes = new List<CladogramNode>(nodes);
        
        while (workingNodes.Count > 1)
        {
            // Find the two most similar nodes
            double minDistance = double.MaxValue;
            int bestI = 0, bestJ = 1;
            
            for (int i = 0; i < workingNodes.Count; i++)
            {
                for (int j = i + 1; j < workingNodes.Count; j++)
                {
                    var distance = workingNodes[i].CalculateGeneticDistance(workingNodes[j]);
                    // Ensure distance is reasonable
                    if (distance < minDistance && distance < double.MaxValue)
                    {
                        minDistance = distance;
                        bestI = i;
                        bestJ = j;
                    }
                }
            }
            
            // If no valid distance found, use a small default distance
            if (minDistance >= double.MaxValue)
            {
                minDistance = 0.1; // Small default distance
            }
            
            // Create internal node
            var internalNode = CladogramNode.CreateInternalNode($"Internal_{nextNodeId++}");
            
            // Add children
            internalNode.AddChild(workingNodes[bestI]);
            internalNode.AddChild(workingNodes[bestJ]);
            
            // Set distances (ensure they're reasonable)
            workingNodes[bestI].Distance = Math.Min(minDistance / 2.0, 1.0);
            workingNodes[bestJ].Distance = Math.Min(minDistance / 2.0, 1.0);
            
            // Update working list
            var newWorkingNodes = new List<CladogramNode>();
            for (int k = 0; k < workingNodes.Count; k++)
            {
                if (k != bestI && k != bestJ)
                {
                    newWorkingNodes.Add(workingNodes[k]);
                }
            }
            newWorkingNodes.Add(internalNode);
            
            workingNodes = newWorkingNodes;
        }
        
        return new Cladogram(workingNodes[0]);
    }
    
    /// <summary>
    /// Builds initial distance matrix between all clusters
    /// </summary>
    private double[,] BuildDistanceMatrix(List<Cluster> clusters)
    {
        int n = clusters.Count;
        var matrix = new double[n, n];
        
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i == j)
                {
                    matrix[i, j] = 0;
                }
                else
                {
                    matrix[i, j] = clusters[i].Representative.CalculateGeneticDistance(clusters[j].Representative);
                }
            }
        }
        
        return matrix;
    }
    
    /// <summary>
    /// Finds the minimum distance in the matrix and returns indices and value
    /// </summary>
    private (int i, int j, double distance) FindMinimumDistance(double[,] matrix)
    {
        int n = matrix.GetLength(0);
        double minDistance = double.MaxValue;
        int minI = 0, minJ = 0;
        
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                if (matrix[i, j] < minDistance)
                {
                    minDistance = matrix[i, j];
                    minI = i;
                    minJ = j;
                }
            }
        }
        
        return (minI, minJ, minDistance);
    }
    
    /// <summary>
    /// Updates distance matrix after merging two clusters
    /// </summary>
    private double[,] UpdateDistanceMatrix(double[,] oldMatrix, List<Cluster> oldClusters, int i, int j, Cluster newCluster)
    {
        int oldN = oldClusters.Count;
        int newN = oldN - 1;
        var newMatrix = new double[newN, newN];
        
        // Create mapping from old indices to new indices
        var oldToNewIndex = new Dictionary<int, int>();
        int newIndex = 0;
        
        for (int oldIndex = 0; oldIndex < oldN; oldIndex++)
        {
            if (oldIndex != i && oldIndex != j)
            {
                oldToNewIndex[oldIndex] = newIndex;
                newIndex++;
            }
        }
        
        // Copy existing distances (excluding rows/columns i and j)
        foreach (var kvp1 in oldToNewIndex)
        {
            foreach (var kvp2 in oldToNewIndex)
            {
                newMatrix[kvp1.Value, kvp2.Value] = oldMatrix[kvp1.Key, kvp2.Key];
            }
        }
        
        // Calculate distances to new cluster using UPGMA formula
        int newClusterIndex = newN - 1;
        var clusterI = oldClusters[i];
        var clusterJ = oldClusters[j];
        int sizeI = clusterI.Size;
        int sizeJ = clusterJ.Size;
        
        for (int k = 0; k < newN - 1; k++)
        {
            // Find the old index corresponding to this new index
            int oldK = oldToNewIndex.First(x => x.Value == k).Key;
            
            // UPGMA: d(new, k) = (d(i,k) * |i| + d(j,k) * |j|) / (|i| + |j|)
            double distanceIK = oldMatrix[i, oldK];
            double distanceJK = oldMatrix[j, oldK];
            
            double newDistance = (distanceIK * sizeI + distanceJK * sizeJ) / (sizeI + sizeJ);
            
            newMatrix[newClusterIndex, k] = newDistance;
            newMatrix[k, newClusterIndex] = newDistance;
        }
        
        return newMatrix;
    }
    
    /// <summary>
    /// Helper class for UPGMA algorithm
    /// </summary>
    private class Cluster
    {
        public CladogramNode Representative { get; set; }
        public int Size { get; set; }
        
        public Cluster(CladogramNode representative)
        {
            Representative = representative;
            Size = representative.IsLeaf ? 1 : representative.GetLeafDescendants().Count;
        }
    }
    
    /// <summary>
    /// Builds a simple neighbor-joining tree (alternative algorithm)
    /// </summary>
    public Cladogram BuildNeighborJoiningTree(List<IOrganism> livingOrganisms, List<LazyFossil> fossils, int currentGeneration)
    {
        var allNodes = new List<CladogramNode>();
        
        // Create nodes for living organisms
        foreach (var organism in livingOrganisms)
        {
            var node = CladogramNode.CreateLivingOrganismNode(organism, currentGeneration);
            allNodes.Add(node);
        }
        
        // Create nodes for fossils
        foreach (var fossil in fossils)
        {
            var node = CladogramNode.CreateFossilNode(fossil);
            allNodes.Add(node);
        }
        
        if (allNodes.Count == 0)
        {
            throw new InvalidOperationException("No organisms or fossils provided for cladogram construction");
        }
        
        if (allNodes.Count == 1)
        {
            return new Cladogram(allNodes[0]);
        }
        
        // For simplicity, fall back to UPGMA for now
        // TODO: Implement full neighbor-joining algorithm
        return BuildUPGMATree(allNodes);
    }
}
