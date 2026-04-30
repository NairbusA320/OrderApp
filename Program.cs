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
                    case "3": FilterByStatus();        break;
                    case "4": DisplayOrdersAtRisk();   break;
                    case "5": DisplayDashboard();      break;
                    case "6": DisplayExpeditionList(); break;
                    case "7": DisplayStats();          break;
                    case "8": DisplayTopClients();     break;
                    case "9": DisplayRecoveryPlan();   break;
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
        Console.WriteLine("│ 3. Filtrer par statut (payé / envoyé) |");
        Console.WriteLine("│ 4. Commandes à risque                 │");
        Console.WriteLine("│ 5. Tableau de bord (4 cadrans)        │");
        Console.WriteLine("│ 6. File d'expédition (à envoyer)      │");
        Console.WriteLine("│ 7. Statistiques financières           │");
        Console.WriteLine("│ 8. Top clients & entreprises          │");
        Console.WriteLine("│ 9. Plan de relances priorisé          │");
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

    private static void FilterByStatus()
    {
        Console.WriteLine("  1. Payées");
        Console.WriteLine("  2. Non payées");
        Console.WriteLine("  3. Envoyées");
        Console.WriteLine("  4. Non envoyées");
        Console.Write("Votre choix : ");
        var choix = Console.ReadLine()?.Trim();

        IEnumerable<Order> resultats = choix switch
        {
            "1" => _service.PaidOrders(),
            "2" => _service.UnpaidOrders(),
            "3" => _service.SentOrders(),
            "4" => _service.UnsentOrders(),
            _   => Enumerable.Empty<Order>()
        };

        DisplayConsole.DisplayOrdersArray(
            resultats.OrderBy(c => c.Number), _service);
    }

    private static void DisplayOrdersAtRisk()
    {
        DisplayConsole.DisplayTitle("Commandes à risque (envoyées sans paiement)");
        var risque = _service.OrdersAtRisk().OrderByDescending(c => c.Amount);
        DisplayConsole.DisplayOrdersArray(risque, _service);
        Console.WriteLine($"Montant total à recouvrer en urgence : {_service.TotalAmountAtRisk():N0} €");
    }

    private static void DisplayDashboard()
    {
        DisplayConsole.DisplayTitle("Tableau de bord");

        var toutes = _service.AllOrders();
        var aTraiter = toutes.Count(c => !c.Paid && !c.Sent);
        var aExpedier = toutes.Count(c => c.Paid && !c.Sent);
        var aRisque = toutes.Count(c => !c.Paid && c.Sent);
        var cloturees = toutes.Count(c => c.Paid && c.Sent);

        Console.WriteLine();
        Console.WriteLine("                  Non envoyée        Envoyée");
        Console.WriteLine($"  Non payée   │   À traiter : {aTraiter,3} │  À RISQUE: {aRisque,3}  │");
        Console.WriteLine($"  Payée       │   À expédier: {aExpedier,3} │  Clôturée: {cloturees,3}  │");
        Console.WriteLine();
        Console.WriteLine($"  Total : {toutes.Count} commandes");
        Console.WriteLine($"  CA encaissé          : {_service.TotalIncome(),10:N0} €");
        Console.WriteLine($"  Encours à recouvrer  : {_service.PendingToRecover(),10:N0} €");
        Console.WriteLine($"  Montant à risque     : {_service.TotalAmountAtRisk(),10:N0} €");
    }

    private static void DisplayExpeditionList()
    {
        DisplayConsole.DisplayTitle("File d'expédition (payées, non envoyées, par priorité)");
        var file = _service.ExpeditionLineByPriority();
        DisplayConsole.DisplayOrdersArray(file, _service);

        var coutTotal = file.Sum(c => _service.GetExpeditionCost(c));
        Console.WriteLine($"Coût d'expédition prévisionnel : {coutTotal:N0} €");
    }

    private static void DisplayStats()
    {
        DisplayConsole.DisplayTitle("Statistiques financières");
        Console.WriteLine($"  CA encaissé              : {_service.TotalIncome(),10:N0} €");
        Console.WriteLine($"  Encours à recouvrer      : {_service.PendingToRecover(),10:N0} €");
        Console.WriteLine($"  Coût des expéditions     : {_service.PendingExpeditionsCost(),10:N0} €");
        Console.WriteLine($"  Montant à risque         : {_service.TotalAmountAtRisk(),10:N0} €");

        Console.WriteLine();
        Console.WriteLine("Répartition par type d'expédition :");
        Console.WriteLine($"  {"Type",-12} {"Nb cmd",8} {"Coût total",12}");
        foreach (var (type, nb, cout) in _service.TypeDistribution())
        {
            Console.WriteLine($"  {type,-12} {nb,8} {cout,10:N0} €");
        }
    }

    private static void DisplayTopClients()
    {
        DisplayConsole.DisplayTitle("Top 5 clients (par montant cumulé)");
        Console.WriteLine($"  {"Client",-30} {"Nb cmd",8} {"Total",12}");
        foreach (var (client, nb, total) in _service.TopClients(5))
        {
            Console.WriteLine($"  {client,-30} {nb,8} {total,10:N0} €");
        }

        DisplayConsole.DisplayTitle("Top 5 entreprises (par nombre de commandes)");
        Console.WriteLine($"  {"Entreprise",-30} {"Nb cmd",8} {"Total",12}");
        foreach (var (entr, nb, total) in _service.TopEnterprises(5))
        {
            Console.WriteLine($"  {entr,-30} {nb,8} {total,10:N0} €");
        }
    }

    private static void DisplayRecoveryPlan()
    {
        DisplayConsole.DisplayTitle("Plan de relances — trié par priorité décroissante");
        var plan = _service.GenerateRecoveryPlan().ToList();

        // Regroupement par niveau pour un affichage hiérarchisé
        foreach (var group in plan.GroupBy(p => p.Level).OrderBy(g => g.Key))
        {
            Console.WriteLine();
            Console.WriteLine($"—— {group.Key.ToString().ToUpper()} —— ({group.Count()} commandes, {group.Sum(p => p.Order.Amount):N0} €)");

            foreach (var p in group)
            {
                var c = p.Order;
                var indicCli = p.NbOtherClientOrders > 0
                    ? $" {p.NbOtherClientOrders + 1} commandes non payées"
                    : "";
                Console.WriteLine($"  N°{c.Number,-4} {c.FullName,-25} {c.Amount,6:N0} €  {p.Action}{indicCli}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Total à recouvrer : {plan.Sum(p => p.Order.Amount):N0} €");
    }
}
