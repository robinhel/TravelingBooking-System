using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;

namespace server;

public static class City
{
    public record CityRequest(int CountryId, string Name, string Culinary);

    public static async Task<IResult> AddCity(CityRequest request, Config config, HttpContext ctx)
    {
        string? role = await Permission.GetUserRole(config, ctx);
        if (!Permission.IsAdmin(role))
            return Results.Forbid();

        string query = "INSERT INTO cities (countries_id, name, culinary) VALUES (@countries_id, @name, @culinary)";
        var parameters = new MySqlParameter[]
        {
            new("@countries_id", request.CountryId),
            new("@name", request.Name),
            new("@culinary", request.Culinary)
        };

        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, query, parameters);

        return Results.Ok("City added!");
    }

    public static async Task<IResult> GetCityByCountry(int countryId, Config config)
    {
        string query = "SELECT city_id, name, culinary FROM cities WHERE countries_id=@countries_id";
        var parameters = new MySqlParameter[]
        {
            new("@countries_id", countryId)
        };

        using var result = await MySqlHelper.ExecuteReaderAsync(config.connectionString, query, parameters);

        var list = new List<object>();
        while (await result.ReadAsync())
        {
            list.Add (new
            {
                CityId = result.GetInt32(0),
                Name = result.GetString(1),
                Culinary = result.GetString(2)
            });
        }

        return Results.Ok(list);
    }
}