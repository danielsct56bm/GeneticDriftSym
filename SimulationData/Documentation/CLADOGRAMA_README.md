# Cladograma Filogenético - Simulación Evolutiva

## Descripción

Se ha implementado un sistema completo de cladograma filogenético que permite visualizar las relaciones evolutivas entre organismos vivos y fósiles en la simulación de deriva genética.

## Características Implementadas

### 1. Modelos de Datos

- **CladogramNode**: Representa un nodo en el árbol filogenético
  - Nodos hoja para organismos vivos y fósiles
  - Nodos internos para ancestros comunes
  - Cálculo de distancias genéticas usando distancia de Hamming
  - Propiedades de visualización (profundidad, posición)

- **Cladogram**: El árbol filogenético completo
  - Estadísticas del árbol (profundidad, longitud total)
  - Búsqueda de ancestros comunes más recientes (MRCA)
  - Exportación en formato texto y Newick
  - Análisis de rangos temporales y espaciales

- **CladogramBuilder**: Constructor del cladograma
  - Algoritmo UPGMA (Unweighted Pair Group Method with Arithmetic Mean)
  - Construcción de árboles para organismos vivos, fósiles, o ambos
  - Manejo de secuencias genéticas dañadas en fósiles

### 2. Integración con SimulationEngine

- **Métodos principales**:
  - `BuildCladogram()`: Cladograma completo (vivos + fósiles)
  - `BuildLivingOrganismsCladogram()`: Solo organismos vivos
  - `BuildFossilsCladogram()`: Solo fósiles
  - `AnalyzeCladogram()`: Análisis y visualización
  - `ExportCladogramToText()`: Exportar en formato texto
  - `ExportCladogramToNewick()`: Exportar en formato Newick

### 3. Algoritmo de Construcción

El cladograma se construye usando el algoritmo UPGMA:

1. **Inicialización**: Cada organismo/fósil es un cluster individual
2. **Matriz de distancias**: Se calcula la distancia genética entre todos los pares
3. **Iteración UPGMA**:
   - Se encuentra el par con menor distancia genética
   - Se crea un nodo interno que conecta los dos clusters
   - Se actualiza la matriz de distancias
   - Se repite hasta tener un solo cluster (raíz)

### 4. Cálculo de Distancias Genéticas

- **Distancia de Hamming**: Compara secuencias nucleotídicas
- **Manejo de fósiles**: Ignora posiciones dañadas (*) en fósiles
- **Normalización**: Distancia promedio por gen válido
- **Robustez**: Maneja secuencias de diferentes longitudes

### 5. Formatos de Exportación

#### Formato Texto
```
=== CLADOGRAMA FILOGENÉTICO ===
Total nodos: 15
Total hojas: 8 (3 fósiles, 5 vivos)
Profundidad del árbol: 4
Longitud total del árbol: 2.456

Nodo interno Internal_1 (Dist: 0.000)
  Fossil Organism_123 (Gen 15, Pos 25) (Dist: 0.123)
  Nodo interno Internal_2 (Dist: 0.000)
    Living Organism_456 (Gen 20, Pos 30) (Dist: 0.234)
    ...
```

#### Formato Newick
```
((F_15_25:0.123,L_20_30:0.234):0.000,F_12_18:0.156):0.000;
```

### 6. Estadísticas del Cladograma

- **Total nodos**: Número total de nodos en el árbol
- **Total hojas**: Organismos vivos + fósiles
- **Profundidad**: Nivel máximo del árbol
- **Longitud total**: Suma de todas las distancias de ramas
- **Rango temporal**: Generaciones mínima y máxima
- **Rango espacial**: Posiciones mínima y máxima

## Uso

### Desde el Programa Principal

1. Ejecutar la simulación normal
2. Al final, se pregunta si desea ver la demostración del cladograma
3. Seleccionar "Y" para ejecutar `CladogramDemo.RunDemo()`

### Desde Código

```csharp
// Crear simulación
var simulation = new SimulationEngine(config);
simulation.Initialize();
simulation.RunSimulation(50);

// Construir cladograma
var cladogram = simulation.BuildCladogram();

// Analizar
simulation.AnalyzeCladogram();

// Exportar
simulation.ExportCladogramToText("cladograma.txt");
simulation.ExportCladogramToNewick("cladograma.newick");
```

### Demostración Independiente

```csharp
// Ejecutar demostración completa
CladogramDemo.RunDemo();
```

## Archivos Generados

- `cladograma_completo.txt`: Cladograma completo en formato texto
- `cladograma_completo.newick`: Cladograma completo en formato Newick
- `cladograma_vivos.txt`: Solo organismos vivos
- `cladograma_fosiles.txt`: Solo fósiles
- `demo_cladograma_*.txt`: Archivos de demostración

## Configuración Recomendada

Para obtener cladogramas interesantes:

```csharp
var config = new SimulationConfig
{
    InitialPopulationSize = 3-10,     // Población pequeña-mediana
    MutationRate = 0.05-0.1,          // Alta mutación para diversidad
    FossilizationProbability = 0.1-0.2, // Alta fosilización
    MaxGenerations = 20-50,           // Suficientes generaciones
    EnableFossilRecord = true         // Habilitar fósiles
};
```

## Limitaciones Actuales

1. **Algoritmo**: Solo UPGMA implementado (Neighbor-Joining pendiente)
2. **Escalabilidad**: Para poblaciones muy grandes (>1000) puede ser lento
3. **Visualización**: Solo formato texto, sin gráficos visuales
4. **Fósiles**: No considera el tiempo de formación en la construcción del árbol

## Futuras Mejoras

1. Implementar algoritmo Neighbor-Joining
2. Agregar visualización gráfica del árbol
3. Considerar tiempo de formación de fósiles
4. Análisis de bootstrap para confianza estadística
5. Exportación a formatos estándar (NEXUS, PhyloXML)

## Archivos del Sistema

- `Core/Models/CladogramNode.cs`: Nodo del árbol filogenético
- `Core/Models/Cladogram.cs`: Árbol filogenético completo
- `Core/Models/CladogramBuilder.cs`: Constructor del cladograma
- `Services/SimulationEngine.cs`: Integración con el motor de simulación
- `CladogramDemo.cs`: Demostración independiente
- `Program.cs`: Integración en el programa principal
