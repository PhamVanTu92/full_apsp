using BackEndAPI.Service.Test;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VnpayPaymentController(IVnPayPaymentService vnPayPaymentService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> PayAsync(CreatePaymentDto dto)
    {
        var result = await vnPayPaymentService.PaymentAsync(dto);
        if (result.Item2 is not null) return Ok(result.Item2);
        return Ok(result.Item1);
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await vnPayPaymentService.CallBackAsync(Request.Query);

        if (result.Item2 is not null)
        {
            return Ok(new
            {
                status  = -1,
                message = result.Item2.Errors,
                value   = result.Item1,
                obj     = result.Item3
            });
        }

        var data = result.Item1?.Split("/");
        return Ok(new
        {
            status = 0,
            value  = data?.Length > 1 ? data[1] : null,
            obj    = result.Item3
        });
    }


    [HttpGet("IPN")]
    public async Task<IActionResult> CallBack()
    {
        var result = await vnPayPaymentService.IpnAsync(Request.Query);
        return Ok(result);
    }
}