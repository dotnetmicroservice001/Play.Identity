using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Play.Identity.Service.Entities;
using Play.Identity.Service.Settings;

namespace Play.Identity.Service.HostedServices;

public class IdentitySeedHostedService : IHostedService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IdentitySettings _settings;

    public IdentitySeedHostedService(
        IServiceScopeFactory serviceScopeFactory, 
        IOptions<IdentitySettings> identityOptions
        )
    {
        _serviceScopeFactory = serviceScopeFactory;
        _settings = identityOptions.Value;
    }
    
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }

    private static async Task CreateRoleIfNotExistsAsync(string role, 
        RoleManager<ApplicationRole> roleManager)
    {
        var roleExists = await roleManager.RoleExistsAsync(role);
        if (!roleExists)
        {
            await roleManager.CreateAsync( new ApplicationRole { Name = role } );
        }
    }
    
}