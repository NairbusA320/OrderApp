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

}
