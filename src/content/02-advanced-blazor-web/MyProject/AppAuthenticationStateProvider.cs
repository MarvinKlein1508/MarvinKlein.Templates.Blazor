using System.Globalization;
using System.Security.Claims;
using MyProject.Domain.Entities;
using MyProject.Infrastructure.Database;
using MyProject.Infrastructure.Repositories;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace MyProject;

/// <summary>
/// Server-side authentication state provider that revalidates the current user's authentication
/// on a regular interval against the application's user store.
/// </summary>
public sealed class AppAuthenticationStateProvider : RevalidatingServerAuthenticationStateProvider
{
    private readonly IDbConnectionFactory _dbFactory;
    private readonly UserRepository _userRepository;

    private Task<AuthenticationState>? _initialStateTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="AppAuthenticationStateProvider"/> class.
    /// </summary>
    /// <param name="dbFactory">Factory used to obtain database connections for user lookups.</param>
    /// <param name="userRepository">Repository used to query users.</param>
    /// <param name="loggerFactory">Logger factory passed to the base revalidation provider.</param>
    public AppAuthenticationStateProvider(IDbConnectionFactory dbFactory, UserRepository userRepository, ILoggerFactory loggerFactory) : base(loggerFactory)
    {
        _dbFactory = dbFactory;
        _userRepository = userRepository;
    }

    /// <inheritdoc />
    protected override TimeSpan RevalidationInterval => TimeSpan.FromSeconds(5);

    /// <inheritdoc />
    protected override async Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        Console.WriteLine("Authentification validation");

        Claim? claim = authenticationState.User.FindFirst("userId");

        if (claim is null)
        {
            return false;
        }

        var userId = Convert.ToInt32(claim.Value, CultureInfo.InvariantCulture);
        using var connection = await _dbFactory.CreateConnectionAsync(cancellationToken);
        User? result = await _userRepository.GetAsync(userId, connection, cancellationToken: cancellationToken);

        if (result is null)
        {
            // User has been deleted from the database
            return false;
        }

        if (!result.IsActive)
        {
            // User has been deactivated by an admin
            return false;
        }

        return true;
    }

    /// <inheritdoc />
    public override Task<AuthenticationState> GetAuthenticationStateAsync() => _initialStateTask ??= GetInitialAuthenticationStateAsync();

    /// <summary>
    /// Retrieves the initial authentication state and verifies the corresponding database user is present and active.
    /// If the principal does not contain a valid user id claim, or the user is missing/inactive in the DB,
    /// an anonymous <see cref="ClaimsPrincipal"/> is returned.
    /// </summary>
    /// <returns>The validated initial <see cref="AuthenticationState"/> or an anonymous state when invalid.</returns>
    private async Task<AuthenticationState> GetInitialAuthenticationStateAsync()
    {
        AuthenticationState authState = await base.GetAuthenticationStateAsync();
        ClaimsPrincipal user = authState.User;

        var claim = user.FindFirst("userId");
        if (claim is null || !int.TryParse(claim.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var userId))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        using var connection = await _dbFactory.CreateConnectionAsync();
        var dbUser = await _userRepository.GetAsync(userId, connection);

        if (dbUser is null || !dbUser.IsActive)
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        return authState;
    }
}
