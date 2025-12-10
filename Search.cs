using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Common;

namespace server;

public static class SearchHandler
{
    public static async Task<IResult> Search(
        string city,
        DateTime Check_in,
        DateTime Check_out,
        int guests,
        Config config
    )
    {
        string sql = """
            SELECT 
                h.hotel_id,
                h.name AS hotel_name,
                r.rooms_id,
                r.number AS room_number,
                r.capacity,
                r.price
            FROM rooms r
            JOIN hotels h ON r.hotel_id = h.hotel_id
            JOIN cities c ON h.city_id = c.city_id
            WHERE c.name = @city
            AND r.capacity >= @guests
            AND r.rooms_id NOT IN (
                SELECT rb.rooms_id
                FROM rooms_by_booking rb
                JOIN bookings b ON rb.booking_id = b.booking_id
                WHERE NOT (b.Check_OUT <= @check_in OR b.Check_IN >= @check_out)
            )
        """;
        var parameters = new MySqlParameter[]
        {
    new("@city",city),
    new("@guests",guests),
    new("@Check_in",Check_in),
    new("@Check_out",Check_out),
        };

        using var reader = await MySqlHelper.ExecuteReaderAsync(config.connectionString, sql, parameters);

        var result = new List<object>();


        while (await reader.ReadAsync())
        {
            result.Add(new
            {
                hotel_id = reader.GetInt32("hotel_id"),
                hotel_name = reader.GetString("hotel_name"),
                room_id = reader.GetInt32("rooms_id"),
                room_number = reader.GetInt32("room_number"),
                capacity = reader.GetInt32("capacity"),
                price = reader.GetInt32("price")

            });
        }
        return Results.Ok("ok");
    }
}
