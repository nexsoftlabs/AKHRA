using MoviePlatform.Infrastructure;
using MoviePlatform.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHostedService<EncodingJobWorker>();
builder.Services.AddHostedService<ReconciliationWorker>();

var host = builder.Build();
host.Run();
