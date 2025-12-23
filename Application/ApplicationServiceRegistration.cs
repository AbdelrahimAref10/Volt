using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Application
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));

            // Register validators (all validators must be registered here)
            services.AddScoped<Features.Customer.Command.AdminCreateCustomerCommand.AdminCreateCustomerCommandValidator>();
            services.AddScoped<Features.Customer.Command.RegisterCustomerCommand.RegisterCustomerCommandValidator>();
            services.AddScoped<Features.Customer.Command.UpdateCustomerCommand.UpdateCustomerCommandValidator>();
            services.AddScoped<Features.Customer.Command.ActivateCustomerCommand.ActivateCustomerCommandValidator>();
            services.AddScoped<Features.Customer.Command.CustomerLoginCommand.CustomerLoginCommandValidator>();
            services.AddScoped<Features.City.Command.AddCityCommand.AddCityCommandValidator>();
            services.AddScoped<Features.City.Command.UpdateCityCommand.UpdateCityCommandValidator>();
            services.AddScoped<Features.Admin.Command.AdminLoginCommand.AdminLoginCommandValidator>();
            services.AddScoped<Features.Category.Command.CreateCategoryCommand.CreateCategoryCommandValidator>();
            services.AddScoped<Features.Category.Command.UpdateCategoryCommand.UpdateCategoryCommandValidator>();

            return services;
        }
    }
}
