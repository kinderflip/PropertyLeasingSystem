namespace PropertyLeasingReports
{
    /// <summary>
    /// Q3: local mirror of the API's role constants. The Reports project doesn't reference
    /// the API project (it consumes data over HTTP), so it keeps its own copy.
    /// </summary>
    public static class Roles
    {
        public const string PropertyManager  = "PropertyManager";
        public const string Tenant           = "Tenant";
        public const string MaintenanceStaff = "MaintenanceStaff";
    }
}
