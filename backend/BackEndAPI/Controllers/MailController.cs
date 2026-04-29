using BackEndAPI.Models.Other;
using BackEndAPI.Service.Mail;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MailController : Controller
    {

        IMailService Mail_Service;
        //injecting the IMailService into the constructor
        public MailController(IMailService _MailService)
        {
            Mail_Service = _MailService;
        }
        [HttpPost]
        public async Task<IActionResult> SendMail(MailData Mail_Data)
        {
            var(check,mess) = await Mail_Service.SendMail(Mail_Data);
            if (!check)
            {
                return BadRequest(new { mess.Status, mess.Errors });
            }
            return Ok();
        }
    }
}
