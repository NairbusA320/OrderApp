using OrderApp.Models;
using OrderApp.Services;

namespace OrderApp.Services;

/// <summary>
/// Mise en forme des sorties console.
/// </summary>
public static class DisplayConsole
{
    public static void DisplayTitle(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"=== {title} ===");
    }

    private static string Truncate(string s, int length) =>
        s.Length <= length ? s : s.Substring(0, length - 1) + "…";

    public static void DisplayOrdersArray(IEnumerable<Order> orders, OrderService service)
    {
        var list = orders.ToList();
        if (list.Count == 0)
        {
            Console.WriteLine("Aucune commande à afficher.");
            return;
        }

        Console.WriteLine();
        Console.WriteLine($"{"N°",-5} {"Client",-25} {"Entreprise",-26} {"Montant",7} {"Payé",-6} {"Envoyé",-7} {"Type exp.",-12} {"Statut",-12}");
        Console.WriteLine(new string('-', 110));

        foreach (var c in list)
        {
            var type = service.GetExpeditionType(c);
            Console.WriteLine(
                $"{c.Number,-5} " +
                $"{Truncate(c.FullName, 25),-25} " +
                $"{Truncate(c.Enterprise, 26),-26} " +
                $"{c.Amount,6} € " +
                $"{(c.Paid ? "oui" : "non"),-6} " +
                $"{(c.Sent ? "oui" : "non"),-7} " +
                $"{type,-12} " +
                $"{c.Status,-12}");
        }

        Console.WriteLine(new string('-', 110));
        Console.WriteLine($"Total : {list.Count} commande(s) — {list.Sum(c => c.Amount):N0} € (Dont {list.Where(c => c.Paid).Sum(c => c.Amount):N0} € payées)");
    }
}
