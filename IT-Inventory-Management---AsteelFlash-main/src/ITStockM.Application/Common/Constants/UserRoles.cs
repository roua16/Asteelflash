namespace ITStockM.Models.Constants
{
    /// <summary>
    /// Defines user roles for the IT Stock Management system
    /// </summary>
    public static class UserRoles
    {
        /// <summary>
        /// System Administrator - Full access to all features
        /// </summary>
        public const string Admin = "Admin";

        /// <summary>
        /// PDR (Production/Development/Research) - Manages PDR stock and materials
        /// </summary>
        public const string PDR = "PDR";

        /// <summary>
        /// Purchasing Department - Manages procurement and supplier relations
        /// </summary>
        public const string Purchasing = "Purchasing";

        /// <summary>
        /// IT Department - Manages IT stock and assignments
        /// </summary>
        public const string IT = "IT";

        /// <summary>
        /// Infrastructure Department - Manages infrastructure requests and approvals
        /// </summary>
        public const string Infrastructure = "Infrastructure";

        /// <summary>
        /// Employee - Standard user with limited access
        /// </summary>
        public const string Employee = "Employee";

        /// <summary>
        /// Returns all available roles
        /// </summary>
        public static string[] All => new[]
        {
            Admin,
            PDR,
            Purchasing,
            IT,
            Infrastructure,
            Employee
        };

        /// <summary>
        /// Roles with administrative privileges
        /// </summary>
        public static string[] Administrative => new[]
        {
            Admin,
            PDR,
            Purchasing,
            IT,
            Infrastructure
        };

        /// <summary>
        /// Roles that can manage materials
        /// </summary>
        public static string[] MaterialManagers => new[]
        {
            Admin,
            PDR,
            IT
        };

        /// <summary>
        /// Roles that can approve requests
        /// </summary>
        public static string[] RequestApprovers => new[]
        {
            Admin,
            Infrastructure,
            Purchasing
        };

        public static string NormalizeRole(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return Employee;
            }

            var trimmed = role.Trim();

            foreach (var knownRole in All)
            {
                if (string.Equals(trimmed, knownRole, StringComparison.OrdinalIgnoreCase))
                {
                    return knownRole;
                }
            }

            // Fallback heuristics for legacy values (e.g., job titles stored in Post).
            var lowered = trimmed.ToLowerInvariant();

            if (lowered.Contains("admin")) return Admin;
            if (lowered.Contains("pdr")) return PDR;
            if (lowered.Contains("purch")) return Purchasing;
            if (lowered.Contains("infra")) return Infrastructure;
            if (lowered.Contains("it")) return IT;

            return Employee;
        }
    }
}
