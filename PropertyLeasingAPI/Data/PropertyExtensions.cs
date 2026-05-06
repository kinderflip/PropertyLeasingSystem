using Microsoft.EntityFrameworkCore;

namespace PropertyLeasingAPI.Data
{
    /// <summary>
    /// L4: Safe alternatives to <c>Property.IsStandalone</c> for callers that cannot
    /// guarantee the <c>Units</c> navigation has been eager-loaded.
    /// </summary>
    public static class PropertyExtensions
    {
        /// <summary>
        /// Checks whether a property has zero units by querying the database directly.
        /// Always accurate, regardless of whether the Property entity is tracked.
        /// </summary>
        public static async Task<bool> IsPropertyStandaloneAsync(this AppDbContext db, int propertyId)
        {
            return !await db.Units.AnyAsync(u => u.PropertyId == propertyId);
        }
    }
}
