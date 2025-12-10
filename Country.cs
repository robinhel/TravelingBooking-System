using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using server;

public static class Country
{
    public record CountryRequest(string Name);

    public static async Task<IResult> AddCountry(CountryRequest request, Config config, HttpContext ctx)
    {
        string query = "INSERT INTO countries (name) VALUES (@name)";
        var parameters = new MySqlParameter[]
        {
            new("@name", request.Name)
        };

        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, query, parameters);

        return Results.Ok("Country added!");
    }
    public static async Task<IResult> GetCountry([FromQuery] string name, Config config)
    {
        string query = "SELECT name FROM countries WHERE name=@name";

        var parameters = new MySqlParameter[]
        {
            new("@name", name)
        };

        object result = await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, query, parameters);

        if (result == null)
        {
            return Results.NotFound("Country not found");
        }

        return Results.Ok(new { Name = result.ToString() });
    }
}