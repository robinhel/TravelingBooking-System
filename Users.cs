using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;




namespace server;

public record ShowProfileRequest(int id, string name, string email);
public static class LoginHandler
{
    public record CreateAccountRequest(string name, string email, string password);
    public record LoginRequest(string Email, string Password);

    public static async Task<IResult> Login(LoginRequest request, Config config, HttpContext ctx)
    {
        string query = "SELECT user_id FROM Users WHERE email=@email AND password=@password";
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

    public static async Task<IResult> CreateAccount(CreateAccountRequest request, Config config, HttpContext ctx)
    {
        string AccountQuery = "INSERT INTO users (name, email, password) VALUES (@name , @email, @password)";
        var parameters = new MySqlParameter[]
        {
            new("@name", request.name),
            new("@email", request.email),
            new("@password", request.password)
        };
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, AccountQuery, parameters);

        return Results.Ok("Account created");
    }
}
public static class Users
{

    public static async Task<IResult> ViewProfile(Config config, HttpContext ctx)
    {
        int? SessionUserId = ctx.Session.GetInt32("user_id");
        if (SessionUserId == null)
        {
            return Results.Unauthorized();
        }

        MySqlParameter[] parameters = {
            new MySqlParameter("@user_id" , SessionUserId.Value)
        };


        const string ShowQuery = "SELECT user_id, name, email FROM users WHERE user_id = @user_id";

        using var reader = await MySqlHelper.ExecuteReaderAsync(config.connectionString, ShowQuery, parameters);

        int userId;
        string name;
        string email;

        if (!await reader.ReadAsync())
        {
            return Results.NotFound("Profile not found");
        }
        else
        {
            userId = reader.GetInt32(0);
            name = reader.GetString(1);
            email = reader.GetString(2);
        }
        var Profile = new ShowProfileRequest(userId, name, email);


        return Results.Ok(Profile);
    }

}







