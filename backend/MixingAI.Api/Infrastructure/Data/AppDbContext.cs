using Microsoft.EntityFrameworkCore;

namespace MixingAI.Api.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("app_core");
        base.OnModelCreating(modelBuilder);
    }
}
