using CAS.FinancePortal.Web.Server.Core.Applications;
using CAS.FinancePortal.Web.Server.Core.Models.Dtos.User;
using CAS.FinancePortal.Web.Server.Core.UnitOfWork;
using Microsoft.AspNetCore.Mvc;

namespace CAS.FinancePortal.Web.Server.Persistence.Applications
{
	public class UserAuthenticationService (
		ICurrentApplicationUnitOfWork uowCurrentApplicationApplication,
		ILogger<UserAuthenticationService> logger
		) : IUserAuthenticationService
	{

		private readonly ICurrentApplicationUnitOfWork _uowCurrentApplicationApplication = uowCurrentApplicationApplication;
		private readonly ILogger<UserAuthenticationService> _logger = logger;


		public async Task<(bool IsSuccess, int Status, string Message, UserDto resultData)> VerifyTokenService(int userId)
		{
			try
			{
				var loggedUserDetails = await _uowCurrentApplicationApplication.UserAccessRepository.SelectLoggedUserDetails(userId);
				if (loggedUserDetails != null && loggedUserDetails.ReferenceId > 0)
				{
					// Enrich profile with Department details from Employee repository
					var employee = await _uowCurrentApplicationApplication.EmployeeRepository.SelectEmployeeInformationById(loggedUserDetails.ReferenceId);
					if (employee != null)
					{
						loggedUserDetails.DepartmentName = employee.DepartmentName;
						// Optionally ensure DepartmentId is also synchronized if it was missing
						loggedUserDetails.DepartmentId = employee.DepartmentId;
					}
				}
				return (true, 200, $"Successfully!", loggedUserDetails);

			}
			catch (Exception e)
			{
				_logger.LogError($"Error occurred while fetching User Details: {e.Message}");
				return (false, 400, $"Error occurred while fetching User Details: {e.Message}", null)!;
			}

		}

		public async Task<(bool IsSuccess, int Status, string Message, UserDto resultData)> VerifyTokenByEmailService(string email)
		{
			try
			{
				var loggedUserDetails = await _uowCurrentApplicationApplication.UserAccessRepository.SelectLoggedUserDetailsByEmail(email);
				return (true, 200, $"Successfully!", loggedUserDetails);
			}
			catch (Exception e)
			{
				_logger.LogError($"Error occurred while fetching User Details by email: {e.Message}");
				return (false, 400, $"Error occurred while fetching User Details: {e.Message}", null)!;
			}
		}

		public async Task<(bool IsSuccess, int Status, string Message, List<UserSystemMenuDto> leftSideBarMenus)> LeftSideBarService(int userId)
		{
			try
			{
				var leftSideBarMenus = await _uowCurrentApplicationApplication.UserAccessRepository.UserMenus(userId!);

				return (true, 200, $"Successfully!", leftSideBarMenus);

			}
			catch (Exception e)
			{
				_logger.LogError($"Error occurred while fetching User Menus: {e.Message}");
				return (false, 400, $"Error occurred while fetching User Menus: {e.Message}", []);
			}

		}


		public async Task<(bool IsSuccess, int Status, string Message, List<string> resultData)> UserMenuRouteService(int userId)
		{
			try
			{
				var routes = await _uowCurrentApplicationApplication.UserAccessRepository.UserMenuPaths(userId!);

				return (true, 200, $"Successfully!", routes);

			}
			catch (Exception e)
			{
				_logger.LogError($"Error occurred while fetching User Menus: {e.Message}");
				return (false, 400, $"Error occurred while fetching User Menus: {e.Message}", []);
			}

		}
	}
}
