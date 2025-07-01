using Azure.Monitor.OpenTelemetry.AspNetCore;

await RunWebApp.Run(
  args,
  builder =>
  {
    builder.Services.AddOpenTelemetry().UseAzureMonitor();
    builder.Services.AddSingleton<RunningDocker>();
  });

