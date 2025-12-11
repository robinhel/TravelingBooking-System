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

}