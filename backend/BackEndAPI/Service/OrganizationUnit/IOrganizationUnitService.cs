using Gridify;

namespace BackEndAPI.Service.OrganizationUnit;

public interface IOrganizationUnitService
{
    Task<(List<Models.OrganizationUnit>, int)> GetOrganizationUnits(GridifyQuery gridifyQuery,
        string? search);

    Task<Models.OrganizationUnit> GetOrganizationUnitsById(int id);
    Task<Models.OrganizationUnit> CreateOrganiztionUnit(Models.OrganizationUnitDto organizationUnitDto);
    Task<Models.OrganizationUnit> UpdateOrganizationUnit(Models.OrganizationUnitDto organizationUnitDto, int id);
    Task<List<Models.OrganizationUnit>> GetOrganizationTreeAsync();
    Task<Models.OrganizationUnit> SetManager(int id, int managerId);
    Task<Models.OrganizationUnit> RemoveEmployess(int id, List<int?> employeeIds);
    Task<Models.OrganizationUnit> AddEmployess(int id, List<int> employeeIds);
    Task DeleteOrgraniztion(int id);
}