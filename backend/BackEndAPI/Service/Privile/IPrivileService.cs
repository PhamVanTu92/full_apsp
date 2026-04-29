namespace BackEndAPI.Service.Privile;

public interface IPrivileService
{
    Task<bool> UserHasPrivilegeAsync(int userId, string privilegeName);
}