using System.Data;
using MyProject.Domain.Entities;
using MyProject.Domain.Interfaces;
using MyProject.Infrastructure.Database;
using MyProject.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;

namespace MyProject.Infrastructure;

/// <summary>
/// Simple in-memory storage cache used by the application.
/// <para>
/// Entries are stored in a dictionary keyed by the stored item's <see cref="System.Type"/>.
/// </para>
/// </summary>
public static class Storage
{
    /// <summary>
    /// Configuration instance provided during <see cref="InitAsync(IConfiguration)"/>.
    /// May be null before initialization.
    /// </summary>
    private static IConfiguration? _configuration;

    /// <summary>
    /// Internal storage mapping a CLR <see cref="Type"/> to an object containing the stored values (usually a <see cref="List{T}"/>).
    /// </summary>
    private static readonly Dictionary<Type, object> _storage = [];

    /// <summary>
    /// Initializes the storage by loading required data from the database.
    /// </summary>
    /// <param name="configuration">The application's configuration. Must contain a connection string named "Default".</param>
    /// <returns>A task that completes when initialization is finished.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the "Default" connection string is not found in <paramref name="configuration"/>.</exception>
    public static async Task InitAsync(IConfiguration configuration)
    {
        _configuration = configuration;
        string connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException(nameof(connectionString));

        var dbFactory = new SqlConnectionFactory(connectionString);
        using var connection = await dbFactory.CreateConnectionAsync();

        _storage.Add(typeof(Role), await RoleRepository.GetAllAsync(connection));
    }

    /// <summary>
    /// Adds or updates an item in the in-memory storage for the given element type.
    /// </summary>
    /// <typeparam name="T">The element type stored in the collection. Must implement <see cref="IDbModel{TIdentifier}"/>.</typeparam>
    /// <typeparam name="TIdentifier">The type used to identify the model (e.g. int, Guid, string).</typeparam>
    /// <param name="input">The item to add or update.</param>
    public static void UpdateStorage<T, TIdentifier>(T input) where T : class, IDbModel<TIdentifier>
    {
        if (!_storage.ContainsKey(typeof(T)))
        {
            return;
        }

        if (_storage[typeof(T)] is not List<T> list)
        {
            return;
        }

        var existingItem = list.FirstOrDefault(x => x?.GetIdentifier()?.Equals(input.GetIdentifier()) ?? false);

        if (existingItem == null)
        {
            list.Add(input);
        }
        else
        {
            int index = list.IndexOf(existingItem);
            list[index] = input;
        }
    }

    /// <summary>
    /// Removes an item from the in-memory storage collection that matches the provided model's identifier.
    /// </summary>
    /// <typeparam name="T">The element type stored in the collection. Must implement <see cref="IDbModel{TIdentifier}"/>.</typeparam>
    /// <typeparam name="TIdentifier">The identifier type.</typeparam>
    /// <param name="input">The item whose identifier will be used to find and remove the stored entry.</param>
    public static void DeleteFromStorage<T, TIdentifier>(T input) where T : class, IDbModel<TIdentifier>
    {
        var storage = _storage.GetValueOrDefault(typeof(T)) as List<T>;

        var item = storage?.Cast<T>().FirstOrDefault(x => x.GetIdentifier()?.Equals(input.GetIdentifier()) ?? false);

        if (item is not null)
        {
            storage!.Remove(item);
        }
    }

    /// <summary>
    /// Enumerates items of the specified type from the in-memory storage.
    /// </summary>
    /// <typeparam name="T">The element type to enumerate.</typeparam>
    /// <returns>An enumerable sequence of items of type <typeparamref name="T"/>. If no entry exists for the type, the sequence is empty.</returns>
    public static IEnumerable<T> Get<T>() where T : class
    {
        if (_storage.ContainsKey(typeof(T)))
        {
            var storage = _storage.GetValueOrDefault(typeof(T)) as List<T> ?? [];

            foreach (var item in storage)
            {
                yield return item;
            }
        }
    }

    /// <summary>
    /// Retrieves a single item by its identifier from the in-memory storage.
    /// </summary>
    /// <typeparam name="T">The element type stored in the collection. Must implement <see cref="IDbModel{TIdentifier}"/>.</typeparam>
    /// <typeparam name="TIdentifier">The identifier type.</typeparam>
    /// <param name="identifier">Identifier to search for. If <c>null</c>, the method returns <c>null</c>.</param>
    /// <returns>The matching item if found; otherwise <c>null</c>.</returns>
    public static T? Get<T, TIdentifier>(TIdentifier identifier) where T : class, IDbModel<TIdentifier>
    {
        if (identifier is null)
        {
            return null;
        }

        foreach (var item in Get<T>())
        {
            if (item.GetIdentifier()?.Equals(identifier) ?? false)
            {
                return item;
            }
        }

        return null;
    }
}
