using MySql.Data.MySqlClient;           // MySQL client library for database access
using Microsoft.AspNetCore.Http;
using MySqlX.XDevAPI.Common;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.Metadata;

namespace server;

public static class City        // static handler for city-related operations
{
    public record CityRequest(int CountryId, string CityName, string FoodName);

    // Here is the Endpoint: POST /cities
    public static async Task<IResult> AddCity(CityRequest request, Config config, HttpContext ctx)
    {
        // Retrieve logged-in user's role. 
        string? role = await Permission.GetUserRole(config, ctx);
        if (!Permission.IsAdmin(role))          // Admin only can add a city
            return Results.Forbid();

        // SQL query to insert new city into cities table
        string query = "INSERT INTO cities (countries_id, city_name, food_name) VALUES (@countries_id, @city_name, @food_name)";
        var parameters = new MySqlParameter[]   // Parameterized query values
        {
            new("@countries_id", request.CountryId),
            new("@city_name", request.CityName),
            new("@food_name", request.FoodName ?? string.Empty),
        };

        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, query, parameters);     // Execute insert command

        return Results.Ok("City added!");       // Return confirmation message
    }

    // Endpoint: GET /cities/{countryId} - return cities in a particular country
    public static async Task<IResult> GetCityByCountry(int countryId, Config config)
    {
        // SQL query to fetch cities belonging to a country
        string query = "SELECT city_id, city_name, food_name FROM cities WHERE countries_id=@countries_id";
        var parameters = new MySqlParameter[]       // Bind countryId to query
        {
            new("@countries_id", countryId)
        };

        using var reader = await MySqlHelper.ExecuteReaderAsync(config.connectionString, query, parameters);    // Execute reader

        var list = new List<object>();      // List to store result rows
        while (await reader.ReadAsync())    // Iterate each result row returned from the database
        {
            list.Add(new
            {
                CityId = reader.GetInt32("city_id"),
                CityName = reader.GetString("city_name"),
                FoodName = reader.IsDBNull(reader.GetOrdinal("food_name")) ? "" : reader.GetString("food_name")
            });
        }

        // Return list of cities
        return Results.Ok(list);
    }
}
