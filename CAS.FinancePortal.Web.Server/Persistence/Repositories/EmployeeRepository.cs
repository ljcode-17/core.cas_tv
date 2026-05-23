using CAS.FinancePortal.Web.Server.Core.Models.Dtos.Global;
using CAS.FinancePortal.Web.Server.Core.Repositories;
using CAS.FinancePortal.Web.Server.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace CAS.FinancePortal.Web.Server.Persistence.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
	{
		private readonly CurrentApplicationDbContext dbContext;

		public EmployeeRepository(CurrentApplicationDbContext dbContext)
		{
			this.dbContext = dbContext;
		}
		public async Task<List<DropDownDto>> SelectEmployeesDropDown(int departmentId, int? teamId)
		{
			var query = dbContext.VwEmployees
				.Where(w => w.IsActive)
				.AsNoTracking();

			if (departmentId != 0)
				query = query.Where(w => w.DepartmentId == departmentId);


			var departments = await query
				.Select(s => new DropDownDto()
				{
					Id = s.EmployeeId,
					Name = s.FirstName + " " + s.LastName
				})
				.OrderBy(o => o.Name)
				.ToListAsync();

			return departments;
		}

		public async Task<int?> GetEmployeeIdBasedOnName(int id)
		{
			var query =  await dbContext.VwEmployees
				.AsNoTracking()
				.FirstOrDefaultAsync(w => w.EmployeeId == id);

			return query?.EmployeeId;
		}

		public async Task<List<DropDownDto>> SelectMultipleEmployeesDropDown(int[] departmentIds, int[]? teamIds)
		{
			var query = dbContext.VwEmployees
				.Where(w => w.IsActive)
				.AsNoTracking();

			if (departmentIds.Length > 0)
				query = query.Where(w => departmentIds.Contains(w.DepartmentId));


			var departments = await query
				.Select(s => new DropDownDto()
				{
					Id = s.EmployeeId,
					Name = s.FirstName + " " + s.LastName
				})
				.OrderBy(o => o.Name)
				.ToListAsync();

			return departments;
		}

		public async Task<VwOperationEmployee?> SelectEmployeeInformationById(int id)
		{
			var query = await dbContext.VwEmployees
				.AsNoTracking()
				.FirstOrDefaultAsync(w => w.EmployeeId == id);

            if (query == null) return null;

            var dept = await dbContext.VwDepartments.FirstOrDefaultAsync(d => d.Id == query.DepartmentId);

            return new VwOperationEmployee
            {
                EmployeeId = query.EmployeeId,
                FirstName = query.FirstName,
                LastName = query.LastName,
                IsActive = query.IsActive,
                DepartmentId = query.DepartmentId,
                DepartmentName = dept?.DepartmentName ?? "N/A",
                CompanyEmail = query.CompanyEmail,
                Position = "N/A",
                DepartmentGroupId = 0,
                CoreServiceId = 0
            };
        }

        public async Task<List<VwOperationEmployee>> GetAllEmployeeInformation()
        {
            var employees = await dbContext.VwEmployees.Where(e => e.IsActive).AsNoTracking().ToListAsync();
            var depts = await dbContext.VwDepartments.AsNoTracking().ToListAsync();
            var deptMap = depts.ToDictionary(d => d.Id, d => d.DepartmentName);

            return employees.Select(e => new VwOperationEmployee
            {
                EmployeeId = e.EmployeeId,
                FirstName = e.FirstName,
                LastName = e.LastName,
                IsActive = e.IsActive,
                DepartmentId = e.DepartmentId,
                DepartmentName = deptMap.ContainsKey(e.DepartmentId) ? deptMap[e.DepartmentId] : "N/A",
                CompanyEmail = e.CompanyEmail,
                Position = "N/A"
            }).ToList();
        }

        public async Task<List<PayeeDto>> SelectEmployeePayeeDropDown()
        {
            var data = await dbContext.VwEmployees
                .Where(e => e.IsActive)
                .Select(e => new
                {
                    e.YearId,
                    e.EmployeeId,
                    e.FirstName,
                    e.LastName,
                    e.Tin
                })
                .ToListAsync();

            return data.Select(e => new PayeeDto
            {
                PayeeCode = $"E-{e.YearId ?? ""}-{e.EmployeeId}",
                Name = $"{e.FirstName} {e.LastName}",
                Tin = e.Tin,
                PayeeType = "Employee"
            })
            .OrderBy(o => o.Name)
            .ToList();
        }

        public async Task<Signature?> SelectSignatureByEmployeeId(int employeeId)
        {
            return await dbContext.Signatures
                .Where(s => s.EmployeeId == employeeId && s.IsActive)
                .OrderByDescending(s => s.CreatedDate)
                .FirstOrDefaultAsync();
        }

        public async Task AddSignature(Signature signature)
        {
            await dbContext.Signatures.AddAsync(signature);
        }

    }
}
