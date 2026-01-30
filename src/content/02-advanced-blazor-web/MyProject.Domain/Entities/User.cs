using MyProject.Domain.Interfaces;

namespace MyProject.Domain.Entities;

/// <summary>
/// Represents a user account in the application.
/// </summary>
public class User : IDbParameterizable
{
    /// <summary>
    /// Primary key identifier for the user.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Unique username used for authentication.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Human-friendly display name for the user.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Email address for the user.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Optional GUID from Active Directory when the account is backed by AD.
    /// </summary>
    public Guid? ActiveDirectoryGuid { get; set; }

    /// <summary>
    /// Hashed password for internal accounts.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Salt used when hashing the password.
    /// </summary>
    public string Salt { get; set; } = string.Empty;

    /// <summary>
    /// Indicates whether the account is an internal application account or an Active Directory account.
    /// </summary>
    public AccountType AccountType { get; set; } = AccountType.Internal;

    /// <summary>
    /// True when the account is active; false when disabled.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// True when two-factor authentication is enabled for the account.
    /// </summary>
    public bool TwoFactorEnabled { get; set; }

    /// <summary>
    /// If set, the time until which the user is locked out from signing in.
    /// </summary>
    public DateTimeOffset? LockoutEnd { get; set; }

    /// <summary>
    /// True when lockout is enabled for the user (failed attempts can lock the account).
    /// </summary>
    public bool LockoutEnabled { get; set; }

    /// <summary>
    /// Number of consecutive failed access attempts.
    /// </summary>
    public int AccessFailedCount { get; set; }

    /// <summary>
    /// Token used for two-factor authentication flows (if applicable).
    /// </summary>
    public string? TwoFactorToken { get; set; }

    /// <summary>
    /// Roles assigned to the user. Typically used for authorization.
    /// </summary>
    public IEnumerable<UserRole> Roles { get; set; } = [];

    /// <inheritdoc />
    public Dictionary<string, object?> GetParameters()
    {
        return new Dictionary<string, object?>
        {
            { "UserId", UserId  },
            { "Username", Username  },
            { "DisplayName", DisplayName  },
            { "Email", Email  },
            { "ActiveDirectoryGuid", ActiveDirectoryGuid  },
            { "Password", Password  },
            { "Salt", Salt  },
            { "AccountType", (int)AccountType  },
            { "IsActive", IsActive  },
            { "TwoFactorEnabled", TwoFactorEnabled  },
            { "LockoutEnd", LockoutEnd  },
            { "LockoutEnabled", LockoutEnabled  },
            { "AccessFailedCount", AccessFailedCount  },
            { "TwoFactorToken", TwoFactorToken }
        };
    }
}

/// <summary>
/// Type of the user account indicating how authentication is performed.
/// </summary>
public enum AccountType
{
    /// <summary>
    /// Internal application-managed account (username/password stored in the app).
    /// </summary>
    Internal = 1,

    /// <summary>
    /// Account authenticated via Active Directory / external identity provider.
    /// </summary>
    ActiveDirectory = 2
}
