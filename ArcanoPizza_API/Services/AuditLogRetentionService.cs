using ArcanoPizza_API.Data;
using ArcanoPizza_API.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ArcanoPizza_API.Services;

public sealed class AuditLogRetentionService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<AuditLogRetentionService> _logger;
    private readonly AuditLogRetentionOptions _options;

    public AuditLogRetentionService(
        IServiceProvider services,
        ILogger<AuditLogRetentionService> logger,
        IOptions<AuditLogRetentionOptions> options)
    {
        _services = services;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Fallo ejecutando retención de audit logs.");
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task RunOnceAsync(CancellationToken cancellationToken)
    {
        var days = Math.Clamp(_options.Days, 1, 3650);
        var cutoff = DateTime.UtcNow.AddDays(-days);

        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ArcanoPizzaDbContext>();

        var deleted = await db.AuditLogs
            .Where(x => x.OcurrioEn < cutoff)
            .ExecuteDeleteAsync(cancellationToken);

        if (deleted > 0)
        {
            _logger.LogInformation("Retención audit logs: eliminadas {Deleted} filas (cutoff={CutoffUtc}).", deleted, cutoff);
        }
    }
}

