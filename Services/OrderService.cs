using OrderApp.Models;

namespace OrderApp.Services;

public class OrderService
{
    private readonly List<Order> _orders;
    private readonly Dictionary<string, ExpeditionType> _typesByEnterprise;
    private readonly Dictionary<ExpeditionType, decimal> _expeditionValues;

    public OrderService(
        List<Order> orders,
        Dictionary<string, ExpeditionType> typesByEnterprise,
        Dictionary<ExpeditionType, decimal> expeditionValues)
    {
        _orders = orders;
        _typesByEnterprise = typesByEnterprise;
        _expeditionValues = expeditionValues;
    }
        
    // -----------------------------------------------------------------
    // Lecture / recherche
    // -----------------------------------------------------------------

    public IReadOnlyList<Order> AllOrders() => _orders;
    
    public Order? SearchByNumber(int number) =>
        _orders.FirstOrDefault(c => c.Number == number);

    public IEnumerable<Order> SearchByClient(string name) =>
        _orders.Where(c =>
            c.FullName.Contains(name, StringComparison.OrdinalIgnoreCase));

    public IEnumerable<Order> SearchByEnterprise(string name) =>
        _orders.Where(c =>
            c.Enterprise.Contains(name, StringComparison.OrdinalIgnoreCase));

    // -----------------------------------------------------------------
    // Filtres
    // -----------------------------------------------------------------

    public IEnumerable<Order> PaidOrders() =>
        _orders.Where(c => c.Paid);

    public IEnumerable<Order> UnpaidOrders() =>
        _orders.Where(c => !c.Paid);

    public IEnumerable<Order> SentOrders() =>
        _orders.Where(c => c.Sent);

    public IEnumerable<Order> UnsentOrders() =>
        _orders.Where(c => !c.Sent);

    /// <summary>
    /// Commandes à risque : envoyées mais pas payées.
    /// </summary>
    public IEnumerable<Order> OrdersAtRisk() =>
        _orders.Where(c => c.Sent && !c.Paid);

    /// <summary>
    /// File d'expédition : commandes payées mais pas encore envoyées, triées par priorité d'expédition décroissante.
    /// </summary>
    public IEnumerable<Order> ExpeditionLineByPriority() =>
        _orders
            .Where(c => c.Paid && !c.Sent)
            .OrderBy(c => PriorityOrder(GetExpeditionType(c)));

    // -----------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------

    public ExpeditionType GetExpeditionType(Order c)
    {
        if (!_typesByEnterprise.TryGetValue(c.Enterprise, out var type))
            throw new InvalidOperationException(
                $"Aucun type d'expédition défini pour l'entreprise '{c.Enterprise}'.");
        return type;
    }

    private static int PriorityOrder(ExpeditionType t) => t switch
    {
        ExpeditionType.Urgent      => 0,
        ExpeditionType.Prioritaire => 1,
        ExpeditionType.Normal      => 2,
        ExpeditionType.Lent        => 3,
        _ => 99
    };

    public decimal GetExpeditionCost(Order c) =>
        _expeditionValues[GetExpeditionType(c)];

    // -----------------------------------------------------------------
    // Statistiques
    // -----------------------------------------------------------------

    public decimal TotalIncome() =>
        _orders.Where(c => c.Paid).Sum(c => c.Amount);

    public decimal PendingToRecover() =>
        _orders.Where(c => !c.Paid).Sum(c => c.Amount);

    public decimal TotalAmountAtRisk() =>
        OrdersAtRisk().Sum(c => c.Amount);

    /// <summary>
    /// Coût total d'expédition pour les commandes restant à envoyer.
    /// </summary>
    public decimal PendingExpeditionsCost() =>
        _orders
            .Where(c => !c.Sent)
            .Sum(c => _expeditionValues[GetExpeditionType(c)]);

    public IEnumerable<(ExpeditionType Type, int NumberOrders, decimal TotalCost)> TypeDistribution() =>
        _orders.GroupBy(GetExpeditionType)
               .Select(g => (
                   Type: g.Key,
                   NumberOrders: g.Count(),
                   TotalCost: g.Count() * _expeditionValues[g.Key]))
               .OrderBy(x => PriorityOrder(x.Type));
}
