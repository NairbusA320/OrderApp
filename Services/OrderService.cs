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
