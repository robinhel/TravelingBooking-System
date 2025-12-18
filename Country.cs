using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;       // MySQL client library for database access
using server;

public static class Country
{
    // Defining the expected request body when adding a country
    public record CountryRequest(string Name);

    // Here is the Endpoint: POST /countries
    public static async Task<IResult> AddCountry(CountryRequest request, Config config, HttpContext ctx)
    {
        string? role = await Permission.GetUserRole(config, ctx);   // Get the role of the currently logged-in user
        if (!Permission.IsAdmin(role))            // Check if user is NOT admin 
            return Results.Forbid();              // Stop further execute if it returns True....gives HTTP 403 Forbidden 

        string query = "INSERT INTO countries (name) VALUES (@name)";       // SQL query to insert a new country into the database
        var parameters = new MySqlParameter[]                   // Parameters prevent SQL injection and bind values safely
        {
            new("@name", request.Name)
        };

        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, query, parameters);     // Execute the INSERT command asynchronously

        return Results.Ok("Country added!");          // Return success message
    }

    // Here is the Endpoint: GET /countries
    public static async Task<IResult> GetCountry(Config config)
    {
        string query = "SELECT countries_id, name FROM countries";          // SQL query to retrieve all countries

        using var reader = await MySqlHelper.ExecuteReaderAsync(config.connectionString, query);    // Execute query and obtain a reader

        var list = new List<object>();      // List to store result rows
        while (await reader.ReadAsync())    // Read each row returned from the database
        {
            list.Add(new
            {
                CountryId = reader.GetInt32(0),  // Primary key 
                Name = reader.GetString(1),      // Country name
            });
        }

        // Return the list of countries as JSON
        return Results.Ok(list);
    }

}

