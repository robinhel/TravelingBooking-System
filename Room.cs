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

        string query = "INSERT INTO rooms (hotel_id, roomNumber, roomCapacity, price) VALUES (@hotel_id, @roomNumber, @roomCapacity, @price)";
        var parameters = new MySqlParameter[]
        {
            new("@hotel_id", request.HotelId),
            new("@roomNumber", request.RoomNumber),
            new("@roomCapacity", request.RoomCapacity),
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

        var parameters = new MySqlParameter[] { new("@hotel_id", hotelId) };

        using var result = await MySqlHelper.ExecuteReaderAsync(config.connectionString, query, parameters);

        var list = new List<object>();
        while (await result.ReadAsync())
        {
            list.Add (new
            {
                RoomId = result.GetInt32(0),
                RoomName = result.GetString(1),
                RoomCapacity = result.GetString(2),
                Price = result.GetString(3)
            });
        }

        return Results.Ok(list);
    }
}