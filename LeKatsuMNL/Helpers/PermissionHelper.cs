using System;
using System.Linq;
using System.Security.Claims;

namespace LeKatsuMNL.Helpers
{
    public static class PermissionHelper
    {
        /// <summary>
        /// Checks if the user has permission for a specific module and optional action.
        /// </summary>
        /// <param name="user">The ClaimsPrincipal user.</param>
        /// <param name="module">The module name (e.g., "Users", "Inventory").</param>
        /// <param name="action">Optional action character: 'C' (Create), 'R' (Read/View), 'U' (Update/Edit), 'D' (Delete).</param>
        /// <returns>True if the user has permission; otherwise, false.</returns>
        public static bool HasPermission(ClaimsPrincipal user, string module, char action = '\0')
        {
            if (user == null) return false;

            var role = user.FindFirst(ClaimTypes.Role)?.Value;
            
            // Managers and Staff bypass granular checks for now as per current requirements
            if (role == "BranchManager" || role == "Staff") return true;
            
            var permissions = user.FindFirst("Permissions")?.Value;
            if (string.IsNullOrEmpty(permissions)) return false;
            
            // Super Admin bypass
            if (permissions == "All") return true;

            // Split into modules: Transactions:CRUD;Users:R
            var modulePairs = permissions.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in modulePairs)
            {
                var parts = pair.Split(':');
                if (parts.Length < 1) continue;
                
                if (parts[0].Trim().Equals(module.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    // If no specific action requested, or if 'View' (Read) is requested, 
                    // being in the list means they can view the module.
                    if (action == '\0' || action == 'R') return true;
                    
                    // Check for specific action in the second part (e.g., "CUD")
                    if (parts.Length > 1 && parts[1].Contains(action))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
