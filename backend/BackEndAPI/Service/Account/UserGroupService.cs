using AutoMapper.Configuration;
using BackEndAPI.Data;
using BackEndAPI.Models.Account;
using BackEndAPI.Models.Other;
using Gridify;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities;

namespace BackEndAPI.Service.Account;

public class UserGroupService: IUserGroupService
{
    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor; 
    
    public UserGroupService(AppDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }
    public async Task<(IEnumerable<UserGroup>?, Mess?, int)> GetUserGroups(int skip, int limit, GridifyQuery q, string? search)
    {
        var mess = new Mess();
        try
        {
            var query = _context.UserGroup.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search)); 
            }
            var total = await query.ApplyFiltering(q).CountAsync();
            var groups = await query.ApplyFiltering(q). Skip(skip * limit).Take(limit).ToListAsync();
    
            return (groups, null, total);
        }
        catch (Exception ex)
        {
            mess.Status = 500;
            mess.Errors  = ex.Message;
            return (null, mess, 0);
        }   
    }
    
    public async Task<(UserGroup?, Mess?)> GetUserGroupById(int id)
    {
        var mess = new Mess();
        try
        {
            var group = await _context.UserGroup.Include(p => p.ListUsers).FirstOrDefaultAsync(p => p.Id == id);
            if (group == null)
            {
                mess.Status = 404;
                mess.Errors = "UserGroup not found";
    
                return (null, mess);
            }
    
            return (group, null);
        }
        catch (Exception ex)
        {
            mess.Errors = ex.Message;
            mess.Status = 500;
            return (null, mess);
        }
    }
    
    public async Task<Mess?> DeleteUserGroup(int id)
    {
        var mess = new Mess();
        try
        {
            var group = await _context.UserGroup.FindAsync(id);
            if (group == null)
            {
                mess.Status = 404;
                mess.Errors = "UserGroup not found";
    
                return mess;
            }
    
            _context.UserGroup.Remove(group);
            await _context.SaveChangesAsync();
    
            return null;
        }
        catch(Exception ex)
        {
            mess.Status = 500;
            mess.Errors = ex.Message;
            return mess;
        }
    }
    
    public async Task<(UserGroup?, Mess?)> CreateUserGroup(UserGroup userGroup)
    {
        var mess = new Mess();
        try
        {
            var context = _httpContextAccessor.HttpContext;
            // Lấy địa chỉ IP của client
            var ipAddress = context?.Connection.RemoteIpAddress?.ToString();

            // Lấy thông tin User-Agent
            var userAgent = context?.Request.Headers["User-Agent"].ToString();

            userGroup.IpAddress = ipAddress;
            userGroup.UserAgent =  userAgent;

            _context.UserGroup.Add(userGroup);
            await _context.SaveChangesAsync();
            return (userGroup, null);
        }
        catch (Exception ex)
        {
            mess.Status = 500;
            mess.Errors = ex.Message;
            return (null, mess);
        }
    }
    
    public async Task<(UserGroup?, Mess?)> UpdateUserGroup(int id, UserGroup userGroup)
    {
        var mess = new Mess();
        try
        {
            userGroup.UpdatedAt = DateTime.Now;
            _context.UserGroup.Update(userGroup);
            await _context.SaveChangesAsync();
            return (userGroup, null);
        }
        catch (Exception ex)
        {
            mess.Status = 500;
            mess.Errors = ex.Message;
            return (null, mess);
        }
    }
    
    public async Task<Mess?> AddUserToGroup(int id, List<int> userIds)
    {
        var mess = new Mess();
        try
        {
            var userGroup = await _context.UserGroup.Include(p => p.ListUsers).FirstOrDefaultAsync(p => p.Id == id);
            if (userGroup == null)
            {
                mess.Errors = "UserGroup not found";
                mess.Status = 404;
                return mess;
            }
            
            var userList = await _context.AppUser.Where(a => userIds.Contains(a.Id)).ToListAsync();
            foreach (var user in userList)
            {
               userGroup?.ListUsers?.Add(user); 
            }

            await _context.SaveChangesAsync();

            return null;
        }
        catch (Exception ex)
        {
            mess.Status = 500;
            mess.Errors = ex.Message;

            return mess;
        }
    }
    
    public async Task<Mess?> RemoveUserFromGroup(int id, List<int> userIds)
    {
        var mess = new Mess();
        try
        {
            var userGroup = await _context.UserGroup.Include(p => p.ListUsers).FirstOrDefaultAsync(p => p.Id == id);
            if (userGroup == null)
            {
                mess.Errors = "UserGroup not found";
                mess.Status = 404;
                return mess;
            }
            
            var listUserRemove = await _context.AppUser.Where(a => userIds.Contains(a.Id)).ToListAsync();

            foreach (var user in listUserRemove)
            {
               userGroup?.ListUsers?.Remove(user);   
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            mess.Status = 500;
            mess.Errors = ex.Message;
        }
        return null;
    }
}