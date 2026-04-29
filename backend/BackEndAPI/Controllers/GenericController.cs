using BackEndAPI.Service.GenericeService;
using Gridify;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAPI.Controllers;
[ApiController]
[Route("api/[controller]")]
public class GenericController<T> : ControllerBase where T : class
{
    private readonly IGenericService<T> _service;

    public GenericController(IGenericService<T> service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetList([FromQuery] GridifyQuery gridifyQuery)
    {
        var result = await _service.GetListAsync(gridifyQuery);
        return Ok(new { items = result.Items, total = result.TotalCount });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetByIdAsync(id);
        return item != null ? Ok(item) : NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Create(T entity)
    {
        var created = await _service.CreateAsync(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.GetType().GetProperty("Id")?.GetValue(created) }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, T entity)
    {
        var entityId = entity.GetType().GetProperty("Id")?.GetValue(entity);
        if (entityId == null || !entityId.Equals(id))
        {
            return BadRequest("ID mismatch");
        }

        var updated = await _service.UpdateAsync(entity);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
}

