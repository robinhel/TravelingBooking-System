using MySql.Data.MySqlClient; // pratar med mysql-databasen (BEHÖVS ALLTID VARA MED)
using Microsoft.AspNetCore.Http; // hanterar webbtrafik(Sessioner och cookies(BEHÖVS ALLTID VARA MED))
using MySqlX.XDevAPI.Common;

namespace server;

public static class UserHandler
{
    public record UpdateProfileRequest(string Name, string Email, string Password, string ConfirmPassword);

    public static async Task<IResult> UpdateProfile(UpdateProfileRequest request, Config config, HttpContext ctx)
    // I = Interface(Regelbok/Kartong)
    //Result för att kunna retunera "resultatet(Results.OK osv) i samma metod
    // config (nycklar till databasen(lösenord, adress))
    //ctx "nuet" information om besökaren, deras session, cookies, ip-adress
    {
        int? userId = ctx.Session.GetInt32("user_id"); // "Känn i fickan" efter ett ID. Om det finns: spara det. Om det är tomt: spara null.

        if (userId == null)
        {
            return Results.BadRequest("You must be logged in to update profile");
        }
        if (request.Password != request.ConfirmPassword)
        {
            return Results.BadRequest("Password do not match, try again ");
        }
        //Instruktioner till databasen
        string query =
        @"
        UPDATE users
        SET name = @name,
        email = @email, 
        password = @password
        WHERE user_id = @id";

        var parameters = new MySqlParameter[]
        {
            //kopplar ihop postman till databasen t.ex Där står @namn på lappen, stoppa in request.Name
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
    public record changePassword(string oldPassword, string newPassword, string ConfirmPassword);

    public static async Task<IResult> ChangePasswordRequest(changePassword request, Config config, HttpContext ctx)

    {
        int? userId = ctx.Session.GetInt32("user_id");
        if (userId == null)
        {
            return Results.BadRequest("You must be logged in. \nTry again");
        }
        if (request.newPassword != request.ConfirmPassword)
        {
            return Results.BadRequest("Password does not match. \nTry again");
        }
        string query =
        @"
            UPDATE users 
            SET password = @newPassword
            WHERE user_Id = @Id AND password = @oldPassword
            ";

        var parameters = new MySqlParameter[]
        {
                new ("@newPassword", request.newPassword),
                new ("@oldPassword", request.oldPassword),
                new("@Id", userId)
        };

        try
        {
            int rowsAffected = await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, query, parameters);

            if (rowsAffected == 0)
            {
                return Results.BadRequest("Incorrect old password. Try again.");
            }
            return Results.Ok(new { message = "Your password was change successfully" });
        }
        catch (MySqlException error)
        {
            return Results.BadRequest($"Database error: {error.Message}");
        }
    }



}