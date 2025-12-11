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


    public static async Task<IResult> SearchFoodAndGetHotels(string food, Config config)
    {
        string search = food ?? "";

        string query = @"
        SELECT
        h.hotel_id,
        h.name AS HotelName,
        c.food_name,
        c.food_description,
        c.city_name AS CityName
        FROM cities as C
        Join Hotels as H on c.city_id = h.city_id
        WHERE c.food_name LIKE CONCAT('%', @SearchTerm, '%')
        ";

        var parameters = new MySqlParameter[]
        {
            new("@SearchTerm", search)
        };

        var results = new List<CityFoodResult>();

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
                        results.Add(new CityFoodResult(
                            reader.GetInt32(0),
                            reader.GetString(1),
                            reader.GetString(2),
                            reader.GetString(3),
                            reader.GetString(4)
                        ));
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

}