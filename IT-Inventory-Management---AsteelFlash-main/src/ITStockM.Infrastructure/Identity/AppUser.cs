using Microsoft.AspNetCore.Identity;

namespace ITStockM.Infrastructure.Identity
{
    /// <summary>
    /// Application user identity model extending ASP.NET Core Identity.
    /// Manages authentication and authorization for IT Inventory Management system.
    /// 
    /// Replaces the plain-text password storage previously in the Employee entity.
    /// Passwords are hashed using PBKDF2 (via Identity) and never stored in plain text.
    /// </summary>
    public class AppUser : IdentityUser<int>
    {
        /// <summary>Employee's full name.</summary>
        public string? FullName { get; set; }

        /// <summary>Employee's job title/position.</summary>
        public string? Post { get; set; }

        /// <summary>Employee's department/service.</summary>
        public string? Service { get; set; }

        /// <summary>
        /// User role for authorization (Admin, PDR, Purchasing, IT, Infrastructure, Employee).
        /// This is separate from ASP.NET Identity roles but used for business-specific authorization.
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// Reference to the Employee entity (one-to-one relationship).
        /// Allows associating an identity account with employee metadata.
        /// </summary>
        public int? EmployeeId { get; set; }

        /// <summary>Account creation timestamp (UTC).</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Last account modification timestamp (UTC).</summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>Soft delete flag for audit purposes.</summary>
        public bool IsDeleted { get; set; }
    }
}
