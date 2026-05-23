namespace ITStockM.Models.ViewModels
{
    /// <summary>
    /// Lightweight user projection returned by the authentication service.
    ///
    /// Security: this type intentionally has no Password property.
    /// Passwords must never be stored in memory beyond the authentication call.
    /// </summary>
    public class AppUser
    {
        public int Id { get; set; }

        public string Email { get; set; } = string.Empty;

        /// <summary>Job title / position stored in the Employee table.</summary>
        public string Post { get; set; } = string.Empty;

        /// <summary>Application role (Admin, PDR, IT, …).</summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>Employee's display name.</summary>
        public string FullName { get; set; } = string.Empty;
    }
}
