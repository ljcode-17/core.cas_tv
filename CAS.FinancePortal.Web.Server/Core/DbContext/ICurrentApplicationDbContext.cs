using Microsoft.EntityFrameworkCore;

namespace CAS.FinancePortal.Web.Server.Core.DbContext;

public interface ICurrentApplicationDbContext
{
	DbSet<VwCombinedUserInformation> VwCombinedUserInformations { get; set; }
	DbSet<VwDepartment> VwDepartments { get; set; }
    DbSet<VwEmployee> VwEmployees { get; set; }
	DbSet<SystemMenu> SystemMenus { get; set; }
	DbSet<UserAccess> UserAccesses { get; set; }
	DbSet<UserAccessType> UserAccessTypes { get; set; }
	DbSet<Signature> Signatures { get; set; }
	DbSet<CAS.FinancePortal.Web.Server.Core.Models.Entities.CurrentApplication.Tables.Video> Videos { get; set; }

	Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}