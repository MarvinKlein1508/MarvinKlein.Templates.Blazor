using System.Data;

namespace MyProject.Infrastructure.Database;

/// <summary>
/// Creates database connections for data-access components.
/// Implementations are responsible for providing configured and open
/// <see cref="IDbConnection"/> instances; callers are responsible for disposing them.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Asynchronously creates and returns an <see cref="IDbConnection"/>.
    /// Implementations should open the connection before returning it.
    /// </summary>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to monitor for cancellation requests.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result is an opened
    /// <see cref="IDbConnection"/> instance. The caller is responsible for disposing the returned connection.
    /// </returns>
    /// <remarks>
    /// Implementations may throw database-specific exceptions (for example, <see cref="System.Data.Common.DbException"/>)
    /// if the connection cannot be created or opened.
    /// </remarks>
    Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default);
}
