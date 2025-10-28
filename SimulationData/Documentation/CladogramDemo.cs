using SimulationEvolucion.Core.Models;
using SimulationEvolucion.Services;

namespace SimulationEvolucion.SimulationData.Documentation;

/// <summary>
/// Programa de demostración del cladograma filogenético
/// </summary>
public class CladogramDemo
{
    public static void RunDemo()
    {
        Console.WriteLine("=== DEMOSTRACIÓN DEL CLADOGRAMA FILOGENÉTICO ===");
        Console.WriteLine();
        
        // Configuración optimizada para demostración
        var config = new SimulationConfig
        {
            WorldSize = 30,
            InitialPopulationSize = 3,          // Población pequeña para demo clara
            CarryingCapacity = 50,
            MutationRate = 0.08,                // Alta mutación para ver diferencias
            SelectionStrength = 0.3,
            GeneCount = 4,                      // Pocos genes para simplicidad
            GeneLength = 8,                    // Genes cortos
            SelectedGeneRatio = 0.5,
            MaxGenerations = 15,                // Pocas generaciones
            LogProgress = false,                // Sin logs para demo limpia
            LogInterval = 5,
            EnableFossilRecord = true,
            FossilizationProbability = 0.15,   // Alta fosilización
            FossilHalfLife = 10
        };
        
        Console.WriteLine("Configuración de demostración:");
        Console.WriteLine($"  Población inicial: {config.InitialPopulationSize}");
        Console.WriteLine($"  Generaciones: {config.MaxGenerations}");
        Console.WriteLine($"  Tasa de mutación: {config.MutationRate:P}");
        Console.WriteLine($"  Fosilización: {config.FossilizationProbability:P}");
        Console.WriteLine($"  Genes por organismo: {config.GeneCount}");
        Console.WriteLine();
        
        // Crear y ejecutar simulación
        var simulation = new SimulationEngine(config, seed: 456);
        simulation.Initialize();
        
        Console.WriteLine("Ejecutando simulación...");
        simulation.RunSimulation(config.MaxGenerations);
        
        // Estadísticas básicas
        var stats = simulation.GetCurrentStatistics();
        var fossilStats = simulation.GetFossilStatistics();
        
        Console.WriteLine($"\nResultados de la simulación:");
        Console.WriteLine($"  Organismos finales: {stats.TotalOrganisms}");
        Console.WriteLine($"  Fósiles creados: {fossilStats?.TotalFossils ?? 0}");
        Console.WriteLine($"  Fitness promedio: {stats.AverageFitness:F3}");
        
        // Demostrar cladograma
        Console.WriteLine("\n" + new string('=', 50));
        Console.WriteLine("CONSTRUCCIÓN DEL CLADOGRAMA");
        Console.WriteLine(new string('=', 50));
        
        try
        {
            // 1. Cladograma completo (vivos + fósiles)
            Console.WriteLine("\n1. CLADOGRAMA COMPLETO:");
            var fullCladogram = simulation.BuildCladogram();
            var fullStats = fullCladogram.GetStatistics();
            
            Console.WriteLine($"   Total nodos: {fullStats.TotalNodes}");
            Console.WriteLine($"   Hojas (vivos + fósiles): {fullStats.TotalLeaves}");
            Console.WriteLine($"   Profundidad: {fullStats.TreeDepth}");
            Console.WriteLine($"   Longitud total: {fullStats.TreeLength:F3}");
            
            // Mostrar estructura con visualización mejorada
            Console.WriteLine("\n   🌳 VISUALIZACIÓN DEL ÁRBOL:");
            var visualizer = new CladogramVisualizer();
            var asciiTree = visualizer.CreateAsciiVisualization(fullCladogram);
            var lines = asciiTree.Split('\n');
            foreach (var line in lines.Take(20)) // Mostrar primeras 20 líneas
            {
                Console.WriteLine($"   {line}");
            }
            if (lines.Length > 20)
            {
                Console.WriteLine($"   ... ({lines.Length - 20} líneas más)");
            }
            
            // 2. Cladograma solo de organismos vivos
            Console.WriteLine("\n2. CLADOGRAMA DE ORGANISMOS VIVOS:");
            var livingCladogram = simulation.BuildLivingOrganismsCladogram();
            var livingStats = livingCladogram.GetStatistics();
            
            Console.WriteLine($"   Organismos vivos: {livingStats.TotalLiving}");
            Console.WriteLine($"   Profundidad: {livingStats.TreeDepth}");
            
            if (livingStats.TotalLiving > 1)
            {
                Console.WriteLine("\n   Estructura:");
                var livingText = livingCladogram.ExportToText();
                var livingLines = livingText.Split('\n');
                foreach (var line in livingLines.Take(10))
                {
                    Console.WriteLine($"   {line}");
                }
            }
            
            // 3. Cladograma solo de fósiles
            Console.WriteLine("\n3. CLADOGRAMA DE FÓSILES:");
            var fossilCladogram = simulation.BuildFossilsCladogram();
            var fossilCladogramStats = fossilCladogram.GetStatistics();
            
            Console.WriteLine($"   Fósiles: {fossilCladogramStats.TotalFossils}");
            Console.WriteLine($"   Profundidad: {fossilCladogramStats.TreeDepth}");
            
            if (fossilCladogramStats.TotalFossils > 0)
            {
                Console.WriteLine("\n   Estructura:");
                var fossilText = fossilCladogram.ExportToText();
                var fossilLines = fossilText.Split('\n');
                foreach (var line in fossilLines.Take(10))
                {
                    Console.WriteLine($"   {line}");
                }
            }
            
            // 4. Exportar archivos
            Console.WriteLine("\n4. EXPORTANDO ARCHIVOS:");
            simulation.ExportCladogramToText("SimulationData/Cladograms/demo_cladograma_completo.txt");
            simulation.ExportCladogramToNewick("SimulationData/Cladograms/demo_cladograma_completo.newick");
            simulation.ExportLivingOrganismsCladogramToText("SimulationData/Cladograms/demo_cladograma_vivos.txt");
            simulation.ExportFossilsCladogramToText("SimulationData/Cladograms/demo_cladograma_fosiles.txt");
            simulation.ExportCladogramWithVisualization("SimulationData/Cladograms/demo_cladograma_visualizado.txt");
            
            Console.WriteLine("   ✓ SimulationData/Cladograms/demo_cladograma_completo.txt");
            Console.WriteLine("   ✓ SimulationData/Cladograms/demo_cladograma_completo.newick");
            Console.WriteLine("   ✓ SimulationData/Cladograms/demo_cladograma_vivos.txt");
            Console.WriteLine("   ✓ SimulationData/Cladograms/demo_cladograma_fosiles.txt");
            Console.WriteLine("   ✓ SimulationData/Cladograms/demo_cladograma_visualizado.txt (con ASCII art)");
            
            // 5. Análisis adicional
            Console.WriteLine("\n5. ANÁLISIS ADICIONAL:");
            
            if (fullStats.MinGeneration.HasValue && fullStats.MaxGeneration.HasValue)
            {
                Console.WriteLine($"   Rango temporal: Gen {fullStats.MinGeneration} - {fullStats.MaxGeneration}");
            }
            
            if (fullStats.MinPosition.HasValue && fullStats.MaxPosition.HasValue)
            {
                Console.WriteLine($"   Rango espacial: Pos {fullStats.MinPosition} - {fullStats.MaxPosition}");
            }
            
            // Mostrar algunos organismos de ejemplo
            Console.WriteLine("\n6. ORGANISMOS DE EJEMPLO:");
            var organisms = simulation.World.Organisms.Take(3).ToList();
            foreach (var org in organisms)
            {
                Console.WriteLine($"   {org}");
            }
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en la demostración: {ex.Message}");
            Console.WriteLine($"Detalles: {ex.StackTrace}");
        }
        
        Console.WriteLine("\n" + new string('=', 50));
        Console.WriteLine("DEMOSTRACIÓN COMPLETADA");
        Console.WriteLine(new string('=', 50));
    }
}
