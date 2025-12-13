using MySql.Data.MySqlClient;

namespace server;

public static class City
{
    public record CityRequest(int CountryId, string CityName, string FoodName);

    public static async Task<IResult> AddCity(CityRequest request, Config config, HttpContext ctx)
    {
        // Check if active_user's role is Admin. Admin only can add a city
        string? role = await Permission.GetUserRole(config, ctx);
        if (!Permission.IsAdmin(role))
            return Results.Forbid();

        string query = "INSERT INTO cities (countries_id, city_name, food_name) VALUES (@countries_id, @city_name, @food_name)";
        var parameters = new MySqlParameter[]
        {
            new("@countries_id", request.CountryId),
            new("@name", request.CityName),
            new("@food_name", request.FoodName ?? string.Empty),
        };

        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, query, parameters);

        return Results.Ok("City added!");
    }

    // GET /cities/{countryId} - returns cities for country
    public static async Task<IResult> GetCityByCountry(int countryId, Config config)
    {
        string query = "SELECT city_id, city_name, food_name FROM cities WHERE countries_id=@countries_id";
        var parameters = new MySqlParameter[]
        {
            new("@countries_id", countryId)
        };

        using var reader = await MySqlHelper.ExecuteReaderAsync(config.connectionString, query, parameters);

        var list = new List<object>();
        while (await reader.ReadAsync())
        {
            list.Add (new
            {
                CityId = reader.GetInt32("city_id"),
                CityName = reader.GetString("city_name"),
                FoodName = reader.IsDBNull(reader.GetOrdinal("food_name")) ? "" : reader.GetString("food_name")
            });
        }

        return Results.Ok(list);
    }
}