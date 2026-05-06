namespace PropertyLeasingAPI.Models
{
    /// <summary>
    /// Q3: single source of truth for role names. Use these constants in every
    /// [Authorize(Roles = ...)] attribute and User.IsInRole(...) call.
    /// </summary>
    public static class Roles
    {
        public const string PropertyManager  = "PropertyManager";
        public const string Tenant           = "Tenant";
        public const string MaintenanceStaff = "MaintenanceStaff";
    }
}
