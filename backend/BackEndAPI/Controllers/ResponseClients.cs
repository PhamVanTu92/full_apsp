using Microsoft.AspNetCore.Mvc;

namespace BackEndAPI.Controllers
{
    public class ResponseClients : ControllerBase
    {

        public IActionResult GetStatusCode(int statusCode, Models.Other.Mess mess) => statusCode switch
        {
            400 => BadRequest(mess),
            404 => NotFound(mess),
            500 => StatusCode(500, mess),
            _ => StatusCode(statusCode, mess),
        };


    }
}
