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
    
    public static async Task<IResult> GetHotelByCity(int cityId, Config config)
    {
        string query = """
        SELECT hotels.hotel_id, hotels.name, cities.name 
        FROM hotels JOIN cities ON hotels.city_id = cities.city_id
        """;

        using var result = await MySqlHelper.ExecuteReaderAsync(config.connectionString, query);

        var list = new List<object>();
        while (await result.ReadAsync())
        {
            list.Add (new
            {
                HotelId = result.GetInt32(0),
                HotelName = result.GetString(1),
                City = result.GetString(2)
            });
        }

        return Results.Ok(list);
    }
}