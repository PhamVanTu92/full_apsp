using Microsoft.AspNetCore.Mvc;
using BackEndAPI.Service.StorageFee;
using Microsoft.AspNetCore.Authorization;

using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace BackEndAPI.Controllers
{
    [ApiController]
    [Route("/api")]
    public class StorageFeeController(IStorageFeeService service) : Controller
    {
        private readonly ResponseClients _responseClients = new();

        [AllowAnonymous]
        [HttpGet("/api/fee-milestones")]
        public async Task<IActionResult> GetFeeMilestones(int skip = 0, int limit = 100)
        {
            var (feeMilestones, mess, total) = await service.GetFeeMilestonesAsync(skip, limit);
            if (mess != null)
            {
                return _responseClients.GetStatusCode(mess.Status, mess);
            }
            return Ok(new { feeMilestones, total, skip, limit });
        }

        [AllowAnonymous]
        [HttpPost("/api/fee-milestones")]
        public async Task<IActionResult> CreateFeeMilestone([FromBody] List<Models.StorageFee.FeeMilestoneDto> feeDto)
        {
            var (feeMilestone, mess) = await service.CreateFeeMilestoneAsync(feeDto);
            if (mess != null)
            {
                return _responseClients.GetStatusCode(mess.Status, mess);
            }
            return Ok(new { feeMilestone });
        }

        [AllowAnonymous]
        [HttpGet("/api/fee-milestones/{id}")]
        public async Task<IActionResult> CreateFeeMilestone(int id,[FromBody] Models.StorageFee.FeeMilestoneDto feeDto)
        {
            feeDto.Id = id;

            var mess = await service.UpdateFeeMilestoneAsync(feeDto);
            if (mess != null)
            {
                return _responseClients.GetStatusCode(mess.Status, mess);
            }
            return Ok("Update Success");
        }

        [AllowAnonymous]
        [HttpDelete("/api/fee-milestones/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var mess = await service.DeleteFeeMilestoneAsync(id);
            if (mess != null)
            {
                return _responseClients.GetStatusCode(mess.Status, mess);
            }
            return Ok("Update Success");
        }
        // Phí lưu kho
        // Storage Fee

        [AllowAnonymous]
        [HttpGet("/api/storage-fees")]
        public async Task<IActionResult> GetStorageFee(int skip = 0, int limit = 100)
        {
            var (storageFees, mess, total) = await service.GetStorageFeeAync(skip, limit);
            if (mess != null)
            {
                return _responseClients.GetStatusCode(mess.Status, mess);
            }
            return Ok(new { storageFees, total, skip, limit });
        }

        [AllowAnonymous]
        [HttpGet("/api/storage-fees/{id}")]
        public async Task<IActionResult> GetStorageFeeById(int id)
        {
            var (storageFee, mess) = await service.GetStorageFeeByIdAync(id);
            if (mess != null)
            {
                return _responseClients.GetStatusCode(mess.Status, mess);
            }
            return Ok(new { storageFee });
        }

        [AllowAnonymous]
        [HttpPost("/api/storage-fees")]
        public async Task<IActionResult> CreateStorageFee([FromBody] Models.StorageFee.StorageFeeDto storageFeeDto)
        {
            var (storageFee, mess) = await service.CreateStorageFeeAsync(storageFeeDto);

            if (mess != null)
            {
                return  _responseClients.GetStatusCode(mess.Status, mess);
            }

            return Ok(new { storageFee });
        }

        [AllowAnonymous]
        [HttpPut("/api/storage-fees/{id}")]
        public async Task<IActionResult> UpdateStorageFee([FromBody] Models.StorageFee.StorageFeeDto storageFeeDto, int id)
        {
            storageFeeDto.Id = id;
            var mess = await service.UpdateStorageFeeAsync(storageFeeDto);

            if (mess != null)
            {
                return _responseClients.GetStatusCode(mess.Status, mess);
            }

            return Ok("Update sucess");
        }

        [AllowAnonymous]
        [HttpDelete("/api/storage-fees/{id}")]
        public async Task<IActionResult> DeleteStorageFee(int id)
        {
            var mess = await service.DeleteStorageFeeAsync(id);

            if (mess != null)
            {
                return _responseClients.GetStatusCode(mess.Status, mess);
            }

            return Ok("Delete success");
        }

        [AllowAnonymous]
        [HttpPost("/api/storage-fees/{id}/lines")]
        public async Task<IActionResult> AddLinesToStorageFee([FromBody] List<Models.StorageFee.StorageFeeLineDto> lines, int id)
        {
            var mess = await service.AddLinesToStorageFeeAsync(id,lines);

            if (mess != null)
            {
                return _responseClients.GetStatusCode(mess.Status, mess); 
            }

            return Ok("success");
        }
        [AllowAnonymous]
        [HttpDelete("/api/storage-fees/{id}/lines")]
        public async Task<IActionResult> RemoveLineFromStorageFee([FromBody] List<int> lineIds)
        {
            var mess = await service.RemoveLinesFromStorageFee(lineIds);

            if (mess != null)
            {
                return _responseClients.GetStatusCode(mess.Status, mess);
            }

            return Ok("success");
        }
    }

}
