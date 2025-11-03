using SimulationEvolucion.Core.Models;
using SimulationEvolucion.Services;

namespace SimulationEvolucion.Services;

/// <summary>
/// Gestor de menús para la simulación de deriva genética
/// </summary>
public class MenuManager
{
    private SimulationConfig _currentConfig;
    private SimulationEngine? _currentSimulation;
    
    public MenuManager()
    {
        _currentConfig = GetDefaultConfig();
        _currentSimulation = null;
    }
    
    /// <summary>
    /// Muestra el menú principal y maneja la navegación
    /// </summary>
    public void ShowMainMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== SIMULACIÓN DE DERIVA GENÉTICA v3.00 ===");
            Console.WriteLine();
            Console.WriteLine("Menú Principal:");
            Console.WriteLine("1. Iniciar Simulación");
            Console.WriteLine("2. Cambiar Configuración");
            Console.WriteLine("3. Generar Cladograma");
            Console.WriteLine("4. Salir");
            Console.WriteLine();
            Console.Write("Seleccione una opción (1-4): ");
            
            var input = Console.ReadLine();
            
            switch (input)
            {
                case "1":
                    StartSimulation();
                    break;
                case "2":
                    ShowConfigurationMenu();
                    break;
                case "3":
                    GenerateCladogram();
                    break;
                case "4":
                    Console.WriteLine("¡Hasta luego!");
                    return;
                default:
                    Console.WriteLine("Opción inválida. Presione cualquier tecla para continuar...");
                    Console.ReadKey();
                    break;
            }
        }
    }
    
    /// <summary>
    /// Muestra el submenú de configuración
    /// </summary>
    private void ShowConfigurationMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== CONFIGURACIÓN DE SIMULACIÓN ===");
            Console.WriteLine();
            Console.WriteLine("Configuración actual:");
            DisplayCurrentConfig();
            Console.WriteLine();
            Console.WriteLine("Opciones de configuración:");
            Console.WriteLine("1. Configuración Completa");
            Console.WriteLine("2. Configuración Básica");
            Console.WriteLine("3. Configuración Predeterminada");
            Console.WriteLine("4. Volver al menú principal");
            Console.WriteLine();
            Console.Write("Seleccione una opción (1-4): ");
            
            var input = Console.ReadLine();
            
            switch (input)
            {
                case "1":
                    ConfigureComplete();
                    break;
                case "2":
                    ConfigureBasic();
                    break;
                case "3":
                    ConfigureDefault();
                    break;
                case "4":
                    return;
                default:
                    Console.WriteLine("Opción inválida. Presione cualquier tecla para continuar...");
                    Console.ReadKey();
                    break;
            }
        }
    }
    
    /// <summary>
    /// Configuración completa con todos los parámetros
    /// </summary>
    private void ConfigureComplete()
    {
        Console.Clear();
        Console.WriteLine("=== CONFIGURACIÓN COMPLETA ===");
        Console.WriteLine();
        
        _currentConfig.WorldSize = ReadInt("Tamaño del mundo", _currentConfig.WorldSize, 10, 1000);
        _currentConfig.InitialPopulationSize = ReadInt("Población inicial", _currentConfig.InitialPopulationSize, 1, 10000);
        _currentConfig.CarryingCapacity = ReadInt("Capacidad de carga", _currentConfig.CarryingCapacity, 100, 50000);
        _currentConfig.MutationRate = ReadDouble("Tasa de mutación", _currentConfig.MutationRate, 0.001, 0.1);
        _currentConfig.SelectionStrength = ReadDouble("Fuerza de selección", _currentConfig.SelectionStrength, 0.0, 1.0);
        _currentConfig.GeneCount = ReadInt("Número de genes", _currentConfig.GeneCount, 1, 50);
        _currentConfig.GeneLength = ReadInt("Longitud de genes", _currentConfig.GeneLength, 10, 100);
        _currentConfig.SelectedGeneRatio = ReadDouble("Proporción genes seleccionados", _currentConfig.SelectedGeneRatio, 0.0, 1.0);
        _currentConfig.MaxGenerations = ReadInt("Máximo de generaciones", _currentConfig.MaxGenerations, 10, 10000);
        _currentConfig.FossilizationProbability = ReadDouble("Probabilidad de fosilización", _currentConfig.FossilizationProbability, 0.001, 0.1);
        _currentConfig.FossilHalfLife = ReadInt("Vida media de fósiles", _currentConfig.FossilHalfLife, 10, 1000);
        
            Console.WriteLine();
            Console.WriteLine("Configuración actualizada exitosamente.");
            Console.WriteLine("Presione cualquier tecla para volver al menú de configuración...");
            Console.ReadKey();
    }
    
    /// <summary>
    /// Configuración básica con parámetros principales
    /// </summary>
    private void ConfigureBasic()
    {
        Console.Clear();
        Console.WriteLine("=== CONFIGURACIÓN BÁSICA ===");
        Console.WriteLine();
        
        _currentConfig.InitialPopulationSize = ReadInt("Población inicial", _currentConfig.InitialPopulationSize, 1, 10000);
        _currentConfig.MaxGenerations = ReadInt("Máximo de generaciones", _currentConfig.MaxGenerations, 10, 10000);
        _currentConfig.MutationRate = ReadDouble("Tasa de mutación", _currentConfig.MutationRate, 0.001, 0.1);
        _currentConfig.SelectionStrength = ReadDouble("Fuerza de selección", _currentConfig.SelectionStrength, 0.0, 1.0);
        
            Console.WriteLine();
            Console.WriteLine("Configuración básica actualizada exitosamente.");
            Console.WriteLine("Presione cualquier tecla para volver al menú de configuración...");
            Console.ReadKey();
    }
    
    /// <summary>
    /// Restaura la configuración predeterminada
    /// </summary>
    private void ConfigureDefault()
    {
        Console.Clear();
        Console.WriteLine("=== CONFIGURACIÓN PREDETERMINADA ===");
        Console.WriteLine();
        Console.WriteLine("¿Está seguro de que desea restaurar la configuración predeterminada? (S/N)");
        
        var response = Console.ReadLine()?.ToUpper();
        if (response == "S" || response == "SI" || response == "YES")
        {
            _currentConfig = GetDefaultConfig();
            Console.WriteLine();
            Console.WriteLine("Configuración restaurada a valores predeterminados.");
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("Operación cancelada.");
        }
        
        Console.WriteLine("Presione cualquier tecla para volver al menú de configuración...");
        Console.ReadKey();
    }
    
    /// <summary>
    /// Inicia una nueva simulación con la configuración actual
    /// </summary>
    private void StartSimulation()
    {
        Console.Clear();
        Console.WriteLine("=== INICIAR SIMULACIÓN ===");
        Console.WriteLine();
        
        Console.WriteLine("Configuración de la simulación:");
        DisplayCurrentConfig();
        Console.WriteLine();
        
        Console.WriteLine("¿Desea iniciar la simulación con esta configuración? (S/N)");
        var response = Console.ReadLine()?.ToUpper();
        
        if (response == "S" || response == "SI" || response == "YES")
        {
            try
            {
                Console.WriteLine();
                Console.WriteLine("=== INICIANDO SIMULACIÓN ===");
                Console.WriteLine();
                
                // Paso 1: Crear simulación
                Console.WriteLine("Paso 1/4: Creando motor de simulación...");
                _currentSimulation = new SimulationEngine(_currentConfig, seed: 42);
                Console.WriteLine("✓ Motor de simulación creado");
                
                // Paso 2: Inicializar
                Console.WriteLine("Paso 2/4: Inicializando población...");
                _currentSimulation.Initialize();
                Console.WriteLine("✓ Población inicializada");
                
                // Paso 3: Ejecutar simulación
                Console.WriteLine("Paso 3/4: Ejecutando simulación...");
                Console.WriteLine($"  Ejecutando {_currentConfig.MaxGenerations} generaciones...");
                _currentSimulation.RunSimulation(_currentConfig.MaxGenerations);
                Console.WriteLine("✓ Simulación ejecutada");
                
                // Paso 4: Analizar y exportar
                Console.WriteLine("Paso 4/4: Analizando resultados y exportando datos...");
                _currentSimulation.AnalyzeGeneticDrift();
                ExportSimulationResults();
                Console.WriteLine("✓ Análisis y exportación completados");
                
                Console.WriteLine();
                Console.WriteLine("=== SIMULACIÓN COMPLETADA EXITOSAMENTE ===");
                Console.WriteLine("Presione cualquier tecla para volver al menú principal...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"Error durante la simulación: {ex.Message}");
                Console.WriteLine("Presione cualquier tecla para volver al menú principal...");
                Console.ReadKey();
            }
        }
        else
        {
            Console.WriteLine();
            Console.WriteLine("Simulación cancelada.");
            Console.WriteLine("Presione cualquier tecla para volver al menú principal...");
            Console.ReadKey();
        }
    }
    
    /// <summary>
    /// Genera cladograma si hay datos de simulación disponibles
    /// </summary>
    private void GenerateCladogram()
    {
        Console.Clear();
        Console.WriteLine("=== GENERAR CLADOGRAMA ===");
        Console.WriteLine();
        
        if (_currentSimulation == null)
        {
            Console.WriteLine("No hay datos de simulación disponibles.");
            Console.WriteLine("Debe ejecutar una simulación primero.");
            Console.WriteLine();
            Console.WriteLine("Presione cualquier tecla para volver al menú principal...");
            Console.ReadKey();
            return;
        }
        
        try
        {
            Console.WriteLine("Generando cladograma filogenético...");
            Console.WriteLine();
            
            // Paso 1: Construir cladograma
            Console.WriteLine("Paso 1/5: Construyendo cladograma...");
            var cladogram = _currentSimulation.BuildCladogram();
            Console.WriteLine("✓ Cladograma construido exitosamente");
            
            // Paso 2: Obtener estadísticas
            Console.WriteLine("Paso 2/5: Calculando estadísticas...");
            var stats = cladogram.GetStatistics();
            Console.WriteLine("✓ Estadísticas calculadas");
            
            // Mostrar estadísticas resumidas
            Console.WriteLine();
            Console.WriteLine("Estadísticas del cladograma:");
            Console.WriteLine($"  Total nodos: {stats.TotalNodes}");
            Console.WriteLine($"  Total hojas: {stats.TotalLeaves}");
            Console.WriteLine($"    - Fósiles: {stats.TotalFossils}");
            Console.WriteLine($"    - Organismos vivos: {stats.TotalLiving}");
            Console.WriteLine($"  Profundidad del árbol: {stats.TreeDepth}");
            Console.WriteLine($"  Longitud total del árbol: {stats.TreeLength:F3}");
            
            if (stats.MinGeneration.HasValue && stats.MaxGeneration.HasValue)
            {
                Console.WriteLine($"  Rango de generaciones: {stats.MinGeneration} - {stats.MaxGeneration}");
            }
            
            Console.WriteLine();
            
            // Paso 3: Exportar cladograma completo (texto)
            Console.WriteLine("Paso 3/5: Exportando cladograma completo (formato texto)...");
            _currentSimulation.ExportCladogramToText("SimulationData/Cladograms/cladograma_completo.txt");
            Console.WriteLine("✓ Cladograma completo exportado");
            
            // Paso 4: Exportar cladograma completo (Newick)
            Console.WriteLine("Paso 4/5: Exportando cladograma completo (formato Newick)...");
            _currentSimulation.ExportCladogramToNewick("SimulationData/Cladograms/cladograma_completo.newick");
            Console.WriteLine("✓ Cladograma Newick exportado");
            
            // Paso 5: Exportar cladogramas específicos
            Console.WriteLine("Paso 5/5: Exportando cladogramas específicos...");
            _currentSimulation.ExportLivingOrganismsCladogramToText("SimulationData/Cladograms/cladograma_vivos.txt");
            Console.WriteLine("✓ Cladograma de organismos vivos exportado");
            
            _currentSimulation.ExportFossilsCladogramToText("SimulationData/Cladograms/cladograma_fosiles.txt");
            Console.WriteLine("✓ Cladograma de fósiles exportado");
            
            Console.WriteLine();
            Console.WriteLine("=== EXPORTACIÓN COMPLETADA ===");
            Console.WriteLine("Cladogramas exportados exitosamente:");
            Console.WriteLine("  - SimulationData/Cladograms/cladograma_completo.txt (formato texto)");
            Console.WriteLine("  - SimulationData/Cladograms/cladograma_completo.newick (formato Newick)");
            Console.WriteLine("  - SimulationData/Cladograms/cladograma_vivos.txt (solo organismos vivos)");
            Console.WriteLine("  - SimulationData/Cladograms/cladograma_fosiles.txt (solo fósiles)");
            
            // Instrucciones para visualizar en iTOL (Interactive Tree Of Life)
            Console.WriteLine();
            Console.WriteLine("Visualización recomendada (iTOL - web):");
            Console.WriteLine("  1) Abrir: https://itol.embl.de/upload.cgi");
            Console.WriteLine("  2) Subir el archivo: SimulationData/Cladograms/cladograma_completo.newick");
            Console.WriteLine("  3) Personalizar y exportar como imagen (PNG/SVG/PDF)");
            
            // Mostrar estadísticas de fósiles si están habilitados
            if (_currentConfig.EnableFossilRecord)
            {
                var fossilStats = _currentSimulation.GetFossilStatistics();
                if (fossilStats != null)
                {
                    Console.WriteLine();
                    Console.WriteLine("=== ESTADÍSTICAS DEL REGISTRO FÓSIL ===");
                    Console.WriteLine($"Total de fósiles: {fossilStats.TotalFossils}");
                    Console.WriteLine($"Promedio de genes por fósil: {fossilStats.AverageGenesPerFossil:F2}");
                    Console.WriteLine($"Longitud promedio preservada: {fossilStats.AveragePreservedLength:F2}");
                    Console.WriteLine($"Genes seleccionados: {fossilStats.SelectedGenesCount}");
                    Console.WriteLine($"Genes neutrales: {fossilStats.NeutralGenesCount}");
                    Console.WriteLine($"Genes vacíos: {fossilStats.EmptyGenesCount}");
                    Console.WriteLine($"Tasa de preservación: {fossilStats.PreservationRate:P2}");
                }
            }
            
            Console.WriteLine();
            Console.WriteLine("=== GENERACIÓN DE CLADOGRAMA COMPLETADA ===");
            Console.WriteLine("Presione cualquier tecla para volver al menú principal...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"Error al generar cladograma: {ex.Message}");
            Console.WriteLine("Presione cualquier tecla para volver al menú principal...");
            Console.ReadKey();
        }
    }
    
    /// <summary>
    /// Exporta los resultados de la simulación
    /// </summary>
    private void ExportSimulationResults()
    {
        if (_currentSimulation == null) return;
        
        // Exportar resultados
        var resultsPath = "SimulationData/Results/simulation_results.csv";
        _currentSimulation.ExportResults(resultsPath);
        Console.WriteLine($"Resultados exportados a: {resultsPath}");
        
        // Exportar registro fósil
        if (_currentConfig.EnableFossilRecord)
        {
            var fossilsPath = "SimulationData/Results/fosiles.json";
            _currentSimulation.ExportFossils(fossilsPath);
            Console.WriteLine($"Registro fósil exportado a: {fossilsPath}");
        }
    }
    
    /// <summary>
    /// Muestra la configuración actual
    /// </summary>
    private void DisplayCurrentConfig()
    {
        Console.WriteLine($"  Tamaño del mundo: {_currentConfig.WorldSize}");
        Console.WriteLine($"  Población inicial: {_currentConfig.InitialPopulationSize}");
        Console.WriteLine($"  Capacidad de carga: {_currentConfig.CarryingCapacity}");
        Console.WriteLine($"  Tasa de mutación: {_currentConfig.MutationRate}");
        Console.WriteLine($"  Fuerza de selección: {_currentConfig.SelectionStrength}");
        Console.WriteLine($"  Número de genes: {_currentConfig.GeneCount}");
        Console.WriteLine($"  Longitud de genes: {_currentConfig.GeneLength}");
        Console.WriteLine($"  Proporción genes seleccionados: {_currentConfig.SelectedGeneRatio:P}");
        Console.WriteLine($"  Generaciones: {_currentConfig.MaxGenerations}");
        Console.WriteLine($"  Registro fósil: {(_currentConfig.EnableFossilRecord ? "Habilitado" : "Deshabilitado")}");
        if (_currentConfig.EnableFossilRecord)
        {
            Console.WriteLine($"  Probabilidad de fosilización: {_currentConfig.FossilizationProbability:P}");
            Console.WriteLine($"  Vida media de fósiles: {_currentConfig.FossilHalfLife} generaciones");
        }
    }
    
    /// <summary>
    /// Lee un entero del usuario con validación
    /// </summary>
    private int ReadInt(string prompt, int currentValue, int min, int max)
    {
        while (true)
        {
            Console.Write($"{prompt} [{currentValue}] (min: {min}, max: {max}): ");
            var input = Console.ReadLine();
            
            if (string.IsNullOrEmpty(input))
                return currentValue;
            
            if (int.TryParse(input, out int value) && value >= min && value <= max)
                return value;
            
            Console.WriteLine($"Valor inválido. Debe ser un número entre {min} y {max}.");
        }
    }
    
    /// <summary>
    /// Lee un double del usuario con validación
    /// </summary>
    private double ReadDouble(string prompt, double currentValue, double min, double max)
    {
        while (true)
        {
            Console.Write($"{prompt} [{currentValue:F3}] (min: {min:F3}, max: {max:F3}): ");
            var input = Console.ReadLine();
            
            if (string.IsNullOrEmpty(input))
                return currentValue;
            
            if (double.TryParse(input, out double value) && value >= min && value <= max)
                return value;
            
            Console.WriteLine($"Valor inválido. Debe ser un número entre {min:F3} y {max:F3}.");
        }
    }
    
    /// <summary>
    /// Obtiene la configuración predeterminada
    /// </summary>
    private SimulationConfig GetDefaultConfig()
    {
        return new SimulationConfig
        {
            WorldSize = 100,
            InitialPopulationSize = 1,
            CarryingCapacity = 1000,
            MutationRate = 0.01,
            SelectionStrength = 0.3,
            GeneCount = 10,
            GeneLength = 20,
            SelectedGeneRatio = 0.3,
            MaxGenerations = 50,
            LogProgress = true,
            LogInterval = 10,
            EnableFossilRecord = true,
            FossilizationProbability = 0.01,
            FossilHalfLife = 50
        };
    }
}
