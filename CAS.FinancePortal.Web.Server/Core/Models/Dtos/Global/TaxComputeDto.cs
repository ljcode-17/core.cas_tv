namespace CAS.FinancePortal.Web.Server.Core.Models.Dtos.Global;

public class TaxComputeItemDto
{
    public decimal Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public int TaxTypeId { get; set; }
    public int EwtTypeId { get; set; }
}

public class TaxComputeRequestDto
{
    public int TaxBasisId { get; set; }
    public List<TaxComputeItemDto> Items { get; set; } = new();
}

public class TaxComputeResponseDto
{
    public decimal NetOfVat { get; set; }
    public decimal VatAmount { get; set; }
    public decimal EwtAmount { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal GrandTotal { get; set; }
    public string FormattedGrandTotal { get; set; } = string.Empty;
}
