using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace BackEndAPI.Service.Privile;

public class PrivilegeAuthorizationHandler(IPrivileService privilegeService)
    : AuthorizationHandler<PrivilegeRequirement>
{
    protected override async Task<Task> HandleRequirementAsync(AuthorizationHandlerContext context,
        PrivilegeRequirement requirement)
    {
        var userId = int.Parse(context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? "0");
        

        if (await privilegeService.UserHasPrivilegeAsync(userId, requirement.PrivilegeName))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

public class PrivilegeRequirement : AuthorizeAttribute, IAuthorizationRequirement, IAuthorizationRequirementData
{
    public string PrivilegeName { get; }

    public PrivilegeRequirement(string privilegeName)
    {
        PrivilegeName = privilegeName;
    }
    public IEnumerable<IAuthorizationRequirement> GetRequirements()
    {
        yield return this;
    }
}