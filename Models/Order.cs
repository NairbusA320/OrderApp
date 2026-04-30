namespace OrderApp.Models;

public class Order
{
    public int Number { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Enterprise { get; set; } = string.Empty;
    public bool Paid { get; set; }
    public bool Sent { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Statut métier déduit de la combinaison Payé / Envoyé.
    /// </summary>
    public string Status => (Paid, Sent) switch
    {
        (true, true)   => "Clôturée",
        (true, false)  => "À expédier",
        (false, false) => "À traiter",
        (false, true)  => "À risque" // envoyée sans paiement
    };
}
