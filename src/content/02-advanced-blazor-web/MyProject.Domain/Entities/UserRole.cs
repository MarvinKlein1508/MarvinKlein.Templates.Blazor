using MyProject.Domain.Interfaces;

namespace MyProject.Domain.Entities;

/// <summary>
/// Represents the assignment of a <see cref="Role"/> to a <see cref="User"/>.
/// </summary>
public sealed class UserRole : IDbParameterizable
{
    /// <summary>
    /// Gets or sets the identifier of the user.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the role.
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this user-role assignment is active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <inheritdoc />
    public Dictionary<string, object?> GetParameters()
    {
        return new Dictionary<string, object?>
        {
            { "UserId", UserId  },
            { "RoleId", RoleId  },
            { "IsActive", IsActive  },
        };
    }
}
