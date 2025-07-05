namespace SimulationEvolucion.Utils;

public class GeneTranslator
{
    public static Dictionary<string, char> GeneToChar = new Dictionary<string, char>()
    {
        { "AAA", 'A' }, { "AAB", 'B' }, { "AAC", 'C' }, { "AAD", 'D' },
        { "ABA", 'E' }, { "ABB", 'F' }, { "ABC", 'G' }, { "ABD", 'H' },
        { "ACA", 'I' }, { "ACB", 'J' }, { "ACC", 'K' }, { "ACD", 'L' },
        { "ADA", 'M' }, { "ADB", 'N' }, { "ADC", 'O' }, { "ADD", 'P' },

        { "BAA", 'Q' }, { "BAB", 'R' }, { "BAC", 'S' }, { "BAD", 'T' },
        { "BBA", 'U' }, { "BBB", 'V' }, { "BBC", 'W' }, { "BBD", 'X' },
        { "BCA", 'Y' }, { "BCB", 'Z' }, { "BCC", '<' }, { "BCD", '>' },
        { "BDA", '(' }, { "BDB", ')' }, { "BDC", '[' }, { "BDD", ']' },

        { "CAA", '.' }, { "CAB", ',' }, { "CAC", '1' }, { "CAD", '2' },
        { "CBA", '3' }, { "CBB", '4' }, { "CBC", '5' }, { "CBD", '6' },
        { "CCA", '7' }, { "CCB", '8' }, { "CCC", '9' }, { "CCD", '0' },
        { "CDA", ':' }, { "CDB", ';' }, { "CDC", '/' }, { "CDD", '*' },

        { "DAA", '~' }, { "DAB", '!' }, { "DAC", '@' }, { "DAD", '#' },
        { "DBA", '$' }, { "DBB", '%' }, { "DBC", '^' }, { "DBD", '&' },
        { "DCA", '_' }, { "DCB", '+' }, { "DCC", '"' }, { "DCD", '`' },
        { "DDA", '?' }, { "DDB", '|' }, { "DDC", '{' }, { "DDD", '}' }
    };

    public static bool IsValidGene(string triplet)
    {
        return GeneToChar.ContainsKey(triplet);
    }

    public static char Translate(string triplet)
    {
        return GeneToChar.ContainsKey(triplet) ? GeneToChar[triplet] : '?';
    }
}