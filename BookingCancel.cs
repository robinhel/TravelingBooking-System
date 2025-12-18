
using System.Security.Cryptography.X509Certificates;
using MySql.Data.MySqlClient;

namespace server;

public static class BookingCancel 
{
    public static async Task<IResult> CancelBooking(int bookingId, Config config, HttpContext ctx)
    {
        // Get the role of the currently logged-in user
        string? role = await Permission.GetUserRole(config, ctx);
        if (!Permission.IsAdmin(role))         // If user is NOT Admin, deny the action with a response
        {
            return Results.Json(
                new
                {
                    Message = "Action denied! You cannot perform this function."
                },
                statusCode: StatusCodes.Status403Forbidden
            );
        }

        using var connection = new MySqlConnection(config.connectionString);         // Create a database connection using the connection string
        await connection.OpenAsync();                                                // Open the database connection asynchronously 

        using var transaction = await connection.BeginTransactionAsync(); 

        try
        {
            // Delete room-booking dependants first; foreign key constraints
            string deleteRoomLinks = """
                DELETE FROM rooms_by_booking
                WHERE booking_id = @booking_id
            """;

            // Create SQL command using the transaction
            using (var command = new MySqlCommand(deleteRoomLinks, connection, (MySqlTransaction) transaction))
            {
                command.Parameters.AddWithValue("@booking_id", bookingId);   // Bind the booking ID parameter
                await command.ExecuteNonQueryAsync();                        // Execute deletion of linked room
            }

            // Delete the booking itself
            string deleteBooking = """
            DELETE FROM bookings
            WHERE booking_id = @booking_id
            """;

            using (var command = new MySqlCommand(deleteBooking, connection, (MySqlTransaction) transaction))
            {
                command.Parameters.AddWithValue("@booking_id", bookingId);     // Bind booking ID again
                int affected = await command.ExecuteNonQueryAsync();           // Execute deletion of booking

                // If no row was deleted, the booking did not exist
                if (affected == 0)
                {
                    await transaction.RollbackAsync();        // Undo previous delete
                    return Results.NotFound("Booking not found");
                }
            }
            // Commit transaction (save changes)
            await transaction.CommitAsync();
            return Results.Ok($"booking_id = {bookingId} cancelled successfully");     // return success response
        }

        catch (Exception error)
        {
            // If any error occur, undo all database changes. And return an error message
            await transaction.RollbackAsync();
            return Results.Problem($"Error cancelling booking: {error.Message}");
        }
    }
}