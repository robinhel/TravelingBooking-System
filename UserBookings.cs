using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;
using server;

namespace server;

public static class UserBooking
{
    public record BookingView(
        int BookingID,
        string HotelName,
        int RoomNumber,
        DateTime CheckIn,
        DateTime CheckOut,
        int Price
    );

    public static async Task<IResult> GetMyBookings(Config config, HttpContext ctx)
    {
        int? userID = ctx.Session.GetInt32("user_id");

        if (userID == null)
        {
            return Results.Content("You are not logged in");
        }

        string query = @"
         SELECT
        b.booking_id,
        h.name AS HotelName,
        r.number AS RoomNumber,
        b.check_in,
        b.check_out,
        r.Price
        FROM bookings AS b
        JOIN rooms_by_booking as rb ON b.booking_id = rb.booking_id
        JOIN rooms as r ON rb.room_id = r.room_id
        JOIN hotels as h on r.hotel_id = h.hotel_id
        WHERE b.user_id = @UserID
        ORDER BY b.check_in DESC
        ";

        var bookings = new List <BookingView>();

        using(var connection = new MySqlConnection(config.connectionString))
        {
            await connection.OpenAsync();
            using (var command = new MySqlCommand(query,connection))
            {
                command.Parameters.AddWithValue("@UserID", userID);

                using(var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        bookings.Add(new BookingView(
                            reader.GetInt32(0),
                            reader.GetString(1),
                            reader.GetInt32(2),
                            reader.GetDateTime(3),
                            reader.GetDateTime(4),
                            reader.GetInt32(5)                      
                        ));

                    }
                }
            }
        }
        if (bookings.Count == 0)
        {
            return Results.NotFound("You don't have any bookings.");
        }
        return Results.Ok(bookings);

    }
}