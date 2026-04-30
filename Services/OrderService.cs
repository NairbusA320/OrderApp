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

    public enum AlertLevel
    {
        Critique,    // envoyée non payée + gros montant
        Importante,  // envoyée non payée
        Standard,    // non payée non envoyée, gros montant
        Surveillance // non payée non envoyée, petit montant
    }

    public record RecoveryPlan(
        Order Order,
        AlertLevel Level,
        int Score,
        string Action,
        int NbOtherClientOrders);
        
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

    public IEnumerable<(string Client, int NumberOrders, decimal Total)> TopClients(int n = 5) =>
        _orders
            .GroupBy(c => c.FullName)
            .Select(g => (Client: g.Key, NumberOrders: g.Count(), Total: g.Sum(c => c.Amount)))
            .OrderByDescending(x => x.Total)
            .Take(n);

    public IEnumerable<(string Entreprise, int NumberOrders, decimal Total)> TopEnterprises(int n = 5) =>
        _orders
            .GroupBy(c => c.Enterprise)
            .Select(g => (Entreprise: g.Key, NumberOrders: g.Count(), Total: g.Sum(c => c.Amount)))
            .OrderByDescending(x => x.NumberOrders)
            .Take(n);


    // -----------------------------------------------------------------
    // Actions
    // -----------------------------------------------------------------

    public IEnumerable<RecoveryPlan> GenerateRecoveryPlan()
    {
        // Compte les commandes non payées par client (pour détecter les clients récurrents)
        var ordersByClient = _orders
            .Where(c => !c.Paid)
            .GroupBy(c => c.FullName)
            .ToDictionary(g => g.Key, g => g.Count());

        return _orders
            .Where(c => !c.Paid)
            .Select(c =>
            {
                var nbOthers = ordersByClient[c.FullName] - 1;

                // Calcul du score : montant + bonus risque + bonus client récurrent
                int score = (int)c.Amount;
                if (c.Sent) score += 500;            // gros bonus si déjà envoyée
                if (nbOthers > 0) score += 100 * nbOthers; // bonus client récurrent

                AlertLevel level = (c.Sent, c.Amount) switch
                {
                    (true, >= 400)  => AlertLevel.Critique,
                    (true, _)       => AlertLevel.Importante,
                    (false, >= 400) => AlertLevel.Standard,
                    _               => AlertLevel.Surveillance
                };

                string action = level switch
                {
                    AlertLevel.Critique     => " Appel direct sous 24h — marchandise livrée non réglée",
                    AlertLevel.Importante   => " Relance recommandée + mise en demeure si pas de retour",
                    AlertLevel.Standard     => " Relance email avant expédition",
                    AlertLevel.Surveillance => " Surveiller, relancer à J+30",
                    _                       => ""
                };

                return new RecoveryPlan(c, level, score, action, nbOthers);
            })
            .OrderByDescending(p => p.Score);
    }
}
