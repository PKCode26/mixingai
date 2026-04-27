using MixingAI.Api.Core.Auth;
using MixingAI.Api.Core.Contracts;
using MixingAI.Api.Core.Security;
using MixingAI.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace MixingAI.Api.Core.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth");
        group.MapPost("/login", LoginAsync).RequireRateLimiting("auth");
        group.MapPost("/logout", LogoutAsync);
        group.MapGet("/me", MeAsync);
        return app;
    }

    private static async Task<IResult> LoginAsync(
        HttpContext httpContext,
        LoginRequest request,
        AppDbContext db,
        PasswordHashingService passwords,
        SessionTokenService tokens,
        CurrentUserService currentUserService,
        IWebHostEnvironment env,
        IConfiguration config,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.UsernameOrEmail) || string.IsNullOrWhiteSpace(request.Password))
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["credentials"] = ["Benutzername/E-Mail und Passwort sind erforderlich."]
            });

        var identifier = request.UsernameOrEmail.Trim().ToLowerInvariant();
        var user = await db.Users
            .SingleOrDefaultAsync(x => x.Username == identifier || x.Email == identifier, cancellationToken);

        if (user is null || !user.IsActive)
            return Results.Unauthorized();

        var utcNow = DateTime.UtcNow;
        if (!CurrentUserService.IsAllowedToSignIn(user, utcNow))
            return Results.Unauthorized();

        if (!passwords.VerifyPassword(user, request.Password))
            return Results.Unauthorized();

        user.RegisterLogin(utcNow);
        user.SetUpdated(user.Id, utcNow);

        var token = tokens.GenerateToken();
        var sessionDays = config.GetValue<int>("Auth:SessionDays", 7);
        var session = new AuthSession { UserId = user.Id };
        session.Initialize(tokens.HashToken(token), utcNow.AddDays(sessionDays), utcNow);

        db.AuthSessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);

        AppendCookie(httpContext, env, token, session.ExpiresAtUtc, config);

        return Results.Ok(new LoginResponse(token, session.ExpiresAtUtc, ToResponse(CurrentUserService.ToContext(user))));
    }

    private static async Task<IResult> LogoutAsync(
        HttpContext httpContext,
        AppDbContext db,
        SessionTokenService tokens,
        IWebHostEnvironment env,
        IConfiguration config,
        CancellationToken cancellationToken)
    {
        var token = CurrentUserService.TryReadSessionToken(httpContext);
        if (!string.IsNullOrWhiteSpace(token))
        {
            var hash = tokens.HashToken(token);
            var session = await db.AuthSessions
                .SingleOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);
            if (session?.RevokedAtUtc is null)
            {
                session?.Revoke(DateTime.UtcNow);
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        DeleteCookie(httpContext, env, config);
        return Results.NoContent();
    }

    private static async Task<IResult> MeAsync(
        HttpContext httpContext,
        AppDbContext db,
        CurrentUserService currentUserService,
        CancellationToken cancellationToken)
    {
        var ctx = await currentUserService.GetCurrentUserAsync(httpContext, db, cancellationToken);
        return ctx is null ? Results.Unauthorized() : Results.Ok(ToResponse(ctx));
    }

    private static AuthUserResponse ToResponse(RequestUserContext ctx) =>
        new(ctx.UserId, ctx.Username, ctx.Email, ctx.FirstName, ctx.LastName, ctx.IsAdmin);

    private static void AppendCookie(HttpContext httpContext, IWebHostEnvironment env, string token, DateTime expires, IConfiguration config)
    {
        var cookieName = config.GetValue<string>("Auth:CookieName") ?? CurrentUserService.SessionCookieName;
        httpContext.Response.Cookies.Append(cookieName, token, BuildCookieOptions(httpContext, env, expires));
    }

    private static void DeleteCookie(HttpContext httpContext, IWebHostEnvironment env, IConfiguration config)
    {
        var cookieName = config.GetValue<string>("Auth:CookieName") ?? CurrentUserService.SessionCookieName;
        httpContext.Response.Cookies.Delete(cookieName, BuildCookieOptions(httpContext, env, DateTime.UtcNow.AddDays(-1)));
    }

    private static CookieOptions BuildCookieOptions(HttpContext httpContext, IWebHostEnvironment env, DateTime expires) => new()
    {
        HttpOnly = true,
        IsEssential = true,
        SameSite = SameSiteMode.Lax,
        Secure = !env.IsDevelopment() || httpContext.Request.IsHttps,
        Expires = new DateTimeOffset(expires),
        Path = "/"
    };
}
