namespace SimulationEvolucion.Models;

public enum OrganUseType
{
    BuildOrgan,    // Construir órganos
    Damage,        // Dañarse
    Absorb,        // Absorber otros órganos
    Heal,          // Curarse
    Synthesize,    // Sintetizar: convertir un carácter en otro
    Discard,       // Desechar caracteres
    Reproduce,     // Reproducir organismo
    Attack,        // Atacar a otros organismos
    Defend,
}

public class OrganUse
{
    public OrganUseType UseType { get; set; }
    
    public Dictionary<char, int> Costs { get; set; }
    
    public char? Source { get; set; }
    public char? Target { get; set; }
    
    public int EffectValue { get; set; }
    
    public OrganUse(OrganUseType useType, Dictionary<char, int> costs, char? sourceChar = null, char? targetChar = null, int effectValue = 0)
    {
        UseType = useType;
        Costs = costs;
        Source = sourceChar;
        Target = targetChar;
        EffectValue = effectValue;
    }
}