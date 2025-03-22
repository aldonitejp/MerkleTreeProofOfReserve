using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Hosting;
using ProofOfReserveApi.Services;

/// <summary>
/// A background worker that periodically (every 24 hours) refreshes the user data
/// from a hypothetical data source, then updates the <see cref="ProofOfReserveService"/>.
/// </summary>
public class DailyRefreshWorker : BackgroundService
{
    private readonly ProofOfReserveService _service;

    /// <summary>
    /// Constructs the <see cref="DailyRefreshWorker"/>, which depends on a
    /// <see cref="ProofOfReserveService"/> instance for updating the daily snapshot of user data.
    /// </summary>
    /// <param name="service">
    /// The proof-of-reserve service that will receive fresh user data once per day.
    /// </param>
    public DailyRefreshWorker(ProofOfReserveService service)
    {
        _service = service;
    }

    /// <summary>
    /// The main execution loop of the background service. Waits for 24 hours, then
    /// simulates fetching new data (e.g. from a real database), updates the service,
    /// and repeats until cancellation is requested.
    /// </summary>
    /// <param name="stoppingToken">
    /// A <see cref="CancellationToken"/> that indicates the operation should stop.
    /// </param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Just a naive loop: every 24 hours, refresh data
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);

            //TODO: replace this placeholder with actual implementation
            // that loads from the database, instead of hardcoded values
            // Load from DB (example placeholder):
            var newData = new Dictionary<int, int>()
            {
                {1, 1234}, {2, 5678}, // etc.
            };

            _service.RefreshData(newData);
        }
    }
}
