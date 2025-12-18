using MySql.Data.MySqlClient;           // MySQL database access client

namespace server;

public static class Hotel           // Handler class for hotel endpoints
{
    // Record defining request body when adding a hotel
    public record AddHotelRequest(string Name, int CityId);

    // Endpoint: POST /hotels
    public static async Task<IResult> AddHotel(AddHotelRequest request, Config config, HttpContext ctx)
    {
        string? role = await Permission.GetUserRole(config, ctx);      // Retrieve cuurent user's role
        if (!Permission.IsAdmin(role))          // Only Admin users may add hotels
            return Results.Forbid();

        // SQL query to insert a hotel
        string query = "INSERT INTO hotels (name, city_id) VALUES (@name, @city_id)";
        var parameters = new MySqlParameter[]       // SQL parameters
        {
            new("@name", request.Name),
            new("@city_id", request.CityId)
        };

        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, query, parameters);    // Execute insert

        return Results.Ok("Hotel added!");      // Confirm success
    }

    // GET /hotels/{cityId)} - return hotels within a given city
    public static async Task<IResult> GetHotelByCity(int cityId, Config config)
    {
        // SQL query to retrieve hotels in a given city
        string query = @"
        SELECT hotels.hotel_id, hotels.name AS hotel_name, cities.city_id, cities.city_name 
        FROM hotels JOIN cities ON hotels.city_id = cities.city_id
        WHERE cities.city_id = @cityId
        ";

        var parameters = new MySqlParameter[]   // Parameter binding
        {
            new("@cityId", cityId)
        };

        using var reader = await MySqlHelper.ExecuteReaderAsync(config.connectionString, query, parameters);    // Execute query and obtain a reader

        var list = new List<object>();  // List to store result rows
        while (await reader.ReadAsync())    // Iterate or read each row returned from database
        {
            list.Add(new
            {
                HotelId = reader.GetInt32("hotel_id"),
                HotelName = reader.GetString("hotel_name"),
                CityId = reader.GetInt32("city_id"),
                CityName = reader.GetString("city_name")
            });
        }
        // Return hotels list
        return Results.Ok(list);
    }

    public static async Task<IResult> DeleteHotel(int id, Config config, HttpContext ctx)
    {
        // 1. Admin-koll
        string? role = await Permission.GetUserRole(config, ctx);
        if (!Permission.IsAdmin(role))
            return Results.Forbid();

        var parameters = new MySqlParameter[]
        {
            new("@id", id)
        };

        try
        {

            string cleanLinks = @"
            DELETE rbb FROM rooms_by_booking rbb
            JOIN rooms r ON rbb.room_id = r.room_id
            WHERE r.hotel_id = @id";
            await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, cleanLinks, parameters);

            string cleanBookings = @"
            DELETE b FROM bookings b
            JOIN rooms r ON b.room_id = r.room_id
            WHERE r.hotel_id = @id";
            await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, cleanBookings, parameters);

            string cleanRooms = "DELETE FROM rooms WHERE hotel_id = @id";
            await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, cleanRooms, parameters);

            string deleteHotel = "DELETE FROM hotels WHERE hotel_id = @id";
            int affected = await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, deleteHotel, parameters);

            if (affected == 0)
                return Results.NotFound($"No hotels with ID: {id} was found.");

            return Results.Ok($"The hotel ID: {id} and all its rooms/bookings have been deleted.");
        }
        catch (MySqlException error)
        {
            return Results.Problem($"Database error: {error.Message}");
        }
    }

}
