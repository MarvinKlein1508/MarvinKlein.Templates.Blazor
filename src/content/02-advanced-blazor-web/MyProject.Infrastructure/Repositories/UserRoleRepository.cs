using System.Data;
using Dapper;
using MyProject.Domain.Entities;
using MyProject.Domain.Interfaces;

namespace MyProject.Infrastructure.Repositories;

/// <summary>
/// Repository for <see cref="UserRole"/> data access operations.
/// </summary>
public sealed class UserRoleRepository : ICreateOperation<UserRole>, IGetMultipleOperation<UserRole, int>
{
    /// <inheritdoc />
    public Task CreateAsync(UserRole input, IDbConnection connection, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        string sql =
            """
            INSERT INTO UserRoles
            (
                UserId,
                RoleId,
                IsActive
            )
            VALUES
            (
                @UserId,
                @RoleId,
                @IsActive
            )
            """;

        var command = new CommandDefinition(commandText: sql, parameters: input.GetParameters(), transaction: transaction, cancellationToken: cancellationToken);

        return connection.ExecuteAsync(command);
    }

    /// <inheritdoc />
    public Task<IEnumerable<UserRole>> GetAsync(IEnumerable<int> identifiers, IDbConnection connection, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        if (!identifiers.Any())
        {
            return Task.FromResult(Enumerable.Empty<UserRole>());
        }

        string sql =
            $"""
            SELECT
                ui.UserId,
                r.RoleId,
                coalesce(ur.IsActive, 0) as IsActive
            FROM Roles r 
            LEFT JOIN (VALUES ({string.Join("),(", identifiers)})) AS ui (UserId) ON 1 = 1
            LEFT JOIN UserRoles ur ON (ur.UserId = ui.UserId AND ur.RoleId = r.RoleId)
            """;

        var command = new CommandDefinition(commandText: sql, parameters: null, transaction: transaction, cancellationToken: default);

        return connection.QueryAsync<UserRole>(command);
    }

    /// <summary>
    /// Deletes all role assignments for the specified user.
    /// </summary>
    /// <param name="userId">The id of the user whose role assignments should be removed.</param>
    /// <param name="connection">An open <see cref="IDbConnection"/> used to execute the command.</param>
    /// <param name="transaction">Optional <see cref="IDbTransaction"/> to participate in.</param>
    /// <param name="token">Cancellation token for the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous delete operation.</returns>
    public Task DeleteByUserIdAsync(int userId, IDbConnection connection, IDbTransaction? transaction = null, CancellationToken token = default)
    {
        string sql = "DELETE FROM UserRoles WHERE UserId = @UserId";

        var command = new CommandDefinition(commandText: sql, parameters: new { UserId = userId }, transaction: transaction, cancellationToken: token);

        return connection.ExecuteAsync(command);
    }
}
