using SimulationEvolucion.Core.Models;
using SimulationEvolucion.Services;
using SimulationEvolucion.SimulationData.Documentation;

namespace SimulationEvolucion;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== SIMULACIÓN DE DERIVA GENÉTICA ===");
        Console.WriteLine();
        
        // Configuración de la simulación
        var config = new SimulationConfig
        {
            WorldSize = 100,
            InitialPopulationSize = 1,          // 1 organismo inicial (población fundadora)
            CarryingCapacity = 1000,            // Capacidad de carga alta para crecimiento
            MutationRate = 0.01,
            SelectionStrength = 0.3,
            GeneCount = 10,
            GeneLength = 20,
            SelectedGeneRatio = 0.3, // 30% genes seleccionados, 70% neutrales
            MaxGenerations = 50,
            LogProgress = true,
            LogInterval = 10,
            EnableFossilRecord = true,          // Habilitar registro fósil para cladograma
            FossilizationProbability = 0.01,   // 1% probabilidad de fosilización
            FossilHalfLife = 50                 // Vida media de fósiles
        };
        
        // Crear y ejecutar simulación
        var simulation = new SimulationEngine(config, seed: 42);
        
        Console.WriteLine("Configuración de la simulación:");
        Console.WriteLine($"  Tamaño del mundo: {config.WorldSize}");
        Console.WriteLine($"  Población inicial: {config.InitialPopulationSize}");
        Console.WriteLine($"  Capacidad de carga: {config.CarryingCapacity}");
        Console.WriteLine($"  Tasa de mutación: {config.MutationRate}");
        Console.WriteLine($"  Fuerza de selección: {config.SelectionStrength}");
        Console.WriteLine($"  Número de genes: {config.GeneCount}");
        Console.WriteLine($"  Longitud de genes: {config.GeneLength}");
        Console.WriteLine($"  Proporción genes seleccionados: {config.SelectedGeneRatio:P}");
        Console.WriteLine($"  Generaciones: {config.MaxGenerations}");
        Console.WriteLine($"  Registro fósil: {(config.EnableFossilRecord ? "Habilitado" : "Deshabilitado")}");
        if (config.EnableFossilRecord)
        {
            Console.WriteLine($"  Probabilidad de fosilización: {config.FossilizationProbability:P}");
            Console.WriteLine($"  Vida media de fósiles: {config.FossilHalfLife} generaciones");
        }
        Console.WriteLine();
        
        // Inicializar simulación
        simulation.Initialize();
        
        // Ejecutar simulación
        Console.WriteLine("Iniciando simulación...");
        simulation.RunSimulation(config.MaxGenerations);
        
        // Analizar resultados
        simulation.AnalyzeGeneticDrift();
        
        // Generar y analizar cladograma
        Console.WriteLine("\n" + new string('=', 60));
        Console.WriteLine("GENERANDO CLADOGRAMA FILOGENÉTICO");
        Console.WriteLine(new string('=', 60));
        
        try
        {
            // Analizar cladograma completo
            simulation.AnalyzeCladogram();
            
            // Exportar cladogramas
            simulation.ExportCladogramToText("SimulationData/Cladograms/cladograma_completo.txt");
            simulation.ExportCladogramToNewick("SimulationData/Cladograms/cladograma_completo.newick");
            
            // Exportar cladograma solo de organismos vivos
            simulation.ExportLivingOrganismsCladogramToText("SimulationData/Cladograms/cladograma_vivos.txt");
            
            // Exportar cladograma solo de fósiles
            simulation.ExportFossilsCladogramToText("SimulationData/Cladograms/cladograma_fosiles.txt");
            
            Console.WriteLine("\nCladogramas exportados exitosamente:");
            Console.WriteLine("  - SimulationData/Cladograms/cladograma_completo.txt (formato texto)");
            Console.WriteLine("  - SimulationData/Cladograms/cladograma_completo.newick (formato Newick)");
            Console.WriteLine("  - SimulationData/Cladograms/cladograma_vivos.txt (solo organismos vivos)");
            Console.WriteLine("  - SimulationData/Cladograms/cladograma_fosiles.txt (solo fósiles)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al generar cladograma: {ex.Message}");
        }
        
        // Exportar resultados
        var resultsPath = "SimulationData/Results/simulation_results.csv";
        simulation.ExportResults(resultsPath);
        Console.WriteLine($"\nResultados exportados a: {resultsPath}");
        
        // Exportar registro fósil
        if (config.EnableFossilRecord)
        {
            var fossilsPath = "SimulationData/Results/fosiles.json";
            simulation.ExportFossils(fossilsPath);
            
            // Mostrar estadísticas de fósiles
            var fossilStats = simulation.GetFossilStatistics();
            if (fossilStats != null)
            {
                Console.WriteLine("\n=== ESTADÍSTICAS DEL REGISTRO FÓSIL ===");
                Console.WriteLine($"Total de fósiles: {fossilStats.TotalFossils}");
                Console.WriteLine($"Promedio de genes por fósil: {fossilStats.AverageGenesPerFossil:F2}");
                Console.WriteLine($"Longitud promedio preservada: {fossilStats.AveragePreservedLength:F2}");
                Console.WriteLine($"Genes seleccionados: {fossilStats.SelectedGenesCount}");
                Console.WriteLine($"Genes neutrales: {fossilStats.NeutralGenesCount}");
                Console.WriteLine($"Genes vacíos: {fossilStats.EmptyGenesCount}");
                Console.WriteLine($"Tasa de preservación: {fossilStats.PreservationRate:P2}");
            }
        }
        
        // Mostrar algunos organismos finales como ejemplo
        ShowSampleOrganisms(simulation);
        
        // Opción de continuar simulación
        while (true)
        {
            Console.WriteLine("\n¿Deseas simular 50 pasos más? (Y/N)");
            var response = Console.ReadLine()?.ToUpper();
            
            if (response == "Y" || response == "YES")
            {
                Console.WriteLine("\nContinuando simulación...");
                simulation.RunSimulation(50);
                
                // Analizar resultados actualizados
                simulation.AnalyzeGeneticDrift();
                
                // Exportar resultados actualizados
                simulation.ExportResults(resultsPath);
                Console.WriteLine($"\nResultados actualizados exportados a: {resultsPath}");
                
                // Exportar registro fósil actualizado
                if (config.EnableFossilRecord)
                {
                    simulation.ExportFossils("fosiles.json");
                }
                
                // Mostrar algunos organismos finales como ejemplo
                ShowSampleOrganisms(simulation);
            }
            else if (response == "N" || response == "NO")
            {
                break;
            }
            else
            {
                Console.WriteLine("Por favor, responde Y para sí o N para no.");
            }
        }
        
        Console.WriteLine("\nSimulación completada. Presiona cualquier tecla para salir...");
        Console.ReadKey();
        
        // Opción de demostrar cladograma
        Console.WriteLine("\n¿Deseas ver una demostración del cladograma? (Y/N)");
        var cladogramResponse = Console.ReadLine()?.ToUpper();
        
        if (cladogramResponse == "Y" || cladogramResponse == "YES")
        {
            Console.WriteLine("\nEjecutando demostración del cladograma...");
            CladogramDemo.RunDemo();
        }
        
        Console.WriteLine("\nPresiona cualquier tecla para salir...");
        Console.ReadKey();
    }
    
    static void ShowSampleOrganisms(SimulationEngine simulation)
    {
        Console.WriteLine("\n=== ORGANISMOS DE MUESTRA (Generación Final) ===");
        
        var organisms = simulation.World.Organisms.Take(5).ToList();
        
        foreach (var organism in organisms)
        {
            Console.WriteLine($"\n{organism}");
            
            // Mostrar algunos genes como ejemplo
            var selectedGenes = organism.GetGenesByType(SimulationEvolucion.Core.Enums.GeneType.Selected).Take(2);
            var neutralGenes = organism.GetGenesByType(SimulationEvolucion.Core.Enums.GeneType.Neutral).Take(2);
            
            Console.WriteLine("  Genes seleccionados (ejemplo):");
            foreach (var gene in selectedGenes)
            {
                Console.WriteLine($"    {gene}");
            }
            
            Console.WriteLine("  Genes neutrales (ejemplo):");
            foreach (var gene in neutralGenes)
            {
                Console.WriteLine($"    {gene}");
            }
        }
    }
}
