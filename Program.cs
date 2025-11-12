using SimulationEvolucion.Services;

namespace SimulationEvolucion;

class Program
{
    static void Main(string[] args)
    {
        // Crear y mostrar menú principal
        var menuManager = new MenuManager();
        menuManager.ShowMainMenu();
    }
    
}
