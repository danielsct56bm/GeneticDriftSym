namespace SimulationEvolucion.Utils;

public static class IdGenerator
{
    private static int _counter = 0;

    public static string GetNextId()
    {
        return $"Org-{_counter++}";
    }

}