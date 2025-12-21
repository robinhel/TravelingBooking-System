using MySql.Data.MySqlClient;

namespace server;

public static class Permission          // static handler for permission-related helper methods
{
    // Asynchronous method that returns a string role ("Admin" or "Traveler") or null if no user is logged-in
    public static async Task<string?> GetUserRole(Config config, HttpContext ctx)
    {
        int? users_id = ctx.Session.GetInt32("user_id");    // Reads the currently logged-in user's ID from the session
        if (users_id == null)       // check whether there is no logged-in user
        return null;                // Return null if the user is not authenticated

        string query = "SELECT Role FROM users WHERE user_id=@users_id";    // SQL query to retrieve logged-in user's role from the database
        var parameters = new MySqlParameter[]       // Create a parameter array to safely inject values into the SQL query
        {
            new("@users_id", users_id.Value)        // Bind user ID to the SQL query parameter. 
        };

        // Executes the SQL query asynchronosly
        object? result = await MySqlHelper.ExecuteScalarAsync(config.connectionString, query, parameters); 

        return result?.ToString(); // Converts the result to string if it exists. --> "Admin" or "Traveler" or null
    }

    // Helper to get active_user id
    public static int? GetUserId(HttpContext ctx)
    {
        return ctx.Session.GetInt32("user_id");     // Returns the logged-in user's ID
    }
    // Helper method that checks if the logged-in user has Admin access
    public static bool IsAdmin(string? role) => role == "Admin";
}