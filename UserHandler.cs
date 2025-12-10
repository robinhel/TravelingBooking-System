using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;
using MySqlX.XDevAPI.Common;

namespace server;

public static class UserHandler
{
    public record UpdateProfileRequest(string Name, string Email, string Password);

    public static async Task<IResult> UpdateProfile(UpdateProfileRequest request, Config config, HttpContext ctx)
    {
        int? userId = ctx.Session.GetInt32("user_id");

        if (userId == null)
        {
            return Results.BadRequest("You must be logged in to update profile");
        }

        string query =
        @"
        UPDATE users
        SET name = @name,
        email = @email, 
        password = @password
        WHERE user_id = @id";

        var parameters = new MySqlParameter[]
        {
            new("@name", request.Name),
            new("@email", request.Email),
            new("@password", request.Password),
            new("@Id", userId)
        };

        try
        {
            await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, query, parameters);
            return Results.Ok(new { Message = "Profile updated successfully " });
        }
        catch (MySqlException error)
        {
            return Results.Problem($"Database error: {error.Message}.\n Please try again.");
        }


    }
}