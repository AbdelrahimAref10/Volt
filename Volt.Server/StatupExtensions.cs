using Application;
using Infrastructure;

namespace Volt.Server
{
    public static class StatupExtensions
    {
        public static WebApplication ConfigureServices (this WebApplicationBuilder builder)
        {
            builder.Services.AddApplicationServices();
            builder.Services.AddDatabaseServices(builder.Configuration);
            builder.Services.AddControllers();
            // Configure CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy => policy
                        .AllowAnyOrigin() // Allow any origin
                        .AllowAnyMethod() // Allow any method (GET, POST, etc.)
                        .AllowAnyHeader()); // Allow any header
            });
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddOpenApiDocument(document =>
            {
                document.Title = "Vot";
            });
            return builder.Build();
        }

        public static WebApplication ConfigurePipeline (this WebApplication app)
        {
            // Global exception handling must be first
            app.UseMiddleware<Presentation.Middleware.GlobalExceptionHandlingMiddleware>();
            
            app.UseCors("AllowAll");
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseSwaggerUi();
            app.UseOpenApi();
            app.UseRouting();
            app.UseHttpsRedirection();
            
            // JWT Authentication
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapFallbackToFile("/index.html");
            return app;
        }

    }
}
