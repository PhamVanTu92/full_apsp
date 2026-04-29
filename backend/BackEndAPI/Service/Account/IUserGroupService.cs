using BackEndAPI.Models.Account;
using BackEndAPI.Models.Other;
using Gridify;

namespace BackEndAPI.Service.Account;

public interface IUserGroupService
{
    Task<(IEnumerable<UserGroup>?, Mess?, int)> GetUserGroups(int skip, int limit, GridifyQuery q, string? search);
    Task<(UserGroup?, Mess?)> GetUserGroupById(int id);
    Task<Mess?> DeleteUserGroup(int id);
    Task<(UserGroup?, Mess?)> CreateUserGroup(UserGroup userGroup);
    Task<(UserGroup?, Mess?)> UpdateUserGroup(int id, UserGroup userGroup);
    Task<Mess?> AddUserToGroup(int id, List<int> userIds);
    Task<Mess?> RemoveUserFromGroup(int id, List<int> userIds);
}