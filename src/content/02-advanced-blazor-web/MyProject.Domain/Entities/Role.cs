using MyProject.Domain.Interfaces;

namespace MyProject.Domain.Entities;

/// <summary>
/// Represents a security role within the system.
/// </summary>
public class Role : IDbModel<int?>, IDbParameterizable
{
    /// <summary>
    /// Gets or sets the unique identifier for the role.
    /// </summary>
    public int RoleId { get; set; }

    /// <summary>
    /// Gets or sets the display name of the role.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the normalized form of <see cref="Name"/> used for comparisons.
    /// Typically this is an uppercase or otherwise normalized representation.
    /// </summary>
    public string NormalizedName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Active Directory group's Common Name (CN) associated with this role.
    /// Empty when no AD group is linked.
    /// </summary>
    public string ActiveDirectoryGroupCN { get; set; } = string.Empty;

    /// <inheritdoc />
    public int? GetIdentifier() => RoleId > 0 ? RoleId : null;

    /// <inheritdoc />
    public Dictionary<string, object?> GetParameters()
    {
        return new Dictionary<string, object?>
        {
            { "RoleId", RoleId },
            { "Name", Name  },
            { "NormalizedName", NormalizedName  },
            { "ActiveDirectoryGroupCN", ActiveDirectoryGroupCN  },
        };
    }
}
