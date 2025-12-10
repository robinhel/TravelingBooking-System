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
    public static async Task<IResult> GetCountry(CountryRequest request, Config config, HttpContext ctx)
    {
        string query = "SELECT name FROM countries WHERE name=@name";
        var parameters = new MySqlParameter[]
        {
                new("@name", request.Name)
        };

        await MySqlHelper.ExecuteScalarAsync(config.connectionString, query, parameters);

        return Results.Ok("Successful!");
    }
}