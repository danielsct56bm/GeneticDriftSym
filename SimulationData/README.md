# 📁 SimulationData - Estructura de Archivos

Esta carpeta contiene todos los archivos generados por la simulación de evolución.

## 📂 Estructura de Carpetas

### 📊 Results/
Contiene los resultados principales de la simulación:
- `simulation_results.csv` - Estadísticas de cada generación (población, fitness, diversidad genética)
- `fosiles.json` - Registro completo de fósiles con secuencias genéticas

### 🌳 Cladograms/
Contiene todos los cladogramas generados:
- `cladograma_completo.txt` - Cladograma completo (organismos vivos + fósiles) en formato texto
- `cladograma_completo.newick` - Cladograma completo en formato Newick estándar
- `cladograma_vivos.txt` - Cladograma solo de organismos vivos de la última generación
- `cladograma_fosiles.txt` - Cladograma solo de fósiles
- `demo_cladograma_*.txt` - Archivos de demostración del sistema de cladogramas

### 📚 Documentation/
Contiene documentación técnica del sistema:
- `CLADOGRAMA_README.md` - Documentación completa del sistema de cladogramas
- `VISUALIZACION_CLADOGRAMA.md` - Guía de visualización de cladogramas
- `CladogramDemo.cs` - Programa de demostración del cladograma
- `CladogramVisualizationDemo.cs` - Programa de demostración de visualización

### 📝 Logs/
Reservado para archivos de log futuros (si se implementan).

## 🔄 Archivos Generados Automáticamente

Todos los archivos se generan automáticamente cuando ejecutas la simulación:

1. **Resultados de simulación** → `Results/`
2. **Cladogramas** → `Cladograms/`
3. **Registro fósil** → `Results/`

## 📋 Formato de Archivos

### CSV (simulation_results.csv)
```csv
Generation,TotalOrganisms,AverageFitness,FitnessVariance,SelectedGenes,NeutralGenes,NeutralDiversity,SelectedDiversity
0,100,0.500000,0.250000,5,5,0.800000,0.600000
...
```

### JSON (fosiles.json)
```json
{
  "fossils": [
    {
      "position": 77,
      "generation": 17,
      "organismId": "Organism_1156",
      "sequences": ["ATCGATCGATCGATCGATCG", ...]
    }
  ],
  "metadata": {
    "totalFossils": 172,
    "totalGenes": 1720,
    "fossilizationProbability": 0.01,
    "fossilHalfLife": 50
  }
}
```

### Newick (cladograma_completo.newick)
```
((Organism_1001:0.1,Organism_1002:0.1):0.2,(Fossil_1156:0.3,Organism_1003:0.1):0.2);
```

## 🎯 Uso Recomendado

1. **Análisis de datos**: Usa `Results/simulation_results.csv` para análisis estadísticos
2. **Visualización**: Usa `Cladograms/cladograma_completo.txt` para ver la estructura del árbol
3. **Herramientas externas**: Usa `Cladograms/cladograma_completo.newick` con software como FigTree, MEGA, etc.
4. **Comparación**: Compara `cladograma_vivos.txt` vs `cladograma_fosiles.txt` para ver diferencias evolutivas

## 🧹 Mantenimiento

- Los archivos se sobrescriben en cada ejecución
- Para preservar resultados, copia los archivos antes de una nueva simulación
- La carpeta se crea automáticamente si no existe
