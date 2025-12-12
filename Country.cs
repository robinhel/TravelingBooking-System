using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using server;

public static class Country
{
    public record CountryRequest(string Name);

    public static async Task<IResult> AddCountry(CountryRequest request, Config config, HttpContext ctx)
    {
        string? role = await Permission.GetUserRole(config, ctx);
        if (!Permission.IsAdmin(role))
            return Results.Forbid();

        string query = "INSERT INTO countries (name) VALUES (@name)";
        var parameters = new MySqlParameter[]
        {
            new("@name", request.Name)
        };

        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, query, parameters);

        return Results.Ok("Country added!");
    }
    public static async Task<IResult> GetCountry(Config config)
    {
        string query = "SELECT countries_id, name FROM countries";

        using var result = await MySqlHelper.ExecuteReaderAsync(config.connectionString, query);

        var list = new List<object>();
        while (await result.ReadAsync())
        {
            list.Add(new
            {
                CountryId = result.GetInt32(0),
                Name = result.GetString(1),
            });
        }

        return Results.Ok(list);
    }


    public static async Task<IResult> DeleteCountry(int id, Config config, HttpContext ctx)
    {
        string? role = await Permission.GetUserRole(config, ctx);
        if (!Permission.IsAdmin(role))
        {
            return Results.BadRequest("You don't have the permission to access this method ");
        }

        string query =
        @" DELETE FROM countries WHERE countries_id = @id;
                ";

        var parameters = new MySqlParameter[]
        {
                  new("@id", id)
        };
        try
        {
            int Affected = await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, query, parameters);
            if (Affected == 0)
            {
                return Results.BadRequest($"No countries with this id: {id} was found. Try again");
            }
            return Results.Ok($"City with ID: {id} has been deleted. ");
        }
        catch (MySqlException error)
        {
            return Results.BadRequest($"Database error: {error.Message}");
        }

    }



}





/*



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
*/