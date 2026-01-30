namespace MyProject.Domain.Interfaces;

/// <summary>
/// Represents a domain model that exposes a stable identifier used by persistence and identity operations.
/// </summary>
/// <typeparam name="TIdentifier">
/// The type used for the model's identifier (for example <c>int</c>, <c>Guid</c> or <c>string</c>).
/// </typeparam>
public interface IDbModel<TIdentifier>
{
    /// <summary>
    /// Gets the identifier for this model instance.
    /// </summary>
    /// <returns>
    /// The identifier value of type <typeparamref name="TIdentifier"/>.
    /// Implementations should return a stable identifier used for persistence and equality checks.
    /// </returns>
    /// <remarks>
    /// Implementations may return the default value of <typeparamref name="TIdentifier"/> for transient/new instances
    /// that have not yet been persisted. This method should not change object state.
    /// </remarks>
    TIdentifier GetIdentifier();
}
