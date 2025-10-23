namespace SimulationEvolucion.Core.Enums;

/// <summary>
/// Tipos de mutaciones genéticas que pueden ocurrir
/// </summary>
public enum MutationType
{
    /// <summary>Inserciones de nuevas bases</summary>
    Insertion,
    /// <summary>Duplicación de secuencias</summary>
    Duplication,
    /// <summary>Rotación de secuencias</summary>
    Rotation,
    /// <summary>Eliminación de bases</summary>
    Deletion
}
