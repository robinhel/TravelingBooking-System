using MySql.Data.MySqlClient;

namespace server;

public static class Hotel
{
    public record AddHotelRequest(string Name, int CityId);

    public static async Task<IResult> AddHotel(AddHotelRequest request, Config config, HttpContext ctx)
    {
        string? role = await Permission.GetUserRole(config, ctx);
        if (!Permission.IsAdmin(role))
            return Results.Forbid();

        string query = "INSERT INTO hotels (name, city_id) VALUES (@name, @city_id)";
        var parameters = new MySqlParameter[]
        {
            new("@name", request.Name),
            new("@city_id", request.CityId)
        };

        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, query, parameters);

        return Results.Ok("Hotel added!");
    }

    // GET /hotels/{cityId)} - return hotels for a given city only
    public static async Task<IResult> GetHotelByCity(int cityId, Config config)
    {
        string query = @"
        SELECT hotels.hotel_id, hotels.name AS hotel_name, cities.city_id, cities.city_name 
        FROM hotels JOIN cities ON hotels.city_id = cities.city_id
        WHERE cities.city_id = @cityId
        ";

        var parameters = new MySqlParameter[]
        {
            new("@cityId", cityId)
        };

        using var reader = await MySqlHelper.ExecuteReaderAsync(config.connectionString, query, parameters);

        var list = new List<object>();
        while (await reader.ReadAsync())
        {
            list.Add(new
            {
                HotelId = reader.GetInt32("hotel_id"),
                HotelName = reader.GetString("hotel_name"),
                CityId = reader.GetInt32("city_id"),
                CityName = reader.GetString("city_name")
            });
        }

        return Results.Ok(list);
    }
    public static async Task<IResult> DeleteHotel(int id, Config config, HttpContext ctx)
    {
        string? role = await Permission.GetUserRole(config, ctx);
        if (!Permission.IsAdmin(role))
            return Results.Forbid();


        string checkQuery = "SELECT COUNT(*) FROM rooms WHERE hotel_id = @id";
        var parameters = new MySqlParameter[]
        {
            new("@id", id)
        };
        object countResult = await MySqlHelper.ExecuteScalarAsync(config.connectionString, checkQuery, parameters);
        int numberOfRooms = Convert.ToInt32(countResult);

        if (numberOfRooms > 0)
        {
            return Results.Conflict($"Can't delete the the hotel. There is still {numberOfRooms} left registered. Please remove them first.");
        }

        string deleteQuery = "DELETE FROM hotels WHERE hotel_id = @id";
        try
        {
            int affected = await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, deleteQuery, parameters);

            if (affected == 0) return Results.NotFound("No hotels with that ID was found. Try again.");

            return Results.Ok($"Hotel with ID :{id} was deleted.");
        }
        catch (MySqlException error)
        {
            return Results.Problem($"Databas error: {error.Message}");
        }
    }

}