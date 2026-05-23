using CAS.FinancePortal.Web.Server.Core.Models.Dtos.User;
using CAS.FinancePortal.Web.Server.Core.Repositories;
using CAS.FinancePortal.Web.Server.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace CAS.FinancePortal.Web.Server.Persistence.Repositories
{
	public class UserAccessRepository(CurrentApplicationDbContext dbContext) : IUserAccessRepository
	{
		public async Task<UserDto> SelectLoggedUserDetails(int userId)
		{
			var userDetails = await (from s in dbContext.VwCombinedUserInformations
                                     where s.UserId == userId
                                     select new UserDto
                                     {
                                         UserId = s.UserId,
                                         ReferenceId = s.UserReferenceId,
                                         FirstName = s.FirstName,
                                         LastName = s.LastName,
                                         Email = s.UserEmail,
                                         DepartmentId = s.DepartmentId,
                                         UserAccessId = 1, // Defaulting for now as it's not in the view in screenshot
                                     }).FirstOrDefaultAsync();
			return userDetails!;
		}

		public async Task<UserDto> SelectLoggedUserDetailsByEmail(string email)
		{
			var userDetails = await dbContext.VwCombinedUserInformations
				.Where(s => s.UserEmail == email)
				.Select(s => new UserDto
				{
					UserId = s.UserId,
					ReferenceId = s.UserReferenceId,
					FirstName = s.FirstName,
					LastName = s.LastName,
					Email = s.UserEmail,
				})
				.FirstOrDefaultAsync();
			return userDetails!;
		}


		public async Task<List<UserSystemMenuDto>> UserMenus(int userId)
		{
			var userMenus = await dbContext.SystemMenus
				.Include(i => i.UserAccessType)
				.ThenInclude(t => t.UserAccesses)
				.Where(w => w.UserAccessType.UserAccesses.Count(c => c.UserId == userId) > 0 && w.MenuPath != null)
				.AsNoTracking()
				.OrderBy(o => o.OrderBy)
				.ToListAsync();


			var userMenuDto = userMenus.Select(mp => new UserSystemMenuDto()
			{

				MenuId = mp.Id,
				MenuName = mp.MenuName,
				Icon = mp.MenuIcon,
				Path = mp.MenuPath,
				ParentId = mp.ParentId,
				IsParent = mp.IsParent,
				IsSubParent = mp.IsSubParent,
				SubMenus = userMenus
					.Where(w => w.ParentId == mp.Id)
					.Select(ms => new UserSystemMenuDto()
					{
						MenuId = ms.Id,
						MenuName = ms.MenuName,
						Icon = ms.MenuIcon,
						Path = ms.MenuPath,
						ParentId = ms.ParentId,
						IsParent = ms.IsParent,
						IsSubParent = ms.IsSubParent,
						SubMenus = userMenus
							.Where(w => w.ParentId == ms.Id)
							.Select(msc => new UserSystemMenuDto()
							{
								MenuId = msc.Id,
								MenuName = msc.MenuName,
								Icon = msc.MenuIcon,
								Path = msc.MenuPath,
								ParentId = msc.ParentId,
								IsParent = msc.IsParent,
								IsSubParent = msc.IsSubParent,
							}).ToList()
					}).ToList()
			})
			.Where(w=> w.IsParent == true)
				.ToList();


			return userMenuDto;
		}
		public async Task<List<string>> UserMenuPaths(int userId)
		{
			var userMenuPaths = await dbContext.SystemMenus
				.Include(i => i.UserAccessType)
				.ThenInclude(t => t.UserAccesses)
				.Where(w => w.UserAccessType.UserAccesses.Count(c => c.UserId == userId) > 0 && w.MenuPath != null)
				.Select(s => s.MenuPath)
				.AsNoTracking()
				.ToListAsync();
			return userMenuPaths;
		}
	}
}
