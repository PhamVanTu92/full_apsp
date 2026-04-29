using BackEndAPI.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace BackEndAPI.Service.Privile;

public class DatabaseAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, AuthorizationPolicy> _policies = new();

    public DatabaseAuthorizationPolicyProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        if (_policies.ContainsKey(policyName))
            return Task.FromResult(_policies[policyName]);

        // Nếu policy chưa có trong cache, lấy từ cơ sở dữ liệu
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var privilege = dbContext.Privileges.AsNoTracking().FirstOrDefault(p => p.Code == policyName);
            if (privilege == null)
                return Task.FromResult<AuthorizationPolicy>(null);

            var policy = new AuthorizationPolicyBuilder()
                .RequireClaim(policyName, privilege.Code) // Ví dụ yêu cầu claim "Permission" trùng với PolicyName
                .Build();

            _policies[policyName] = policy;
            return Task.FromResult(policy);
        }
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => Task.FromResult<AuthorizationPolicy>(null);
    public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => Task.FromResult<AuthorizationPolicy>(null);
}

