using SimulationEvolucion.Core.Interfaces;

namespace SimulationEvolucion.Utils;

/// <summary>
/// Utility class for batch processing operations to reduce memory allocations
/// </summary>
public static class BatchProcessor
{
    /// <summary>
    /// Process organisms in batches to reduce memory allocations
    /// </summary>
    public static void ProcessOrganismsInBatches<T>(
        IEnumerable<IOrganism> organisms, 
        Func<IOrganism, T> processor, 
        int batchSize = 100)
    {
        var organismList = organisms.ToList();
        
        for (int i = 0; i < organismList.Count; i += batchSize)
        {
            var batch = organismList.Skip(i).Take(batchSize);
            
            foreach (var organism in batch)
            {
                processor(organism);
            }
        }
    }
    
    /// <summary>
    /// Process organisms in batches with action (no return value)
    /// </summary>
    public static void ProcessOrganismsInBatches(
        IEnumerable<IOrganism> organisms, 
        Action<IOrganism> action, 
        int batchSize = 100)
    {
        var organismList = organisms.ToList();
        
        for (int i = 0; i < organismList.Count; i += batchSize)
        {
            var batch = organismList.Skip(i).Take(batchSize);
            
            foreach (var organism in batch)
            {
                action(organism);
            }
        }
    }
    
    /// <summary>
    /// Process genes in batches to reduce memory allocations
    /// </summary>
    public static void ProcessGenesInBatches<T>(
        IEnumerable<IGene> genes, 
        Func<IGene, T> processor, 
        int batchSize = 50)
    {
        var geneList = genes.ToList();
        
        for (int i = 0; i < geneList.Count; i += batchSize)
        {
            var batch = geneList.Skip(i).Take(batchSize);
            
            foreach (var gene in batch)
            {
                processor(gene);
            }
        }
    }
    
    /// <summary>
    /// Process genes in batches with action (no return value)
    /// </summary>
    public static void ProcessGenesInBatches(
        IEnumerable<IGene> genes, 
        Action<IGene> action, 
        int batchSize = 50)
    {
        var geneList = genes.ToList();
        
        for (int i = 0; i < geneList.Count; i += batchSize)
        {
            var batch = geneList.Skip(i).Take(batchSize);
            
            foreach (var gene in batch)
            {
                action(gene);
            }
        }
    }
    
    /// <summary>
    /// Process any collection in batches
    /// </summary>
    public static void ProcessInBatches<T>(
        IEnumerable<T> items, 
        Action<T> action, 
        int batchSize = 100)
    {
        var itemList = items.ToList();
        
        for (int i = 0; i < itemList.Count; i += batchSize)
        {
            var batch = itemList.Skip(i).Take(batchSize);
            
            foreach (var item in batch)
            {
                action(item);
            }
        }
    }
    
    /// <summary>
    /// Process any collection in batches with return values
    /// </summary>
    public static IEnumerable<R> ProcessInBatches<T, R>(
        IEnumerable<T> items, 
        Func<T, R> processor, 
        int batchSize = 100)
    {
        var itemList = items.ToList();
        var results = new List<R>();
        
        for (int i = 0; i < itemList.Count; i += batchSize)
        {
            var batch = itemList.Skip(i).Take(batchSize);
            
            foreach (var item in batch)
            {
                results.Add(processor(item));
            }
        }
        
        return results;
    }
}
