using MySql.Data.MySqlClient;       // MySQL database access client
namespace server;

public static class Rooms           // Handler for room-related operations
{
    // Record defining request body when adding a room
    public record AddRoomRequest(int HotelId, int RoomNumber, int RoomCapacity, int Price);

    // Endpoint: POST /rooms
    public static async Task<IResult> AddRoom(AddRoomRequest request, Config config, HttpContext ctx)
    {
        string? role = await Permission.GetUserRole(config, ctx);   // Retrieve logged-in user's role
        if (!Permission.IsAdmin(role))          // Only Admin user may add a room
            return Results.Forbid();

        // SQL query to insert a room
        string query = "INSERT INTO rooms (number, price, capacity, hotel_id) VALUES (@number, @price, @capacity, @hotel_id)";
        var parameters = new MySqlParameter[]       // SQL parameters
        {

            new("@number", request.RoomNumber),
            new("@price", request.Price),
            new("@capacity", request.RoomCapacity),
            new("@hotel_id", request.HotelId)
        };

        // Execute insert command
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, query, parameters);

        return Results.Ok("Room added!");       // Confirm sucessful addition of room
    }

    // Endpoint: GET /rooms/{hotelId}
    public static async Task<IResult> GetRooms(int hotelId, Config config)
    {
        // SQL query to retrieve rooms for a given hotel
        string query = """
        SELECT room_id, number, capacity, price 
        FROM rooms WHERE hotel_id=@hotel_id
        """;

        var parameters = new MySqlParameter[]   // Bind hotel ID parameter
        {
            new("@hotel_id", hotelId)
        };

        // Execute query and obtain a reader
        using var reader = await MySqlHelper.ExecuteReaderAsync(config.connectionString, query, parameters);

        var list = new List<object>();  // List to store result rows
        while (await reader.ReadAsync())    //Read each row
        {
            list.Add(new
            {
                RoomId = reader.GetInt32("room_id"),
                RoomName = reader.GetInt32("number"),
                Price = reader.GetInt32("price"),
                RoomCapacity = reader.GetInt32("capacity")
            });
        }

        return Results.Ok(list);        // Return list of rooms
    }
    public static async Task<IResult> DeleteRoom(int id, Config config, HttpContext ctx)
    {
        // Kollar så att det är en admin som är inne och har behörigheten för att radera ett rum
        string? role = await Permission.GetUserRole(config, ctx);
        if (!Permission.IsAdmin(role)) //Har användaren inte behörigheten så kommer det ett felmeddelande 
            return Results.Forbid();

        var parameters = new MySqlParameter[] // Rummets ID 
        {
            new("@id", id)
        };

        try
        {
            // Tar bort kopplingarna från rooms_by_booking (om det finns kopplingar)
            string deleteLinks = "DELETE FROM rooms_by_booking WHERE room_id = @id";
            await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, deleteLinks, parameters);


            //Tar bort alla bokningar kopplade till det specifika rummet 

            string deleteBookings = "DELETE FROM bookings WHERE room_id = @id";
            await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, deleteBookings, parameters);

            // Tar bort rummet 
            string deleteRoom = "DELETE FROM rooms WHERE room_id = @id";
            int affected = await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, deleteRoom, parameters);

            if (affected == 0) // Kollar så att det finns ett rum med det specifika ID:t
                return Results.NotFound($"No rooms with ID: {id} was found.");

            // Bekräftar att rummet togs bort 
            return Results.Ok($"Room with ID: {id}) was succesfully delete");
        }
        catch (MySqlException error)
        {
            return Results.Problem($"Database error: {error.Message}");
        }
    }
}

