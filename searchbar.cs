using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
namespace server;


public static class SearchHandler
{
    // Dessa records används för att bestämma exakt vilken info som ska skickas in till Postman
    public record CityFoodResult(
        int HotelId,
        string HotelName,
        string FoodName,
        string FoodDescription,
        string CityName
    );
    public record SearchRequest(
    string Food
    );

    public record RoomSearchRequest(
        string CheckInDate,
        string CheckOutDate,
        string? CountryName,
        string? CityName,
        string? HotelName
    );

    public record AvailableRoomResult(
        int RoomID,
        int RoomNumber,
        int Price,
        int Capacity,
        string HotelName,
        string CityName,
        string CountryName
    );


// Sök efter mat och få fram information
    public static async Task<IResult> SearchFoodAndGetHotels(SearchRequest request, Config config)
    {
        string search = request.Food ?? "";

        string query = @"
        SELECT
        h.hotel_id,
        h.name AS HotelName,
        IFNULL(c.food_name, '') AS food_name, 
        IFNULL(c.food_description, '') AS food_description,
        c.city_name AS CityName
        FROM cities AS C
        JOIN Hotels AS H on c.city_id = h.city_id
        WHERE c.food_name LIKE CONCAT('%', @SearchTerm, '%')
        ";

        var parameters = new MySqlParameter[]
        {
        new("@SearchTerm", search)
        };
        var results = new List<CityFoodResult>();

        int HotelId;
        string HotelName;
        string FoodName;
        string FoodDescription;
        string CityName;

        // Skapar en anslutning till databasen
        using (var connection = new MySqlConnection(config.connectionString))
        {
            await connection.OpenAsync(); // Öppnar dörren till databasen
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.Add(parameters[0]); // Skickar med sökordet

                using (var reader = await command.ExecuteReaderAsync()) // Utför sökningen och börjar läsa svaren
                {
                    while (await reader.ReadAsync())
                    {
                        // skapar ett objekt för varje rad vi hittar
                        HotelId = reader.GetInt32(0);
                        HotelName = reader.GetString(1);
                        FoodName = reader.GetString(2);
                        FoodDescription = reader.GetString(3);
                        CityName = reader.GetString(4);

                        var profile = new CityFoodResult(HotelId, HotelName, FoodName, FoodDescription, CityName);
                        results.Add(profile);
                    }
                }
            }

        }
        // Om listan är tom = hittade inget meddelande
        if (results.Count == 0)
        {
            return Results.NotFound("Didn't find any results. Please search for something else");
        }
        return Results.Ok(results);


    }
    // Sök lediga rum med filter
    public static async Task<IResult> SearchAvailableRooms(RoomSearchRequest request, Config config)
    {
        // Kontrollera så att datum finns med i posten, annars går det inte att söka
        if (string.IsNullOrWhiteSpace(request.CheckInDate) || string.IsNullOrWhiteSpace(request.CheckOutDate))
        {
            return Results.BadRequest("CheckInDate and CheckOutDate are required fields.");
        }
        // NOT IN används för att rensa bort rum som redan har bokningar på de datumen
        string query = $@"
            SELECT 
                R.room_id, 
                R.number, 
                R.Price, 
                R.capacity,
                H.name AS HotelName, 
                CI.city_name AS CityName,  
                CN.name AS CountryName
            FROM rooms AS R
            JOIN Hotels AS H ON R.hotel_id = H.hotel_id
            JOIN cities AS CI ON H.city_id = CI.city_id
            JOIN countries AS CN ON CI.countries_id = CN.countries_id
            
            WHERE R.room_id NOT IN (
                SELECT DISTINCT RBB.room_id
                FROM rooms_by_booking AS RBB
                JOIN bookings AS B ON RBB.booking_id = B.booking_id
                WHERE B.Check_OUT > @CheckInDate 
                  AND B.Check_IN < @CheckOutDate
            )
            AND (H.name LIKE CONCAT('%', @HotelName, '%') OR @HotelName IS NULL)
            AND (CI.city_name LIKE CONCAT('%', @CityName, '%') OR @CityName IS NULL)
            AND (CN.name LIKE CONCAT('%', @CountryName, '%') OR @CountryName IS NULL)
            ORDER BY R.room_id;            ";

        // Kopplar ihop postman värdena med sql frågan
        // Om ett filter är tomt så skickar vi DBNULL som är databasens sätt att säga inget/tomt
        var parameters = new List<MySqlParameter>
        {
            new("@CheckInDate", request.CheckInDate),
            new("@CheckOutDate", request.CheckOutDate),
            new("@HotelName", string.IsNullOrEmpty(request.HotelName) ? (object)DBNull.Value : request.HotelName),
            new("@CityName", string.IsNullOrEmpty(request.CityName) ? (object)DBNull.Value : request.CityName),
            new("@CountryName", string.IsNullOrEmpty(request.CountryName) ? (object)DBNull.Value : request.CountryName)
        };

        var results = new List<AvailableRoomResult>();


        using (var connection = new MySqlConnection(config.connectionString))
        {
            await connection.OpenAsync();
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.AddRange(parameters.ToArray());
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        // skapa listan med rum som vi visar för användaren
                        var room = new AvailableRoomResult(
                            reader.GetInt32(0),
                            reader.GetInt32(1),
                            reader.GetInt32(2),
                            reader.GetInt32(3),
                            reader.GetString(4),
                            reader.GetString(5),
                            reader.GetString(6)
                        );
                        results.Add(room);
                    }
                }
            }
        }
        if (results.Count == 0)
        {
            return Results.NotFound("Could not find any available rooms for the given dates.");

        }
        return Results.Ok(results);
    }
}