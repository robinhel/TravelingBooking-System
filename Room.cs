using MySql.Data.MySqlClient;
namespace server;

public static class Rooms
{
    public record AddRoomRequest(int HotelId, int RoomNumber, int RoomCapacity, int Price);

    public static async Task<IResult> AddRoom(AddRoomRequest request, Config config, HttpContext ctx)
    {
        string? role = await Permission.GetUserRole(config, ctx);
        if (!Permission.IsAdmin(role))
            return Results.Forbid();

        string query = "INSERT INTO rooms (hotel_id, number, capacity, price) VALUES (@hotel_id, @number, @capacity, @price)";
        var parameters = new MySqlParameter[]
        {
            new("@hotel_id", request.HotelId),
            new("@number", request.RoomNumber),
            new("@capacity", request.RoomCapacity),
            new("@price", request.Price)
        };

        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, query, parameters);

        return Results.Ok("Room added!");
    }

     public static async Task<IResult> GetRooms(int hotelId, Config config)
    {
        string query = """
        SELECT rooms_id, number, capacity, price 
        FROM rooms WHERE hotel_id=@hotel_id
        """;

        var parameters = new MySqlParameter[]
        { 
            new("@hotel_id", hotelId) 
        };

        using var reader = await MySqlHelper.ExecuteReaderAsync(config.connectionString, query, parameters);

        var list = new List<object>();
        while (await reader.ReadAsync())
        {
            list.Add (new
            {
                RoomId = reader.GetInt32("rooms_id"),
                RoomName = reader.GetString("number"),
                RoomCapacity = reader.GetString("capacity"),
                Price = reader.GetString("price")
            });
        }

        return Results.Ok(list);
    }
}