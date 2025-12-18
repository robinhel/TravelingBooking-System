using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;
using MySqlX.XDevAPI.Common;

namespace server;

public static class UserHandler // klass som hanterar användarlogik(uppdatera profil, byta lösenord)
{
    public record UpdateProfileRequest(string Name, string Email, string Password, string ConfirmPassword);

    public static async Task<IResult> UpdateProfile(UpdateProfileRequest request, Config config, HttpContext ctx)
    // metod för att uppdatera användarens namn, e-post och lösenord 
    {
        int? userId = ctx.Session.GetInt32("user_id"); // Hämtar användarens ID från session för att se om den är inloggad
        if (userId == null)
        {
            return Results.BadRequest("You must be logged in to update profile"); // Om inget ID finns i sessionen så kommer felmeddelande
        }
        if (request.Password != request.ConfirmPassword) // Kollar så att nya lösenordet matchar det bekräftelse-lösenordet
        {
            return Results.BadRequest("Password do not match, try again "); // Felmeddelande om det inte gör det 
        }
        string query = // SQL-fråga för att uppdatera användarens uppgifter i databasen
        @"
        UPDATE users
        SET name = @name,
        email = @email, 
        password = @password
        WHERE user_id = @id";

        var parameters = new MySqlParameter[] // Skapar parametrar för SQL frågan och skyddar mot SQL-injektion
        {
            new("@name", request.Name),
            new("@email", request.Email),
            new("@password", request.Password),
            new("@Id", userId)
        };

        try
        {
            // Kör frågan asynkront
            await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, query, parameters);
            // Retunerar OK om allt gick bra samt ett meddelande som bekräftar att allt gick bra 
            return Results.Ok(new { Message = "Profile updated successfully " });
        }
        catch (MySqlException error)
        {
            // Fångar databasfel och retunerar ett felmeddelande 
            return Results.Problem($"Database error: {error.Message}. Please try again.");
        }


    }
    public record changePassword(string oldPassword, string newPassword, string ConfirmPassword);

    public static async Task<IResult> ChangePasswordRequest(changePassword request, Config config, HttpContext ctx)
    // Metod för att byta lösenord, kräver att man uppger sitt gamla lösenord också 
    {
        int? userId = ctx.Session.GetInt32("user_id"); // Kontrollerar om användaren är inloggad 
        if (userId == null)
        {
            // Felmeddelande om inte användaren är inloggad 
            return Results.BadRequest("You must be logged in. Try again");
        }
        if (request.newPassword != request.ConfirmPassword) // Ser till att det nya lösenorde matchar 
        {
            return Results.BadRequest("Password does not match. Try again"); // Felmeddelande om det inte gör 
        }
        string query = // Uppdaterar lösenordet endast om user_id stämmer och det gamla lösenordet är rätt 
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
            int Affected = await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, query, parameters);

            if (Affected == 0)
            {
                return Results.BadRequest("Incorrect old password. Try again.");
            }
            return Results.Ok(new
            {
                message = "Your password was change successfully"
            });
        }
        catch (MySqlException error)
        {
            return Results.BadRequest($"Database error: {error.Message}");
        }
    }



}