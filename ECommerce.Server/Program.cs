using ECommerce.Server;
using Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

var app = builder
    .ConfigureServices()
    .ConfigurePipeline();

// Seed admin user (non-blocking if database unavailable)
// This allows the app to start even if database is temporarily unavailable
await SeedData.SeedAdminUserAsync(app.Services);

app.Run();
