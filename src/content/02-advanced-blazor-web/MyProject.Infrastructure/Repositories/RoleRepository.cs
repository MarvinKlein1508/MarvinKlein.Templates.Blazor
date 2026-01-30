using System.Data;
using Dapper;
using MyProject.Domain.Entities;
using MyProject.Domain.Interfaces;

namespace MyProject.Infrastructure.Repositories;

public sealed class RoleRepository : IGetOperation<Role, int?>, IGetAllOperation<Role>
{
    /// <inheritdoc />
    public static Task<IEnumerable<Role>> GetAllAsync(IDbConnection connection, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        string sql = "SELECT * FROM Roles";

        var command = new CommandDefinition
        (
            commandText: sql,
            commandType: CommandType.Text,
            parameters: null,
            transaction: transaction,
            cancellationToken: cancellationToken
        );

        return connection.QueryAsync<Role>(command);
    }

    /// <inheritdoc />
    public Task<Role?> GetAsync(int? identifier, IDbConnection connection, IDbTransaction? transaction = null, CancellationToken cancellationToken = default)
    {
        string sql = "SELECT * FROM Roles WHERE RoleId = @RoleId";
        return connection.QueryFirstOrDefaultAsync<Role>(sql, new { RoleId = identifier }, transaction);
    }
}
