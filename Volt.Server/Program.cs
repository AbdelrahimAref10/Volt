using Volt.Server;
using Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

var app = builder
    .ConfigureServices()
    .ConfigurePipeline();


await SeedData.SeedAdminUserAsync(app.Services);

app.Run();
