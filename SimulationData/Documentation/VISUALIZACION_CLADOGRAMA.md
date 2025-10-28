# Visualización del Cladograma Filogenético

## Tipos de Visualización Disponibles

### 1. 🌳 Visualización ASCII Art

```
🌳 CLADOGRAMA FILOGENÉTICO - VISUALIZACIÓN ASCII
════════════════════════════════════════════════════════════
📊 Estadísticas: 8 hojas (3 fósiles, 5 vivos)
📏 Profundidad: 4 | Longitud total: 2.456

📍 Ancestro común (d:0.000, 8 descendientes)
├─ 🪨 Fósil Gen17 Pos77 (d:0.123)
├─ 📍 Ancestro común (d:0.000, 5 descendientes)
│  ├─ 🟢 Vivo Gen20 Pos30 (d:0.234)
│  ├─ 🟢 Vivo Gen20 Pos45 (d:0.156)
│  └─ 📍 Ancestro común (d:0.000, 3 descendientes)
│     ├─ 🟢 Vivo Gen20 Pos12 (d:0.089)
│     ├─ 🟢 Vivo Gen20 Pos67 (d:0.134)
│     └─ 🟢 Vivo Gen20 Pos89 (d:0.098)
└─ 🪨 Fósil Gen15 Pos23 (d:0.201)

🔍 Leyenda:
  🟢 = Organismo vivo
  🪨 = Fósil
  📍 = Nodo interno (ancestro común)
```

### 2. 📋 Visualización Compacta

```
🌳 CLADOGRAMA COMPACTO
────────────────────────────────────────
🟢 ORGANISMOS VIVOS:
  🟢 Pos 12 (dist: 0.089)
  🟢 Pos 30 (dist: 0.234)
  🟢 Pos 45 (dist: 0.156)
  🟢 Pos 67 (dist: 0.134)
  🟢 Pos 89 (dist: 0.098)

🪨 FÓSILES GENERACIÓN 15:
  🪨 Pos 23 (dist: 0.201)

🪨 FÓSILES GENERACIÓN 17:
  🪨 Pos 77 (dist: 0.123)
```

### 3. 📊 Matriz de Distancias Genéticas

```
📊 MATRIZ DE DISTANCIAS GENÉTICAS
──────────────────────────────────────────────────
     L20_12  L20_30  L20_45  L20_67  L20_89  F15_23  F17_77
L20_12   0.000  0.234  0.156  0.134  0.098  0.201  0.123
L20_30   0.234  0.000  0.089  0.156  0.134  0.234  0.201
L20_45   0.156  0.089  0.000  0.098  0.156  0.156  0.134
L20_67   0.134  0.156  0.098  0.000  0.089  0.134  0.156
L20_89   0.098  0.134  0.156  0.089  0.000  0.098  0.123
F15_23   0.201  0.234  0.156  0.134  0.098  0.000  0.201
F17_77   0.123  0.201  0.134  0.156  0.123  0.201  0.000
```

### 4. ⏰ Línea de Tiempo Filogenética

```
⏰ LÍNEA DE TIEMPO FILOGENÉTICA
──────────────────────────────────────────────────
Gen 15: 🪨23 
Gen 17: 🪨77 
Gen 20: 🟢12 🟢30 🟢45 🟢67 🟢89 
```

### 5. 📄 Formato Newick (Para análisis filogenético)

```
((F_15_23:0.201,L_20_12:0.089):0.000,((L_20_30:0.234,L_20_45:0.156):0.000,((L_20_67:0.134,L_20_89:0.098):0.000,F_17_77:0.123):0.000):0.000);
```

## Cómo Usar las Visualizaciones

### Desde el Programa Principal

```csharp
// Usar visualización mejorada en lugar de la básica
simulation.AnalyzeCladogramWithVisualization();

// Exportar con visualización completa
simulation.ExportCladogramWithVisualization("cladograma_visualizado.txt");
```

### Desde Código

```csharp
var cladogram = simulation.BuildCladogram();
var visualizer = new CladogramVisualizer();

// Diferentes tipos de visualización
var asciiTree = visualizer.CreateAsciiVisualization(cladogram);
var compactView = visualizer.CreateCompactVisualization(cladogram);
var distanceMatrix = visualizer.CreateDistanceMatrix(cladogram);
var timeline = visualizer.CreateTimelineVisualization(cladogram);
var comprehensive = visualizer.CreateComprehensiveVisualization(cladogram);

Console.WriteLine(asciiTree);
```

### Demostración Independiente

```csharp
// Ejecutar demostración completa de visualización
CladogramVisualizationDemo.RunVisualizationDemo();
```

## Archivos de Salida

- `visualizacion_completa.txt` - Todas las visualizaciones combinadas
- `visualizacion_ascii.txt` - Solo ASCII art
- `visualizacion_compacta.txt` - Solo vista compacta
- `visualizacion_matriz.txt` - Solo matriz de distancias
- `visualizacion_timeline.txt` - Solo línea de tiempo
- `cladograma_completo.newick` - Formato Newick estándar

## Interpretación de los Símbolos

- 🟢 **Organismo vivo**: Organismo de la generación actual
- 🪨 **Fósil**: Organismo fosilizado de generaciones anteriores
- 📍 **Nodo interno**: Ancestro común hipotético
- **Distancia (d:)**: Distancia genética calculada
- **Gen X**: Generación en la que vivió el organismo
- **Pos Y**: Posición espacial en el mundo

## Ventajas de Cada Tipo

1. **ASCII Art**: Fácil de entender la estructura jerárquica
2. **Compacta**: Vista rápida de todos los organismos
3. **Matriz**: Análisis detallado de distancias genéticas
4. **Timeline**: Evolución temporal de la población
5. **Newick**: Compatible con software filogenético estándar

## Configuración Recomendada para Visualización Clara

```csharp
var config = new SimulationConfig
{
    InitialPopulationSize = 3-6,     // Población pequeña para árbol claro
    MutationRate = 0.08-0.12,         // Alta mutación para diferencias visibles
    FossilizationProbability = 0.15-0.25, // Alta fosilización
    MaxGenerations = 15-25,           // Suficientes generaciones
    GeneCount = 3-5,                  // Pocos genes para simplicidad
    EnableFossilRecord = true
};
```
