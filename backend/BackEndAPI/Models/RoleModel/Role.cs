namespace BackEndAPI.Models.RoleModel;
public class Module
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Permission> Permissions { get; set; }
}

public class Permission
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int ModuleId { get; set; }
    public Module Module { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; }
}

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; }
}

public class RolePermission
{
    public int RoleId { get; set; }
    public Role Role { get; set; }
    public int PermissionId { get; set; }
    public Permission Permission { get; set; }
}
