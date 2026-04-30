using OrderApp.Data;
using OrderApp.Models;
using OrderApp.Services;

namespace OrderApp;

public class Program
{
    private static OrderService _service = null!;

    public static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        _service = new OrderService(
            SeedData.GetOrders(),
            SeedData.GetTypesByEnterprise(),
            SeedData.GetExpeditionValues());

        bool quitter = false;
        while (!quitter)
        {
            DisplayMenu();
            var choix = Console.ReadLine()?.Trim();
            Console.WriteLine();

            try
            {
                switch (choix)
                {
                    case "1": ListAll();               break;
                    case "2": SearchOrder();           break;
                    case "0":
                    case "q":
                    case "Q":
                        quitter = true;
                        break;
                    default:
                        Console.WriteLine("Choix invalide.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
            }

            if (!quitter)
            {
                Console.WriteLine();
                Console.Write("Appuyez sur Entrée pour continuer...");
                Console.ReadLine();
                Console.Clear();
            }
        }
    }

    // -----------------------------------------------------------------
    // Menu
    // -----------------------------------------————----------------------

    private static void DisplayMenu()
    {
        Console.WriteLine("│     GESTION DES COMMANDES — MENU      │");
        Console.WriteLine("|———————————————————————————————————————|");
        Console.WriteLine("│ 1. Lister toutes les commandes        │");
        Console.WriteLine("│ 2. Rechercher une commande            │");
        Console.WriteLine("│ 0. Quitter                            │");
        Console.Write("Votre choix : ");
    }

        // -----------------------------------------------------------------
    // Actions
    // -----------------------------------------------------------------

    private static void ListAll()
    {
        DisplayConsole.DisplayTitle("Toutes les commandes (triées par numéro)");
        var commandes = _service.AllOrders().OrderBy(c => c.Number);
        DisplayConsole.DisplayOrdersArray(commandes, _service);
    }

    private static void SearchOrder()
    {
        Console.WriteLine("Rechercher par :");
        Console.WriteLine("  1. Numéro de commande");
        Console.WriteLine("  2. Nom de client");
        Console.WriteLine("  3. Entreprise");
        Console.Write("Votre choix : ");
        var choix = Console.ReadLine()?.Trim();

        switch (choix)
        {
            case "1":
                Console.Write("Numéro : ");
                if (int.TryParse(Console.ReadLine(), out var numero))
                {
                    var c = _service.SearchByNumber(numero);
                    if (c is null)
                        Console.WriteLine($"Aucune commande n°{numero}.");
                    else
                        DisplayConsole.DisplayOrdersArray(new[] { c }, _service);
                }
                break;

            case "2":
                Console.Write("Nom ou prénom : ");
                var terme = Console.ReadLine() ?? "";
                DisplayConsole.DisplayOrdersArray(
                    _service.SearchByClient(terme), _service);
                break;

            case "3":
                Console.Write("Entreprise : ");
                var entr = Console.ReadLine() ?? "";
                DisplayConsole.DisplayOrdersArray(
                    _service.SearchByEnterprise(entr), _service);
                break;

            default:
                Console.WriteLine("Choix invalide.");
                break;
        }
    }
}
