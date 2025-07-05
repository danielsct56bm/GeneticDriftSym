namespace SimulationEvolucion.Models;

public class Gene
{
    public char Symbol { get; private set; }
    
    private static readonly char[] PossiblesSymbols = { 'A', 'B', 'C', 'D' };
    private static Random _rng = new Random();

    public Gene(char symbol)
    {
        if (!PossiblesSymbols.Contains(symbol))
        {
            throw new ArgumentException("Invalid symbol: " + symbol);
        }
        
        Symbol = symbol;
    }

    public static Gene GenerateRandom()
    {
        char symbol = PossiblesSymbols[_rng.Next(0, PossiblesSymbols.Length)];
        return new Gene(symbol);
    }
    
    public void Mutate()
    {
        char newSymbol;
        do
        {
            newSymbol = PossiblesSymbols[_rng.Next(0, PossiblesSymbols.Length)];
        } while (newSymbol == Symbol);
        
        Symbol = newSymbol;
    }

    public override string ToString()
    {
        return Symbol.ToString();
    }
}