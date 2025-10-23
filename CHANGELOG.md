# Changelog

## Versión 1.0.0 - Simulación de Deriva Genética

### Características Implementadas

#### 🧬 Modelos Genéticos
- **Gene**: Implementación completa de genes con secuencias de nucleótidos
- **Tipos de genes**: Seleccionados (afectados por selección natural) y Neutrales (solo deriva genética)
- **Fitness**: Cálculo basado en contenido GC y patrones repetitivos para genes seleccionados

#### 🦠 Organismos
- **Organism**: Organismos que contienen múltiples genes
- **Reproducción**: Generación de descendientes con mutaciones
- **Posicionamiento**: Organismos ubicados en un mundo 1D

#### 🌍 Mundo 1D
- **World1D**: Mundo unidimensional con capacidad de carga limitada
- **Migración**: Movimiento aleatorio de organismos
- **Densidad poblacional**: Efectos de densidad en la selección natural

#### 🧪 Mutaciones
- **Inserción**: Agregar nucleótidos nuevos
- **Duplicación**: Duplicar segmentos de ADN
- **Rotación**: Rotar segmentos de secuencia
- **Eliminación**: Remover nucleótidos

#### 🔬 Selección Natural
- **Presión selectiva**: Solo afecta genes seleccionados
- **Densidad dependiente**: Mayor presión en zonas más densas
- **Fitness dependiente**: Supervivencia basada en aptitud

#### 📊 Simulación y Análisis
- **SimulationEngine**: Motor completo de simulación
- **Estadísticas**: Análisis de diversidad genética
- **Deriva genética**: Comparación entre genes neutrales y seleccionados
- **Exportación**: Resultados en formato CSV

#### 📈 Métricas
- Diversidad genética por tipo de gen
- Correlación entre genes neutrales y seleccionados
- Evolución del fitness promedio
- Análisis de deriva genética

### Configuración
- Mundo de tamaño configurable
- Población inicial personalizable
- Tasa de mutación ajustable
- Fuerza de selección modificable
- Proporción de genes seleccionados vs neutrales

### Uso
```bash
dotnet run
```

### Archivos Principales
- `Program.cs`: Punto de entrada y configuración
- `Core/Models/`: Modelos de datos (Gene, Organism, World1D)
- `Core/Enums/`: Enumeraciones (MutationType, GeneType, Nucleotide)
- `Core/Interfaces/`: Interfaces principales
- `Services/SimulationEngine.cs`: Motor de simulación

### Resultados
- Log en consola con progreso de la simulación
- Archivo CSV con estadísticas detalladas
- Análisis automático de deriva genética
- Muestra de organismos finales
