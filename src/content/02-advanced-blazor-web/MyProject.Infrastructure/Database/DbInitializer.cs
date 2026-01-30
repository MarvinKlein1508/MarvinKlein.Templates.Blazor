using DbUp;

namespace MyProject.Infrastructure.Database;

/// <summary>
/// Initializes and migrates the SQL database using DbUp.
/// </summary>
public class DbInitializer
{
    private readonly string _connectionString;

    /// <summary>
    /// Creates a new instance of <see cref="DbInitializer"/>.
    /// </summary>
    /// <param name="connectionString">A valid SQL Server connection string pointing to the target database.</param>
    public DbInitializer(string connectionString)
    {
        _connectionString = connectionString;
    }

    /// <summary>
    /// Ensures the target database exists and applies any pending embedded migration scripts.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{Int32}"/> that completes with an exit code:
    /// 0 when the operation succeeded or no upgrade was required, and -1 when an upgrade failed.
    /// </returns>
    /// <remarks>
    /// This method uses DbUp to ensure the database exists and to execute scripts embedded
    /// in the assembly that contains <see cref="DbInitializer"/>. The method returns a
    /// <see cref="Task{Int32}"/> for API compatibility; the implementation executes
    /// synchronously and returns a completed task.
    /// 
    /// Exceptions thrown by DbUp or the underlying database provider will propagate to the caller.
    /// </remarks>
    public Task<int> InitializeAsync()
    {
        EnsureDatabase.For.SqlDatabase(_connectionString);

        var upgrader = DeployChanges.To.SqlDatabase(_connectionString)
            .WithScriptsEmbeddedInAssembly(typeof(DbInitializer).Assembly)
            .LogToConsole()
            .Build();

        if (upgrader.IsUpgradeRequired())
        {
            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                return Task.FromResult(-1);
            }
        }

        return Task.FromResult(0);
    }
}
