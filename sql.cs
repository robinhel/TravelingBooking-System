
using MySql.Data.MySqlClient;
namespace server;


public static class Sql
{
    public static async Task db_reset_to_default(Config config)
    {
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, "DROP TABLE IF EXISTS rooms_by_booking");
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, "DROP TABLE IF EXISTS bookings");
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, "DROP TABLE IF EXISTS rooms");
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, "DROP TABLE IF EXISTS hotels");
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, "DROP TABLE IF EXISTS users");
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, "DROP TABLE IF EXISTS cities");
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, "DROP TABLE IF EXISTS countries");



        string countries_table = """ 
        CREATE TABLE countries( 
        countries_id INT PRIMARY KEY AUTO_INCREMENT,
        name VARCHAR(255)
        )
    """;

        string insert_countries = """
        INSERT INTO countries (name)
        VALUES 
        ('Sweden'),
        ('Norway'),
        ('Italy');
    """;
        //-------------------------------------------------------------------------------------------------
        string cities_table = """
        CREATE TABLE cities(
        city_id INT PRIMARY KEY AUTO_INCREMENT,
        countries_id INT NOT NULL,
        city_name VARCHAR(254) NOT NULL,
        food_name VARCHAR(254),
        food_description VARCHAR(254),
        FOREIGN KEY (countries_id) REFERENCES countries(countries_id)
        )
    """;

        string cities_insert = """
        INSERT INTO cities (countries_id, city_name, food_name)
        VALUES
        (1, 'Stockholm', 'Swedish meatballs'),
        (1, 'Gothenburg', 'Shrimp sandwich'),

        (2, 'Oslo', 'Mutton and Cabbage stew'),
        (2, 'Bergen', 'Bergen fish soup'),
        
        (3, 'Rome', 'Cheese and pepper pasta'),
        (3, 'Milan', 'Risotto alla Milanese');
    """;

        //-------------------------------------------------------------------------------------------------


        string users_table = """ 
        CREATE TABLE users (
            user_id INT PRIMARY KEY AUTO_INCREMENT,
            name VARCHAR(254) NOT NULL,
            email VARCHAR(254) NOT NULL UNIQUE,
            password VARCHAR(128) NOT NULL,
            Role ENUM ('Traveler','Admin') NOT NULL DEFAULT 'Traveler'
        )
    """;

        string insert_users = """
        INSERT INTO users (name, email, password, role)
        VALUES
        ('Test', 'test@hotmail.com', '123', 'Traveler'),
        ('Admin', 'admin@site.com', '123', 'Admin');
    """;


        //-------------------------------------------------------------------------------------------------

        string hotel_table = """ 
        CREATE TABLE Hotels (
            hotel_id INT PRIMARY KEY  AUTO_INCREMENT,
            name VARCHAR(254) NOT NULL,
            city_id INT NOT NULL,
            FOREIGN KEY (city_id) REFERENCES cities (city_id)
        )        
    """;


        string insert_hotels = """
        INSERT INTO hotels (name, city_id)
        VALUES
        
        ('Stockholm Hotel', 1),
        ('Gothenburg Hotel', 2),
        
        ('Oslo Hotel', 3),
        ('Bergen Hotel', 4),

        ('Rome Hotel', 5),
        ('Milan Hotel', 6)
    """;



        //-------------------------------------------------------------------------------------------------


        string rooms_table = """ 
        CREATE TABLE rooms(
            rooms_id INT PRIMARY KEY AUTO_INCREMENT,
            number INT NOT NULL,
            price INT NOT NULL,
            capacity INT NOT NULL,
            hotel_id INT NOT NULL,
            FOREIGN KEY (hotel_id) REFERENCES hotels(hotel_id)
        )
    """;

        string insert_rooms = """
        INSERT INTO rooms (number, price, capacity, hotel_id)
        VALUES
        (101, 120, 2, 1),
        (102, 150, 3, 1),

        (201, 110, 2, 2),
        (202, 160, 4, 2),

        (301, 130, 2, 3),
        (302, 180, 3, 3),

        (401, 140, 2, 4),
        (402, 220, 4, 4),

        (501, 200, 2, 5),
        (502, 260, 3, 5),

        (601, 240, 2, 6),
        (602, 300, 4, 6);
    """;
        //-------------------------------------------------------------------------------------------------

        string bookings_table = """
        CREATE TABLE bookings (
        booking_id INT PRIMARY KEY AUTO_INCREMENT,
        user_id INT NOT NULL,
        check_in DATE NOT NULL,
        check_out DATE NOT NULL,
        FOREIGN KEY (user_id) REFERENCES users(user_id)
)
""";

        string insert_bookings = """
        INSERT INTO bookings (user_id, check_in, check_out)
        VALUES
        (1, '2025-06-10', '2025-06-15'),
        (2, '2025-07-01', '2025-07-05');
        """;


        //-------------------------------------------------------------------------------------------------


        string rooms_by_booking_table = """
        CREATE TABLE rooms_by_booking (
            rooms_by_booking_id INT PRIMARY KEY AUTO_INCREMENT,
            booking_id INT NOT NULL,
            rooms_id INT NOT NULL,
            FOREIGN KEY (booking_id) REFERENCES bookings(booking_id),
            FOREIGN KEY (rooms_id) REFERENCES rooms(rooms_id)
        )
        """;
        string insert_rooms_by_booking = """
        INSERT INTO rooms_by_booking (booking_id, rooms_id)
        VALUES
        (1, 1),
        (1, 2),
        (2, 3);
    """;

        //-------------------------------------------------------------------------------------------------




        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, countries_table);
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, insert_countries);

        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, cities_table);
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, cities_insert);

        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, users_table);
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, insert_users);

        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, hotel_table);
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, insert_hotels);

        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, rooms_table);
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, insert_rooms);

        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, bookings_table);
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, insert_bookings);

        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, rooms_by_booking_table);
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, insert_rooms_by_booking);


    }
}