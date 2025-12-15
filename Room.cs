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

        string query = "INSERT INTO rooms (number, price, capacity, hotel_id) VALUES (@number, @price, @capacity, @hotel_id)";
        var parameters = new MySqlParameter[]
        {
            
            new("@number", request.RoomNumber),
            new("@price", request.Price),
            new("@capacity", request.RoomCapacity),
            new("@hotel_id", request.HotelId)
        };

        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, query, parameters);

        return Results.Ok("Room added!");
    }

     public static async Task<IResult> GetRooms(int hotelId, Config config)
    {
        string query = """
        SELECT room_id, number, capacity, price 
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
                RoomId = reader.GetInt32("room_id"),
                RoomName = reader.GetInt32("number"),
                Price = reader.GetInt32("price"),
                RoomCapacity = reader.GetInt32("capacity")
            });
        }

        return Results.Ok(list);
    }
}