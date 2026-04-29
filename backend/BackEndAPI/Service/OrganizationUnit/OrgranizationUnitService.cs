using System.Net;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BackEndAPI.Data;
using BackEndAPI.Infr.Exceptions;
using Gridify;

namespace BackEndAPI.Service.OrganizationUnit;

public class OrganizationUnitService(AppDbContext context, IMapper mapper) : IOrganizationUnitService
{
    public async Task<(List<Models.OrganizationUnit>, int)> GetOrganizationUnits(GridifyQuery gridifyQuery,
        string? search)
    {
        var query = context
            .OrganizationUnit
            .AsNoTracking()
            .Include(x => x.ManagerUser)
            .ThenInclude(x => x.DirectStaff)
            .Include(x => x.Employees)
            .ApplyFiltering(gridifyQuery)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            search = search.ToLower().Trim();
            query = query.Where(x => x.Name.Contains(search));
        }

        var total = await query.CountAsync();

        var orgList = await query
            .Include(x => x.Parent)
            .ApplyPaging(gridifyQuery).ApplyOrdering(gridifyQuery)
            .ToListAsync();

        return (orgList, total);
    }

    public async Task<Models.OrganizationUnit> GetOrganizationUnitsById(int id)
    {
        var org = await context.OrganizationUnit.AsNoTracking()
            .Include(x => x.Parent)
            .Include(x => x.ManagerUser)
            .ThenInclude(x => x.DirectStaff)
            .Include(x => x.Employees)
            .FirstOrDefaultAsync(x => x.Id == id);


        if (org == null)
        {
            throw new BusinessException($"Organization unit with id: {id} was not found", HttpStatusCode.NotFound,
                "404");
        }

        org.Children = await context.OrganizationUnit.AsNoTracking().Include(x => x.Employees)
            .Where(x => x.ParentId == id)
            .ToListAsync();
        // org.EmployeesCount = org.Children.Count();

        return org;
    }

    public async Task<Models.OrganizationUnit> CreateOrganiztionUnit(Models.OrganizationUnitDto organizationUnitDto)
    {
        var maxCode = context.OrganizationUnit
            .Where(c => c.Code.StartsWith("CCTC"))
            .OrderByDescending(c => c.Code)
            .Select(c => c.Code)
            .FirstOrDefault();

        int newNumber = 1;
        if (!string.IsNullOrEmpty(maxCode))
        {
            var numberPart = maxCode.Substring(4);
            if (int.TryParse(numberPart, out var currentNumber))
            {
                newNumber = currentNumber + 1;
            }
        }

        if (string.IsNullOrEmpty(organizationUnitDto.Code))
        {
            organizationUnitDto.Code = $"CCTC{newNumber:00000}";
        }

        var org = mapper.Map<Models.OrganizationUnit>(organizationUnitDto);

        var employees = context.AppUser.Where(x => organizationUnitDto.EmployeesIds.Contains(x.Id)).ToList();
        org.Employees = employees;


        var usrManger = await context.AppUser.Where(x => x.Id == org.ManagerUserId).Include(u => u.DirectStaff)
            .FirstOrDefaultAsync();
        if (usrManger != null)
        {
            usrManger.DirectStaff.AddRange(employees);
        }

        context.OrganizationUnit.Add(org);

        await context.SaveChangesAsync();

        return org;
    }

    public async Task<Models.OrganizationUnit> UpdateOrganizationUnit(Models.OrganizationUnitDto organizationUnitDto,
        int id)
    {
        var org = await context.OrganizationUnit.Include(x => x.Employees).FirstOrDefaultAsync(x => x.Id == id);

        if (org == null)
        {
            throw new BusinessException($"Organization unit with id: {id} was not found", HttpStatusCode.NotFound,
                "404");
        }

        mapper.Map(organizationUnitDto, org);

        // org.Employees = context.AppUser.Where(x => organizationUnitDto.EmployeesIds.Contains(x.Id)).ToList();

        await context.SaveChangesAsync();

        return org;
    }

    public async Task<List<Models.OrganizationUnit>> GetOrganizationTreeAsync()
    {
        var allUnits =
            await context.OrganizationUnit
                .AsNoTracking()
                .Include(x => x.Employees).Include(p => p.Parent)
                .ToListAsync(); // Lấy toàn bộ danh sách

        // Tạo dictionary để nhóm theo Id
        var unitDict = allUnits.ToDictionary(u => u.Id);
        List<Models.OrganizationUnit> rootUnits = new();

        foreach (var unit in allUnits)
        {
            if (unit.ParentId == null)
            {
                // Nếu không có ParentId => đây là node gốc
                rootUnits.Add(unit);
            }
            else if (unitDict.TryGetValue(unit.ParentId.Value, out var parentUnit))
            {
                // Nếu có ParentId, gán vào danh sách con của Parent
                parentUnit.Children.Add(unit);
            }
        }

        return rootUnits;
    }

    public async Task<Models.OrganizationUnit> AddEmployess(int id, List<int> employeeIds)
    {
        var org = await context.OrganizationUnit.Include(x => x.Employees).FirstOrDefaultAsync(x => x.Id == id);

        if (org == null)
        {
            throw new BusinessException($"Organization unit with id: {id} was not found", HttpStatusCode.NotFound,
                "404");
        }

        var employees = context.AppUser.Where(x => employeeIds.Contains(x.Id)).ToList();

        org.Employees.AddRange(employees);
        var usrManger = await context.AppUser.Where(x => x.Id == org.ManagerUserId).Include(u => u.DirectStaff)
            .FirstOrDefaultAsync();

        if (usrManger != null)
        {
            usrManger.DirectStaff.AddRange(employees);
        }
        // org.Employees = context.AppUser.Where(x => organizationUnitDto.EmployeesIds.Contains(x.Id)).ToList();

        await context.SaveChangesAsync();

        return org;
    }

    public async Task<Models.OrganizationUnit> RemoveEmployess(int id, List<int?> employeeIds)
    {
        var org = await context.OrganizationUnit.Include(x => x.Employees).FirstOrDefaultAsync(x => x.Id == id);

        if (org == null)
        {
            throw new BusinessException($"Organization unit with id: {id} was not found", HttpStatusCode.NotFound,
                "404");
        }

        org.Employees.RemoveAll(x => employeeIds.Contains(x.Id));
        var usrManger = await context.AppUser.Where(x => x.Id == org.ManagerUserId).Include(u => u.DirectStaff)
            .FirstOrDefaultAsync();

        usrManger.DirectStaff.RemoveAll(x => employeeIds.Contains(x.Id));

        if (employeeIds.Contains(org.ManagerUserId ?? -100))
        {
            org.ManagerUserId = null;
        }

        var bpList = context.BP.Where(x => employeeIds.Contains(x.SaleId)).ToList();
        bpList.ForEach(x => x.SaleId = org.ManagerUserId);

        // org.Employees = context.AppUser.Where(x => organizationUnitDto.EmployeesIds.Contains(x.Id)).ToList();

        await context.SaveChangesAsync();

        return org;
    }

    public async Task<Models.OrganizationUnit> SetManager(int id, int managerId)
    {
        var org = await context
            .OrganizationUnit
            .Include(x => x.Employees)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (org == null)
        {
            throw new BusinessException($"Organization unit with id: {id} was not found", HttpStatusCode.NotFound,
                "404");
        }

        var usrManger = await context.AppUser.Where(x => x.Id == org.ManagerUserId).Include(u => u.DirectStaff)
            .FirstOrDefaultAsync();
        if (usrManger != null)
        {
            usrManger.DirectStaff.RemoveAll(x => org.Employees.Select(d => d.Id).Contains(x.Id));
        }

        org.ManagerUserId = managerId;
        var newUsrmanger = await context.AppUser.Where(x => x.Id == org.ManagerUserId).Include(u => u.DirectStaff)
            .FirstOrDefaultAsync();

        if (newUsrmanger != null)
        {
            newUsrmanger.DirectStaff.AddRange(org.Employees.Where(x => x.Id != org.ManagerUserId).ToList());
        }

        await context.SaveChangesAsync();

        return org;
    }

    public async Task DeleteOrgraniztion(int id)
    {
        await using var transaction = await context.Database.BeginTransactionAsync();

        var org = await context.OrganizationUnit.Include(x => x.Employees)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (org == null)
        {
            await transaction.RollbackAsync();
            throw new BusinessException($"Organization unit with id: {id} was not found", HttpStatusCode.NotFound,
                "404");
        }

        var checkChidld = await context.OrganizationUnit.Where(x => x.ParentId == id).CountAsync();
        if (checkChidld > 0)
        {
            await transaction.RollbackAsync();
            throw new BusinessException("Không thể xóa cơ cấu tổ chức này vì đang chứa đơn vị trực thuộc",
                HttpStatusCode.BadRequest,
                "400");
        }


        try
        {
            if (org.ParentId != null)
            {
                var p = await context.OrganizationUnit.Include(x => x.Employees).Where(x => x.Id == org.ParentId)
                    .FirstOrDefaultAsync();

                p.Employees.AddRange(org.Employees);
            }

            context.OrganizationUnit.Remove(org);
            await context.SaveChangesAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        await transaction.CommitAsync();
    }
}