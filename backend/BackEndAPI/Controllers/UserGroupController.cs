using System.Net.Mail;
using BackEndAPI.Models.Account;
using BackEndAPI.Service.Account;
using Gridify;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserGroupController(IUserGroupService service) : Controller
{
    private readonly IUserGroupService _service = service;
    private readonly ResponseClients _responseClients = new ResponseClients();

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetUserGroups([FromQuery] GridifyQuery q, string? search, int skip = 0,
        int limit = 100)
    {
        var (groups, mess, total) = await _service.GetUserGroups(skip, limit, q, search);
        if (mess != null)
        {
            return _responseClients.GetStatusCode(mess.Status, mess);
        }

        return Ok(new { groups, skip, limit, total });
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserGroupById(int id)
    {
        var (group, mess) = await _service.GetUserGroupById(id);
        if (mess != null)
        {
            return _responseClients.GetStatusCode(mess.Status, mess);
        }

        return Ok(group);
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<IActionResult> AddUserGroup([FromBody] UserGroup userGroup)
    {
        var (group, mess) = await _service.CreateUserGroup(userGroup);
        if (mess != null)
        {
            return _responseClients.GetStatusCode(mess.Status, mess);
        }

        return Ok(group);
    }

    [AllowAnonymous]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUserGroup([FromBody] UserGroup userGroup, int id)
    {
        var (group, mess) = await _service.UpdateUserGroup(id, userGroup);

        if (mess != null)
        {
            return _responseClients.GetStatusCode(mess.Status, mess);
        }

        return Ok(group);
    }

    [AllowAnonymous]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUserGroup(int id)
    {
        var mess = await _service.DeleteUserGroup(id);
        if (mess != null)
        {
            return _responseClients.GetStatusCode(mess.Status, mess);
        }

        return Ok();
    }

    [AllowAnonymous]
    [HttpPost("{id:int}/users")]
    public async Task<IActionResult> AddUserToGroup([FromBody] List<int> userIds, int id)
    {
        var mess = await _service.AddUserToGroup(id, userIds);
        if (mess != null)
        {
            return _responseClients.GetStatusCode(mess.Status, mess);
        }

        return Ok();
    }

    [AllowAnonymous]
    [HttpDelete("{id:int}/users")]
    public async Task<IActionResult> RemoveUserToGroup([FromBody] List<int> userIds, int id)
    {
        var mess = await _service.RemoveUserFromGroup(id, userIds);
        if (mess != null)
        {
            return _responseClients.GetStatusCode(mess.Status, mess);
        }

        return Ok();
    }
}