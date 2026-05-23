using CAS.FinancePortal.Web.Server.Core.Models.Dtos.User;

namespace CAS.FinancePortal.Web.Server.Core.Repositories;

public interface IUserAccessRepository
{
	Task<UserDto> SelectLoggedUserDetails(int userId);
	Task<UserDto> SelectLoggedUserDetailsByEmail(string email);
	Task<List<UserSystemMenuDto>> UserMenus(int userId);
	Task<List<string>> UserMenuPaths(int userId);
}