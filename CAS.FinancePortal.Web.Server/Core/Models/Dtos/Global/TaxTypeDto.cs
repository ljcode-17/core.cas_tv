namespace CAS.FinancePortal.Web.Server.Core.Models.Dtos.Global;

public class TaxTypeDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public int TaxBasisId { get; set; }
}
