using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;
using Wasel_Palestine.BAL.Service;

namespace  Wasel_Palestine.BAL.Service
{
public class WeatherBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public WeatherBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var incidentService = scope.ServiceProvider.GetRequiredService<IIncidentService>();

            await incidentService.ProcessWeatherIncidentsAsync();

          
            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
        }
    }
}
}