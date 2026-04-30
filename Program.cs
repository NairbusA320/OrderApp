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
        Console.WriteLine("│ 0. Quitter                            │");
        Console.Write("Votre choix : ");
    }
}
