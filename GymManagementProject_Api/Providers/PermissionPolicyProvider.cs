using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
        _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() =>
        _fallbackPolicyProvider.GetFallbackPolicyAsync();

    public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // PolicyName chính là permission code
        var policy = new AuthorizationPolicyBuilder()
            .RequireClaim("Permission", policyName)
            .Build();

        return await Task.FromResult(policy);
    }
}
