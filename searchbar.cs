using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
namespace server;


public static class SearchHandler
{
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
        string CheckOutDate
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
        using (var connection = new MySqlConnection(config.connectionString))
        {
            await connection.OpenAsync();
            using (var command = new MySqlCommand(query, connection))
            {
                command.Parameters.Add(parameters[0]);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {

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
        if (results.Count == 0)
        {
            return Results.NotFound("Didn't find any results. Please search for something else");
        }
        return Results.Ok(results);


    }
    public static async Task<IResult> SearchAvailableRooms(RoomSearchRequest request, Config config)
    {
        if (string.IsNullOrWhiteSpace(request.CheckInDate) || string.IsNullOrWhiteSpace(request.CheckOutDate))
        {
            return Results.BadRequest("CheckInDate and CheckOutDate are required fields.");
        }

        string query = $@"
            SELECT 
                R.rooms_id, 
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
            
            WHERE R.rooms_id NOT IN (
                SELECT DISTINCT RBB.rooms_id
                FROM rooms_by_booking AS RBB
                JOIN bookings AS B ON RBB.booking_id = B.booking_id
                WHERE B.Status = 'Confirmed'
                  AND B.Check_OUT > @CheckInDate 
                  AND B.Check_IN < @CheckOutDate
            )";

        var parameters = new List<MySqlParameter>
        {
            new("@CheckInDate", request.CheckInDate),
            new("@CheckOutDate", request.CheckOutDate)
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