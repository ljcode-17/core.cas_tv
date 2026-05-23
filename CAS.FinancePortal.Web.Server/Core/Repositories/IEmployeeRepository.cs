using CAS.FinancePortal.Web.Server.Core.Models.Dtos.Global;
using CAS.FinancePortal.Web.Server.Core.Models.Entities.CurrentApplication.Views;
using CAS.FinancePortal.Web.Server.Core.Models.Entities.CurrentApplication.Tables;

namespace CAS.FinancePortal.Web.Server.Core.Repositories;

public interface IEmployeeRepository
{
	Task<List<DropDownDto>> SelectEmployeesDropDown(int departmentId, int? teamId);

	Task<int?> GetEmployeeIdBasedOnName(int id);
	Task<List<DropDownDto>> SelectMultipleEmployeesDropDown(int[] departmentIds, int[]? teamIds);
	Task<VwOperationEmployee?> SelectEmployeeInformationById(int employeeId);
    Task<List<VwOperationEmployee>> GetAllEmployeeInformation();
    Task<List<PayeeDto>> SelectEmployeePayeeDropDown();
    Task<Signature?> SelectSignatureByEmployeeId(int employeeId);
    Task AddSignature(Signature signature);
}
