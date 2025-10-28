using SimulationEvolucion.Core.Models;
using SimulationEvolucion.Services;

namespace SimulationEvolucion.SimulationData.Documentation;

/// <summary>
/// Programa de demostración de la visualización del cladograma
/// </summary>
public class CladogramVisualizationDemo
{
    public static void RunVisualizationDemo()
    {
        Console.WriteLine("=== DEMOSTRACIÓN DE VISUALIZACIÓN DEL CLADOGRAMA ===");
        Console.WriteLine();
        
        // Configuración optimizada para mostrar visualización clara
        var config = new SimulationConfig
        {
            WorldSize = 20,
            InitialPopulationSize = 4,          // Población pequeña para árbol claro
            CarryingCapacity = 30,
            MutationRate = 0.1,                 // Alta mutación para diferencias visibles
            SelectionStrength = 0.2,
            GeneCount = 3,                      // Pocos genes para simplicidad
            GeneLength = 6,                     // Genes cortos
            SelectedGeneRatio = 0.5,
            MaxGenerations = 12,                // Pocas generaciones
            LogProgress = false,
            LogInterval = 5,
            EnableFossilRecord = true,
            FossilizationProbability = 0.2,    // Alta fosilización
            FossilHalfLife = 8
        };
        
        Console.WriteLine("Configuración para visualización clara:");
        Console.WriteLine($"  Población inicial: {config.InitialPopulationSize}");
        Console.WriteLine($"  Generaciones: {config.MaxGenerations}");
        Console.WriteLine($"  Tasa de mutación: {config.MutationRate:P}");
        Console.WriteLine($"  Fosilización: {config.FossilizationProbability:P}");
        Console.WriteLine();
        
        // Crear y ejecutar simulación
        var simulation = new SimulationEngine(config, seed: 789);
        simulation.Initialize();
        
        Console.WriteLine("Ejecutando simulación...");
        simulation.RunSimulation(config.MaxGenerations);
        
        // Estadísticas básicas
        var stats = simulation.GetCurrentStatistics();
        var fossilStats = simulation.GetFossilStatistics();
        
        Console.WriteLine($"\nResultados:");
        Console.WriteLine($"  Organismos finales: {stats.TotalOrganisms}");
        Console.WriteLine($"  Fósiles creados: {fossilStats?.TotalFossils ?? 0}");
        
        // Demostrar diferentes tipos de visualización
        Console.WriteLine("\n" + new string('═', 60));
        Console.WriteLine("TIPOS DE VISUALIZACIÓN DEL CLADOGRAMA");
        Console.WriteLine(new string('═', 60));
        
        try
        {
            var cladogram = simulation.BuildCladogram();
            var visualizer = new CladogramVisualizer();
            
            // 1. Visualización ASCII Art
            Console.WriteLine("\n1️⃣ VISUALIZACIÓN ASCII ART:");
            Console.WriteLine(new string('─', 40));
            var asciiViz = visualizer.CreateAsciiVisualization(cladogram);
            Console.WriteLine(asciiViz);
            
            // 2. Visualización Compacta
            Console.WriteLine("\n2️⃣ VISUALIZACIÓN COMPACTA:");
            Console.WriteLine(new string('─', 40));
            var compactViz = visualizer.CreateCompactVisualization(cladogram);
            Console.WriteLine(compactViz);
            
            // 3. Matriz de Distancias
            Console.WriteLine("\n3️⃣ MATRIZ DE DISTANCIAS:");
            Console.WriteLine(new string('─', 40));
            var matrixViz = visualizer.CreateDistanceMatrix(cladogram);
            Console.WriteLine(matrixViz);
            
            // 4. Línea de Tiempo
            Console.WriteLine("\n4️⃣ LÍNEA DE TIEMPO FILOGENÉTICA:");
            Console.WriteLine(new string('─', 40));
            var timelineViz = visualizer.CreateTimelineVisualization(cladogram);
            Console.WriteLine(timelineViz);
            
            // 5. Visualización Completa
            Console.WriteLine("\n5️⃣ VISUALIZACIÓN COMPLETA:");
            Console.WriteLine(new string('─', 40));
            var fullViz = visualizer.CreateComprehensiveVisualization(cladogram);
            
            // Mostrar solo las primeras líneas para no saturar la pantalla
            var fullLines = fullViz.Split('\n');
            foreach (var line in fullLines.Take(30))
            {
                Console.WriteLine(line);
            }
            if (fullLines.Length > 30)
            {
                Console.WriteLine($"... ({fullLines.Length - 30} líneas más)");
            }
            
            // Exportar todas las visualizaciones
            Console.WriteLine("\n📁 EXPORTANDO VISUALIZACIONES:");
            simulation.ExportCladogramWithVisualization("visualizacion_completa.txt");
            
            // Exportar visualizaciones individuales
            File.WriteAllText("visualizacion_ascii.txt", asciiViz);
            File.WriteAllText("visualizacion_compacta.txt", compactViz);
            File.WriteAllText("visualizacion_matriz.txt", matrixViz);
            File.WriteAllText("visualizacion_timeline.txt", timelineViz);
            
            Console.WriteLine("   ✓ visualizacion_completa.txt");
            Console.WriteLine("   ✓ visualizacion_ascii.txt");
            Console.WriteLine("   ✓ visualizacion_compacta.txt");
            Console.WriteLine("   ✓ visualizacion_matriz.txt");
            Console.WriteLine("   ✓ visualizacion_timeline.txt");
            
            // Mostrar algunos organismos de ejemplo
            Console.WriteLine("\n🔬 ORGANISMOS DE EJEMPLO:");
            var organisms = simulation.World.Organisms.Take(3).ToList();
            foreach (var org in organisms)
            {
                Console.WriteLine($"   {org}");
                
                // Mostrar secuencias genéticas
                var selectedGenes = org.GetGenesByType(SimulationEvolucion.Core.Enums.GeneType.Selected).Take(2);
                Console.WriteLine("     Genes seleccionados:");
                foreach (var gene in selectedGenes)
                {
                    var sequence = string.Join("", gene.Sequence.Select(n => n.ToString()));
                    Console.WriteLine($"       {sequence}");
                }
            }
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en la demostración: {ex.Message}");
        }
        
        Console.WriteLine("\n" + new string('═', 60));
        Console.WriteLine("DEMOSTRACIÓN DE VISUALIZACIÓN COMPLETADA");
        Console.WriteLine(new string('═', 60));
    }
}
