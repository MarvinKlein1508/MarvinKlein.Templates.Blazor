using System.Data;
using Dapper;
using MyProject.Domain.Entities;
using MyProject.Domain.Interfaces;

namespace MyProject.Infrastructure.Repositories;

/// <summary>
/// Repository for managing <see cref="User"/> entities.
/// Provides create, read and update operations and coordinates user role persistence
/// via the <see cref="UserRoleRepository"/>.
/// </summary>
public class UserRepository : IGetOperation<User, int?>, ICreateOperation<User>, IUpdateOperation<User>
{
    private readonly UserRoleRepository _userRoleRepository;

    /// <summary>
    /// Creates a new instance of <see cref="UserRepository"/>.
    /// </summary>
    /// <param name="userRoleRepository">Repository used to persist user roles.</param>
    public UserRepository(UserRoleRepository userRoleRepository)
    {
        _userRoleRepository = userRoleRepository;
    }

    /// <summary>
    /// Creates a new <see cref="User"/> in the database and persists associated roles.
    /// After insertion the generated <see cref="User.UserId"/> is assigned to <paramref name="input"/>.
    /// </summary>
    /// <param name="input">The user to create. Associated roles in <see cref="User.Roles"/> will also be created.</param>
    /// <param name="connection">Open database connection to use for the operation.</param>
    /// <param name="transaction">Optional transaction to enlist the inserts in.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous create operation.</returns>
    public async Task CreateAsync(User input, IDbConnection connection, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        string sql =
            """
            INSERT INTO Users
            (
                Username,
                DisplayName,
                ActiveDirectoryGuid,
                Email,
                Password,
                Salt,
                AccountType,
                IsActive,
                TwoFactorEnabled,
                LockoutEnd,
                LockoutEnabled,
                AccessFailedCount
            )
            VALUES
            (
                @Username,
                @DisplayName,
                @ActiveDirectoryGuid,
                @Email,
                @Password,
                @Salt,
                @AccountType,
                @IsActive,
                @TwoFactorEnabled,
                @LockoutEnd,
                @LockoutEnabled,
                @AccessFailedCount
            ); SELECT SCOPE_IDENTITY();
            """;

        var command = new CommandDefinition(commandText: sql, parameters: input.GetParameters(), transaction: transaction, cancellationToken: cancellationToken);

        input.UserId = await connection.ExecuteScalarAsync<int>(command);

        foreach (var role in input.Roles)
        {
            role.UserId = input.UserId;
            await _userRoleRepository.CreateAsync(role, connection, transaction, cancellationToken);
        }
    }

    /// <summary>
    /// Retrieves a <see cref="User"/> by its identifier.
    /// Also loads associated roles into <see cref="User.Roles"/>.
    /// </summary>
    /// <param name="identifier">The user id to retrieve. If <c>null</c> result will be <c>null</c>.</param>
    /// <param name="connection">Open database connection to use for the query.</param>
    /// <param name="transaction">Optional transaction to use for the query.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The matching <see cref="User"/> or <c>null</c> when no user exists with the provided id.</returns>
    public async Task<User?> GetAsync(int? identifier, IDbConnection connection, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        string sql = "SELECT * FROM Users WHERE UserId = @UserId";
        var result = await connection.QueryFirstOrDefaultAsync<User>(sql, new { UserId = identifier }, transaction);

        if (result is null)
        {
            return null;
        }

        result.Roles = await _userRoleRepository.GetAsync([result.UserId], connection, transaction, cancellationToken);

        return result;
    }

    /// <summary>
    /// Retrieves a <see cref="User"/> by its Active Directory GUID.
    /// Does not populate roles; use <see cref="GetAsync"/> for role loading.
    /// </summary>
    /// <param name="activeDirectoryGuid">Active Directory GUID to search for.</param>
    /// <param name="connection">Open database connection to use for the query.</param>
    /// <param name="transaction">Optional transaction to use for the query.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The matching <see cref="User"/> or <c>null</c> when no user exists with the provided GUID.</returns>
    public Task<User?> GetByActiveDirectoryGuidAsync(Guid activeDirectoryGuid, IDbConnection connection, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        string sql = "SELECT * FROM Users WHERE ActiveDirectoryGuid = @ActiveDirectoryGuid";
        return connection.QueryFirstOrDefaultAsync<User>(sql, new { ActiveDirectoryGuid = activeDirectoryGuid }, transaction);
    }

    /// <summary>
    /// Updates an existing <see cref="User"/> record and replaces its roles with the provided collection.
    /// </summary>
    /// <param name="input">The user containing updated values and the roles to persist.</param>
    /// <param name="connection">Open database connection to use for the update.</param>
    /// <param name="transaction">Optional transaction to enlist the update in.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous update operation.</returns>
    public async Task UpdateAsync(User input, IDbConnection connection, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        string sql =
            """
            UPDATE Users SET
                Username = @Username,
                DisplayName = @DisplayName,
                Email = @Email,
                AccessFailedCount = @AccessFailedCount,
                IsActive = @IsActive
            WHERE 
                UserId = @UserId
            """;

        var command = new CommandDefinition
        (
            commandText: sql,
            commandType: CommandType.Text,
            parameters: input.GetParameters(),
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        await connection.ExecuteAsync(command);

        await _userRoleRepository.DeleteByUserIdAsync(input.UserId, connection, transaction, cancellationToken);
        foreach (var role in input.Roles)
        {
            role.UserId = input.UserId;
            await _userRoleRepository.CreateAsync(role, connection, transaction, cancellationToken);
        }
    }

    /// <summary>
    /// Sets two-factor authentication values for the specified user.
    /// Only updates the TwoFactorEnabled and TwoFactorToken columns.
    /// </summary>
    /// <param name="input">The user containing the two-factor values to persist.</param>
    /// <param name="connection">Open database connection to use for the update.</param>
    /// <param name="transaction">Optional transaction to enlist the update in.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous update operation.</returns>
    public Task SetTwoFactorAuthenticationAsync(User input, IDbConnection connection, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        string sql =
            """
            UPDATE Users SET
                TwoFactorEnabled = @TwoFactorEnabled,
                TwoFactorToken = @TwoFactorToken
            WHERE 
                UserId = @UserId
            """;

        var command = new CommandDefinition
        (
            commandText: sql,
            commandType: CommandType.Text,
            parameters: input.GetParameters(),
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        return connection.ExecuteAsync(command);
    }

    /// <summary>
    /// Retrieves a <see cref="User"/> by username and loads associated roles into <see cref="User.Roles"/>.
    /// </summary>
    /// <param name="username">The username to search for.</param>
    /// <param name="connection">Open database connection to use for the query.</param>
    /// <param name="transaction">Optional transaction to use for the query.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The matching <see cref="User"/> or <c>null</c> when no user exists with the provided username.</returns>
    public async Task<User?> GetByUsernameAsync(string username, IDbConnection connection, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        string sql = "SELECT * FROM Users WHERE Username = @Username";
        var result = await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username }, transaction);

        if (result is null)
        {
            return null;
        }

        result.Roles = await _userRoleRepository.GetAsync([result.UserId], connection, transaction, cancellationToken);

        return result;
    }

    public Task ChangePasswordAsync(User input, IDbConnection connection, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        string sql =
            """
            UPDATE Users SET
                Password = @Password,
                Salt = @Salt
            WHERE 
                UserId = @UserId
            """;

        var command = new CommandDefinition
        (
            commandText: sql,
            commandType: CommandType.Text,
            parameters: input.GetParameters(),
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        return connection.ExecuteAsync(command);
    }
}
