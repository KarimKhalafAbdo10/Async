using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Products.Application.Abstractions;
using Products.Infrastructure.Persistence;
using Products.Infrastructure.Repositories;

namespace Products.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<AppDbContext>(o =>
        {
            // Local convenience: run without SQL Server by setting "UseInMemoryDatabase": true
            if (bool.TryParse(config["UseInMemoryDatabase"], out var inMemory) && inMemory)
                o.UseInMemoryDatabase("ProductsDb");
            else
                o.UseSqlServer(config.GetConnectionString("DefaultConnection"));
        });
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        return services;
    }
}
