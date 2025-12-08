using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;
using server;

namespace server
{
    public static class LoginHandler
    {
        public record LoginRequest(string Email, string Password);

        public static async Task<IResult> Login(LoginRequest request, Config config, HttpContext ctx)
        {
            string query = "SELECT id FROM Users WHERE email=@email AND password=@password";
            var parameters = new MySqlParameter[]
            {
                new("@email", request.Email),
                new("@password", request.Password)
            };

            object result = await MySqlHelper.ExecuteScalarAsync(config.connectionString, query, parameters);

            if (result is int id)
            {
                ctx.Session.SetInt32("user_id", id);
                return Results.Ok(new { Message = "Logged in" });
            }

            return Results.BadRequest("Wrong email or password");
        }
    }
}
public record LoginRequest(string Email, string Password);
