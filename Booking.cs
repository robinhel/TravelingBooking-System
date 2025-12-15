using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace server;

public static class BookingHandler
{
    public record BookingResult(
        int BookingId,
        int RoomId,
        int HotelId,
        string HotelName,
        string CityName,
        DateTime FromDate,
        DateTime ToDate,
        string RoomStatus
    );

    public record BookingRequest(
        int RoomId,
        DateTime FromDate,
        DateTime ToDate
    );

    public static async Task<IResult> CreateBooking(BookingRequest request, Config config)
    {
        int roomId = request.RoomId;
        DateTime from = request.FromDate;
        DateTime to = request.ToDate;

        string roomQuery = @"
        SELECT
            r.room_id,
            h.hotel_id,
            h.name,
            c.name,
            r.room_status
        FROM rooms r
        JOIN hotels h ON r.hotel_id = h.hotel_id
        JOIN cities c ON h.city_id = c.city_id
        WHERE r.room_id = @RoomId
        ";

        string insertBooking = @"
        INSERT INTO bookings (from_date, to_date)
        VALUES (@FromDate, @ToDate);
        SELECT LAST_INSERT_ID();
        ";

        string insertLink = @"
        INSERT INTO rooms_by_booking (booking_id, rooms_id)
        VALUES (@BookingId, @RoomId)
        ";

        string updateRoom = @"
        UPDATE rooms
        SET room_status = 'unavailable'
        WHERE room_id = @RoomId
        ";

        int RoomID;
        int HotelID;
        string HotelName;
        string CityName;
        string RoomStatus;

        using (var connection = new MySqlConnection(config.connectionString))
        {
            await connection.OpenAsync();

            using (var command = new MySqlCommand(roomQuery, connection))
            {
                command.Parameters.Add(new MySqlParameter("@RoomId", roomId));

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                    {
                        return Results.NotFound("Room not found");
                    }

                    RoomID = reader.GetInt32(0);
                    HotelID = reader.GetInt32(1);
                    HotelName = reader.GetString(2);
                    CityName = reader.GetString(3);
                    RoomStatus = reader.GetString(4);
                }
            }

            if (RoomStatus == "unavailable")
            {
                return Results.BadRequest("Room unavailable");
            }

            int newBookingId;
            using (var command = new MySqlCommand(insertBooking, connection))
            {
                command.Parameters.Add(new MySqlParameter("@FromDate", from));
                command.Parameters.Add(new MySqlParameter("@ToDate", to));

                object result = await command.ExecuteScalarAsync();
                newBookingId = Convert.ToInt32(result);
            }

            using (var command = new MySqlCommand(insertLink, connection))
            {
                command.Parameters.Add(new MySqlParameter("@BookingId", newBookingId));
                command.Parameters.Add(new MySqlParameter("@RoomId", RoomID));
                await command.ExecuteNonQueryAsync();
            }

            using (var command = new MySqlCommand(updateRoom, connection))
            {
                command.Parameters.Add(new MySqlParameter("@RoomId", RoomID));
                await command.ExecuteNonQueryAsync();
            }

            var profile = new BookingResult(
                newBookingId,
                RoomID,
                HotelID,
                HotelName,
                CityName,
                from,
                to,
                "unavailable"
            );

            return Results.Ok(profile);
        }
    }
}
