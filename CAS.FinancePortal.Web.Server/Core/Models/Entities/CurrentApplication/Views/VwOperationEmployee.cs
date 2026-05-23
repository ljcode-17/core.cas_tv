namespace CAS.FinancePortal.Web.Server.Core.Models.Entities.CurrentApplication.Views;

public class VwOperationEmployee
{
	public bool IsActive { get; set; }

	public int EmployeeId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public int DepartmentGroupId { get; set; }
	public int CoreServiceId { get; set; }

	public int? TeamId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;

    public string? TeamName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string CompanyEmail { get; set; } = string.Empty;
    public string? Tin { get; internal set; }
}
