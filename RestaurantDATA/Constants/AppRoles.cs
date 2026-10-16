namespace RestaurantDATA.Constants
{

    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Waiter = "Waiter";
        public const string Customer = "Customer";

        public const string Staff = Admin + "," + Waiter;

        public static readonly string[] All = { Admin, Waiter, Customer };


        public static string? Normalize(string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return null;
            }

            var trimmed = role.Trim();
            return All.FirstOrDefault(r => string.Equals(r, trimmed, StringComparison.OrdinalIgnoreCase));
        }
    }
}
