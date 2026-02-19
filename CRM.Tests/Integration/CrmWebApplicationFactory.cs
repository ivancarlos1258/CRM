using CRM.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CRM.Tests.Integration;

public class CrmWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<CrmDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<CrmDbContext>(options =>
            {
                options.UseInMemoryDatabase("CrmTestDb");
            });

            var sp = services.BuildServiceProvider();

            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CrmDbContext>();
            
            db.Database.EnsureCreated();
        });

        builder.UseEnvironment("Testing");
    }
}
