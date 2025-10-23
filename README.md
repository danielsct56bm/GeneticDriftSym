# Simulación de Deriva Genética

Este proyecto implementa una simulación de deriva genética en un mundo 1D, donde los organismos tienen genes que pueden ser seleccionados o neutrales.

## Características

### Mundo 1D
- Mundo unidimensional donde los organismos pueden moverse y reproducirse
- Capacidad de carga limitada para simular competencia por recursos
- Migración aleatoria entre posiciones

### Genes
- **Genes Seleccionados**: Afectados por selección natural, su fitness influye en la supervivencia
- **Genes Neutrales**: No afectados por selección, evolucionan solo por deriva genética

### Tipos de Mutaciones
- **Inserción**: Agregar nucleótidos nuevos
- **Duplicación**: Duplicar segmentos de ADN
- **Rotación**: Rotar segmentos de secuencia
- **Eliminación**: Remover nucleótidos

### Selección Natural
- Solo afecta a genes seleccionados
- Basada en fitness calculado por contenido GC y patrones
- Presión selectiva dependiente de la densidad poblacional

## Uso

### Ejecutar la simulación básica:
```bash
dotnet run
```

### Configuración
Puedes modificar los parámetros en `Program.cs`:

```csharp
var config = new SimulationConfig
{
    WorldSize = 100,                    // Tamaño del mundo 1D
    InitialPopulationSize = 500,        // Población inicial
    CarryingCapacity = 1000,            // Capacidad de carga
    MutationRate = 0.01,                // Tasa de mutación
    SelectionStrength = 0.3,            // Fuerza de selección
    GeneCount = 10,                     // Número de genes por organismo
    GeneLength = 20,                    // Longitud de cada gen
    SelectedGeneRatio = 0.3,            // Proporción de genes seleccionados
    MaxGenerations = 100,               // Número de generaciones
    LogProgress = true,                 // Mostrar progreso
    LogInterval = 10                    // Frecuencia de logging
};
```

## Análisis de Resultados

La simulación genera:

1. **Log en consola**: Progreso de la simulación con estadísticas por generación
2. **Archivo CSV**: `simulation_results.csv` con datos detallados
3. **Análisis de deriva genética**: Comparación entre genes neutrales y seleccionados

### Métricas importantes:
- **Diversidad genética**: Mide cuántas secuencias únicas existen
- **Deriva genética**: Cambios en genes neutrales no atribuibles a selección
- **Correlación**: Relación entre diversidad de genes neutrales y seleccionados

## Estructura del Proyecto

```
├── Core/
│   ├── Enums/          # Enumeraciones (MutationType, GeneType, Nucleotide)
│   ├── Interfaces/     # Interfaces principales
│   └── Models/         # Modelos de datos
├── Services/           # Motor de simulación
├── Program.cs          # Punto de entrada
└── README.md          # Este archivo
```

## Conceptos Científicos

### Deriva Genética
- Cambios aleatorios en frecuencias alélicas en poblaciones pequeñas
- Más pronunciada en genes neutrales (no seleccionados)
- Independiente de la selección natural

### Selección Natural
- Presión selectiva que favorece ciertos genes
- En esta simulación, basada en contenido GC y patrones repetitivos
- Solo afecta genes marcados como "seleccionados"

### Mundo 1D
- Simplificación que permite estudiar migración y dispersión
- Útil para entender efectos de distancia y aislamiento
- Facilita el análisis de patrones espaciales

## Requisitos

- .NET 8.0 o superior
- Visual Studio 2022 o VS Code (recomendado)

## Ejecución

```bash
# Restaurar dependencias
dotnet restore

# Compilar
dotnet build

# Ejecutar
dotnet run
```

## Personalización

Puedes modificar fácilmente:
- Parámetros de mutación
- Criterios de fitness
- Reglas de selección
- Tamaño del mundo
- Características de los organismos

El código está diseñado para ser extensible y fácil de modificar para diferentes experimentos de evolución.
