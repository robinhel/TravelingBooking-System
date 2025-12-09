using MySql.Data.MySqlClient;
namespace server;


public static class Sql
{
    public static async Task db_reset_to_default(Config config)
    {
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, "DROP TABLE IF EXISTS countries");
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, "DROP TABLE IF EXISTS cities");
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, "DROP TABLE IF EXISTS users");
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, "DROP TABLE IF EXISTS hotels");
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, "DROP TABLE IF EXISTS rooms");
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, "DROP TABLE IF EXISTS bookings");
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, "DROP TABLE IF EXISTS rooms_by_booking");


        string countries_table = """ 
        CREATE TABLE countries( 
        countries_id INT PRIMARY KEY AUTO_INCREMENT,
        name VARCHAR(255)
        )
    """;


        string cities_table = """
        CREATE TABLE cities(
        city_id INT PRIMARY KEY AUTO_INCREMENT,
        countries_id INT NOT NULL,
        FOREIGN KEY (countries_id) REFERENCES countries(countries_id)
        )
    """;


        string users_table = """ 
        CREATE TABLE users (
            user_id INT PRIMARY KEY AUTO_INCREMENT,
            name VARCHAR(254) NOT NULL,
            email VARCHAR(254) NOT NULL UNIQUE,
            password VARCHAR(128) NOT NULL,
            Role ENUM ('Traveler','Admin') NOT NULL DEFAULT 'Traveler'
        )
    """;

        string hotel_table = """ 
        CREATE TABLE Hotels (
            hotel_id INT PRIMARY KEY  AUTO_INCREMENT,
            name VARCHAR(254) NOT NULL,
            city_id INT,
            FOREIGN KEY (city_id) REFERENCES cities (city_id)
        )        
    """;


        string insert_test_user = """
        INSERT INTO users(name, email, password, role)
        VALUES ('test', 'test@hotmail.com', '123', 'Traveler')
    """;


        string rooms_table = """ 
        CREATE TABLE rooms(
        rooms_id INT PRIMARY KEY AUTO_INCREMENT,
        number INT NOT NULL,
        Price INT NOT NULL,
        capacity INT NOT NULL,
        hotel_id INT NOT NULL,
        FOREIGN KEY (hotel_id) REFERENCES hotels(hotel_id)
        )
    """;

        string bookings_table = """ 
        CREATE TABLE bookings (
        booking_id INT PRIMARY KEY AUTO_INCREMENT,
        user_id INT NOT NULL ,
        Check_IN DATE NOT NULL,
        Check_OUT DATE NOT NULL,
        FOREIGN KEY (user_id) REFERENCES users(user_id),
        Status ENUM ('Pending','Confirmed','Cancelled') NOT NULL DEFAULT 'Pending'     
        )   
    """;


        string rooms_by_booking_table = """ 
        CREATE TABLE rooms_by_booking(
        rooms_by_booking_id INT PRIMARY KEY AUTO_INCREMENT,
        booking_id INT,
        rooms_id INT,
        FOREIGN KEY (booking_id) REFERENCES bookings (booking_id),
        FOREIGN KEY (rooms_id) REFERENCES rooms (rooms_id)
        )
    """;


        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, countries_table);
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, cities_table);
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, users_table);
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, hotel_table);
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, rooms_table);
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, bookings_table);
        await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, rooms_by_booking_table);

    }
}
/* public static async Task db_reset_to_default(Config config)
{
    // DROP-satser i omvänd ordning (från barn till förälder)
    await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, "SET FOREIGN_KEY_CHECKS = 0;");
    await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, "DROP TABLE IF EXISTS rooms_by_booking");
    await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, "DROP TABLE IF EXISTS bookings");
    await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, "DROP TABLE IF EXISTS rooms");
    await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, "DROP TABLE IF EXISTS Hotels");
    await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, "DROP TABLE IF EXISTS Users");
    await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, "DROP TABLE IF EXISTS cities");
    await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, "DROP TABLE IF EXISTS countries");
    await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, "SET FOREIGN_KEY_CHECKS = 1;");


    // 1. Skapa countries (Förälder till cities)
    string countries_table = """ 
        CREATE TABLE countries( 
            countries_id INT PRIMARY KEY AUTO_INCREMENT,
            name VARCHAR(255)
        )
    """;
    await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, countries_table);

    // 2. Skapa cities (Förälder till Hotels)
    string cities_table = """
        CREATE TABLE cities(
            city_id INT PRIMARY KEY AUTO_INCREMENT,
            countries_id INT NOT NULL,
            FOREIGN KEY (countries_id) REFERENCES countries(countries_id)
        )
    """;
    await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, cities_table);

    // 3. Skapa Users (Förälder till bookings)
    string users_table = """ 
        CREATE TABLE Users (
            id INT PRIMARY KEY AUTO_INCREMENT,
            name VARCHAR(254) NOT NULL,
            email VARCHAR(254) NOT NULL UNIQUE,
            password VARCHAR(128) NOT NULL,
            Role ENUM ('Traveler','Admin') NOT NULL DEFAULT 'Traveler'
        )
    """;
    await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, users_table);

    // 4. Skapa Hotels (Refererar till cities)
    string hotel_table = """ 
        CREATE TABLE Hotels (
            hotel_id INT PRIMARY KEY AUTO_INCREMENT,
            name VARCHAR(254) NOT NULL,
            city_id INT,
            FOREIGN KEY (city_id) REFERENCES cities (city_id)
        )        
    """;
    await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, hotel_table);

    // 5. Skapa rooms (Refererar till Hotels)
    string rooms_table = """ 
        CREATE TABLE rooms(
            rooms_id INT PRIMARY KEY AUTO_INCREMENT,
            number INT NOT NULL,
            Price INT NOT NULL,
            capacity INT NOT NULL,
            hotel_id INT NOT NULL,
            FOREIGN KEY (hotel_id) REFERENCES Hotels(hotel_id)
        )
    """;
    await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, rooms_table);

    // 6. Skapa bookings (Refererar till Users)
    string bookings_table = """ 
        CREATE TABLE bookings (
            booking_id INT PRIMARY KEY AUTO_INCREMENT,
            user_id INT NOT NULL ,
            Check_IN DATE NOT NULL,
            Check_OUT DATE NOT NULL,
            FOREIGN KEY (user_id) REFERENCES Users(id),
            Status ENUM ('Pending','Confirmed','Cancelled') NOT NULL DEFAULT 'Pending'     
        )   
    """;
    await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, bookings_table);

    // 7. Skapa rooms_by_booking (Refererar till bookings och rooms)
    string rooms_by_booking_table = """ 
        CREATE TABLE rooms_by_booking(
            rooms_by_booking_id INT PRIMARY KEY AUTO_INCREMENT,
            booking_id INT,
            rooms_id INT,
            FOREIGN KEY (booking_id) REFERENCES bookings (booking_id),
            FOREIGN KEY (rooms_id) REFERENCES rooms (rooms_id)
        )
    """;
    await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, rooms_by_booking_table);
    
    // Test-användare
    string insert_test_user = """
        INSERT INTO Users(name, email, password, role)
        VALUES ('test', 'test@hotmail.com', '123', 'Traveler')
    """;
    await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, insert_test_user);
} */