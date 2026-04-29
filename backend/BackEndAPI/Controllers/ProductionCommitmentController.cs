using BackEndAPI.Models.ProductionCommitmentModel;
using Gridify;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductionCommitmentController(Service.ProductionCommitment.IProductionCommitmentService service)
    : Controller
{
    private readonly ResponseClients _responseClients = new ResponseClients();

    [HttpPost]
    public async Task<IActionResult> Create(ProductionCommitment productionCommitment)
    {
        var (newPro, mess) = await service.Create(productionCommitment);
        if (mess != null)
        {
            return _responseClients.GetStatusCode(mess.Status, mess);
        }

        return Ok(newPro);
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GridifyQuery q, string? search, int skip = 0, int limit = 30)
    {
        var (data, mess, total) = await service.GetProductionCommitments(skip, limit, q, search);
        if (mess != null)
        {
            return _responseClients.GetStatusCode(mess.Status, mess);
        }

        return Ok(new { data, total, skip, limit });
    }
}