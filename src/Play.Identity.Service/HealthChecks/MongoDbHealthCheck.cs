using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;

namespace Play.Identity.Service.HealthChecks;

public class MongoDbHealthCheck : IHealthCheck
{
    private readonly MongoClient _mongoClient;

    public MongoDbHealthCheck(MongoClient mongoClient)
    {
        _mongoClient = mongoClient;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _mongoClient.ListDatabasesAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (System.Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}