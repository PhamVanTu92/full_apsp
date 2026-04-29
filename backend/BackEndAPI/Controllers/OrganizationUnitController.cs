using BackEndAPI.Models;
using BackEndAPI.Service.GenericeService;
using BackEndAPI.Service.OrganizationUnit;
using Gridify;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrganizationUnitController(IOrganizationUnitService organizationUnitService) : Controller
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OrganizationUnitDto organizationUnit)
    {
        var res = await organizationUnitService.CreateOrganiztionUnit(organizationUnit);

        return Ok(res);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update([FromBody] OrganizationUnitDto organizationUnit, int id)
    {
        var res = await organizationUnitService.UpdateOrganizationUnit(organizationUnit, id);

        return Ok(res);
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GridifyQuery gridifyQuery, [FromQuery] string? search)
    {
        var (res, total) = await organizationUnitService.GetOrganizationUnits(gridifyQuery, search);

        return Ok(new { items = res, total = total, pageSize = gridifyQuery.PageSize, page = gridifyQuery.Page });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        var res = await organizationUnitService.GetOrganizationUnitsById(id);

        return Ok(res);
    }

    [HttpGet("tree")]
    public async Task<IActionResult> GetTree()
    {
        var res = await organizationUnitService.GetOrganizationTreeAsync();

        return Ok(res);
    }

    [HttpPost("{id}/employees")]
    public async Task<IActionResult> AddEmployess([FromBody] List<int> employeesIds, [FromRoute] int id)
    {
        var res = await organizationUnitService.AddEmployess(id, employeesIds);

        return Ok(res);
    }

    [HttpDelete("{id}/employees")]
    public async Task<IActionResult> RemoveEmployess([FromBody] List<int?> employeesIds, [FromRoute] int id)
    {
        var res = await organizationUnitService.RemoveEmployess(id, employeesIds);

        return Ok(res);
    }

    [HttpPut("{id}/set-manager/{employeesId}")]
    public async Task<IActionResult> SetManager([FromRoute] int id, int employeesId)
    {
        var res = await organizationUnitService.SetManager(id, employeesId);

        return Ok(res);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        await organizationUnitService.DeleteOrgraniztion(id);

        return Ok();
    }
}