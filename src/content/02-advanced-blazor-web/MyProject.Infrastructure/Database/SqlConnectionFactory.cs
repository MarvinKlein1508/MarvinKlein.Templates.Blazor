using System.Data;
using Microsoft.Data.SqlClient;

namespace MyProject.Infrastructure.Database;

/// <summary>
/// Factory that creates and opens <see cref="IDbConnection"/> instances using a SQL Server connection string.
/// </summary>
public class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    /// <summary>
    /// Initializes a new instance of the <see cref="SqlConnectionFactory"/> class.
    /// </summary>
    /// <param name="connectionString">The SQL Server connection string used to create connections.</param>
    public SqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Creates a new <see cref="SqlConnection"/>, opens it asynchronously, and returns it as an <see cref="IDbConnection"/>.
    /// </summary>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting for the connection to open.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an open <see cref="IDbConnection"/>.
    /// </returns>
    /// <remarks>
    /// The returned connection is already opened. Callers are responsible for disposing the connection when finished.
    /// This method may throw exceptions thrown by <see cref="SqlConnection.OpenAsync(System.Threading.CancellationToken)"/>,
    /// such as <see cref="SqlException"/> or <see cref="InvalidOperationException"/>, if the connection cannot be opened.
    /// </remarks>
    public async Task<IDbConnection> CreateConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
