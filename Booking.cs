using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace server;

public static class BookingHandler
{
    public record BookingRequest(int RoomId, DateTime FromDate, DateTime ToDate);

    public static async Task<IResult> CreateBooking(BookingRequest request, Config config, HttpContext ctx)
    {
        // HÄMTAR user_id FRÅN SESSION
        int? userId = ctx.Session.GetInt32("user_id");

        // OM INTE INLOGGAD, AVBRYT
        if (userId == null)
        {
            return Results.Unauthorized();
        }

        int roomId = request.RoomId;

        string checkAvailability = """
        SELECT COUNT(*)
        FROM bookings b
        JOIN rooms_by_booking rb ON b.booking_id = rb.booking_id
        WHERE rb.rooms_id = @RoomId
        AND @FromDate < b.check_out
        AND @ToDate > b.check_in
        """;

        string insertBooking = """
        INSERT INTO bookings (user_id, check_in, check_out)
        VALUES (@UserId, @CheckIn, @CheckOut);
        SELECT LAST_INSERT_ID();
        """;

        string insertRoomLink = """
        INSERT INTO rooms_by_booking (booking_id, rooms_id)
        VALUES (@BookingId, @RoomId)
        """;

        using (var connection = new MySqlConnection(config.connectionString))
        {
            await connection.OpenAsync();

            using (var command = new MySqlCommand(checkAvailability, connection))
            {
                command.Parameters.AddWithValue("@RoomId", roomId);
                command.Parameters.AddWithValue("@FromDate", request.FromDate);
                command.Parameters.AddWithValue("@ToDate", request.ToDate);

                int conflicts = Convert.ToInt32(await command.ExecuteScalarAsync());

                // OM DET FINNS MINST EN KROCK
                if (conflicts > 0)
                {
                    return Results.Json(
                        new { message = "Room already booked for selected dates" },
                        statusCode: StatusCodes.Status409Conflict
                    );
                }
            }


            int bookingId;

            // SKAPAR BOOKING MED user_id
            using (var command = new MySqlCommand(insertBooking, connection))
            {
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@CheckIn", request.FromDate);
                command.Parameters.AddWithValue("@CheckOut", request.ToDate);

                object result = await command.ExecuteScalarAsync();
                bookingId = Convert.ToInt32(result);
            }

            // KOPPLAR RUM TILL BOOKING
            using (var command = new MySqlCommand(insertRoomLink, connection))
            {
                command.Parameters.AddWithValue("@BookingId", bookingId);
                command.Parameters.AddWithValue("@RoomId", roomId);
                await command.ExecuteNonQueryAsync();
            }
        }


        return Results.Ok(new
        {
            message = "Booking created",
            roomId = roomId,
            from = request.FromDate,
            to = request.ToDate
        });
    }
}
