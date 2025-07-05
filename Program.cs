using SimulationEvolucion.Services;

class Program
{
    static void Main(string[] args)
    {
        var engine = new SimulationEngine(worldSize: 20);
        engine.SeedInitialOrganisms(organismCount: 5 , geneCount: 9);

        while (true)
        {
            Console.WriteLine("--- Simulación de evolución ---");
            Console.WriteLine("1. Ejecutar un paso de simulación");
            Console.WriteLine("2. Mostrar estado actual");
            Console.WriteLine("3. Reiniciar mundo");
            Console.WriteLine("4. Salir");
            Console.Write("Elige una opción: ");

            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    Console.WriteLine("Cuantos pasos?");
                    var steps = int.Parse(Console.ReadLine());
                    if (steps > 0)
                    {
                        engine.Run(steps);
                    }
                    else
                    {
                        Console.WriteLine("Entrada no valida, ingresa un numero mayor que 0");
                    }

                    continue;
                case "2":
                    engine.ShowWorldDetailed();
                    continue;
                case "3":
                    engine = new SimulationEngine(worldSize: 20);
                    engine.SeedInitialOrganisms(organismCount: 5, geneCount: 15);
                    Console.WriteLine("Mundo reiniciado.");
                    continue;
                case "4":
                    Console.WriteLine("Simulación terminada");
                    break;
                default:
                    Console.WriteLine("Opción invalida, intentar de nuevo");
                    continue;
            }
        }
    }
}