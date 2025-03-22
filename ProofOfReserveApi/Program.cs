using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using ProofOfReserveApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Register your controllers
builder.Services.AddControllers();

// Register the ProofOfReserveService as a singleton.
builder.Services.AddSingleton<ProofOfReserveService>();

builder.Services.AddHostedService<DailyRefreshWorker>();

var app = builder.Build();

// Map controllers
app.MapControllers();

// === Do an initial refresh with sample data ===
using (var scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider.GetRequiredService<ProofOfReserveService>();
    // For now, feed it your in-memory test data. In production you’d load from the real DB
    var initialData = new Dictionary<int, int>
    {
        {1, 1111}, {2, 2222}, {3, 3333}, {4, 4444},
        {5, 5555}, {6, 6666}, {7, 7777}, {8, 8888}
    };
    service.RefreshData(initialData);
}

app.Run();
