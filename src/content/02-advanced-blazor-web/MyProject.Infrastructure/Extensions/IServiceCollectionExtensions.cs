using MyProject.Infrastructure.Auth;
using MyProject.Infrastructure.Database;
using MyProject.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MyProject.Infrastructure.Extensions;

/// <summary>
/// Extension methods for registering infrastructure services into an <see cref="IServiceCollection"/>.
/// </summary>
public static class IServiceCollectionExtensions
{
    /// <summary>
    /// Registers application-level services into the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="config">The application <see cref="IConfiguration"/> used to configure services.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    /// <remarks>
    /// Add application-specific services (authentication, repositories, feature services, etc.) here.
    /// This method currently acts as a placeholder and returns the collection unchanged.
    /// </remarks>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        services.AddSingleton<UserRepository>();
        services.AddSingleton<RoleRepository>();
        services.AddSingleton<UserRoleRepository>();
        services.AddScoped<LoginService>();
        services.AddScoped<LdapProvider>();
        services.AddScoped<AuthService>();
        services.AddScoped<RedirectManager>();

        // Register options
        services.AddOptions<LdapOptions>()
            .Bind(config.GetRequiredSection(LdapOptions.SectionName));

        return services;
    }

    /// <summary>
    /// Registers database-related services into the provided <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="connectionString">The SQL Server connection string used by the database services.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    /// <remarks>
    /// This registers:
    /// - An <see cref="IDbConnectionFactory"/> as a singleton that creates opened SQL Server connections.
    /// - A <see cref="DbInitializer"/> as a singleton to handle database initialization tasks.
    ///
    /// Caller responsibilities:
    /// - The registered <see cref="IDbConnectionFactory"/> produces open <see cref="System.Data.IDbConnection"/>
    ///   instances; callers should dispose those connections when finished.
    ///
    /// Lifetime notes:
    /// - Both services are registered as singletons to share configuration and initialization behavior.
    ///   If per-request or scoped connections are desired, consider changing the lifetimes accordingly.
    /// </remarks>
    public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddSingleton<IDbConnectionFactory>(_ => new SqlConnectionFactory(connectionString));
        services.AddSingleton(_ => new DbInitializer(connectionString));
        return services;
    }
}
