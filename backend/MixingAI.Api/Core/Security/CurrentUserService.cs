using MixingAI.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MixingAI.Api.Core.Security;

public sealed class CurrentUserService(SessionTokenService sessionTokenService)
{
    public const string SessionCookieName = "mixingai_auth";

    public static bool IsAllowedToSignIn(Users.User user, DateTime utcNow) =>
        user.IsActive && (user.LockedUntilUtc is null || user.LockedUntilUtc <= utcNow);

    public async Task<RequestUserContext?> GetCurrentUserAsync(
        HttpContext httpContext,
        AppDbContext db,
        CancellationToken cancellationToken)
    {
        var token = TryReadSessionToken(httpContext);
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var tokenHash = sessionTokenService.HashToken(token);
        var utcNow = DateTime.UtcNow;

        var session = await db.AuthSessions
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        if (session is null || !session.IsActive(utcNow))
            return null;

        var user = await db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == session.UserId, cancellationToken);

        if (user is null || !IsAllowedToSignIn(user, utcNow))
            return null;

        return ToContext(user);
    }

    public static RequestUserContext ToContext(Users.User user) =>
        new(user.Id, user.Username, user.Email, user.FirstName, user.LastName, user.IsAdmin, user.IsActive);

    public static string? TryReadSessionToken(HttpContext httpContext)
    {
        if (httpContext.Request.Cookies.TryGetValue(SessionCookieName, out var cookie)
            && !string.IsNullOrWhiteSpace(cookie))
            return cookie.Trim();

        var header = httpContext.Request.Headers.Authorization.ToString();
        const string prefix = "Bearer ";
        return header.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? header[prefix.Length..].Trim()
            : null;
    }
}
