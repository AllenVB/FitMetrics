using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FitMetrics.Infrastructure.Persistence;

/// <summary>
/// Design-time factory: "dotnet ef migrations" komutlarının API host'una ihtiyaç
/// duymadan AppDbContext üretmesini sağlar. Yalnızca migration üretiminde kullanılır.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        const string connectionString =
            "Server=(localdb)\\MSSQLLocalDB;Database=FitMetricsDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
