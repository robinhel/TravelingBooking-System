using MySql.Data.MySqlClient;

namespace server;

public static class Permission
{
    public static async Task<string?> GetUserRole(Config config, HttpContext ctx)
    {
        int? users_id = ctx.Session.GetInt32("user_id");
        if (users_id == null)
        return null;

        string query = "SELECT Role FROM users WHERE user_id=@users_id";
        var parameters = new MySqlParameter[]
        {
            new("@users_id", users_id.Value)
        };
        object? result = await MySqlHelper.ExecuteScalarAsync(config.connectionString, query, parameters);

        return result?.ToString(); // "Admin" or "Traveler" or null
    }

    // Helper to get active_user id
    public static int? GetUserId(HttpContext ctx)
    {
        return ctx.Session.GetInt32("user_id");
    }

    public static bool IsAdmin(string? role) => role == "Admin";
}