using MixingAI.Api.Core.Security;
using MixingAI.Api.Core.Users;
using MixingAI.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MixingAI.Api.Infrastructure.Seed;

public static class AdminSeedService
{
    public static async Task SeedAsync(AppDbContext db, PasswordHashingService passwords, CancellationToken cancellationToken = default)
    {
        await db.Database.MigrateAsync(cancellationToken);

        if (await db.Users.AnyAsync(u => u.Username == "admin", cancellationToken))
            return;

        var utcNow = DateTime.UtcNow;
        var admin = new User
        {
            Username = "admin"
        };
        admin.UpdateProfile("admin@mixingai.local", "Admin", "User");
        admin.ChangePassword(new PasswordHashingService().HashPassword(admin, PasswordHashingService.DevAdminPassword));
        admin.SetAdmin(true);
        admin.SetCreated(null, utcNow);

        db.Users.Add(admin);
        await db.SaveChangesAsync(cancellationToken);
    }
}
