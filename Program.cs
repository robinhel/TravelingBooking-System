using MySql.Data.MySqlClient;
using server;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);

// config
Config config = new(
    "server=127.0.0.1;database=Traveling_Booking_System;uid=Traveling_Booking_System;password=Traveling_Booking_System;"
);
builder.Services.AddSingleton(config);

// sessionstjänster
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// aktivera session
app.UseSession();

app.MapDelete("/db", Sql.db_reset_to_default);
app.MapPost("/create/account", LoginHandler.CreateAccount);
app.MapPost("/login", LoginHandler.Login);

app.Run();

async Task db_reset_to_default(Config config)
{
    await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, "DROP TABLE IF EXISTS users");

    string users_table = """ 
        CREATE TABLE Users (
            id INT PRIMARY KEY AUTO_INCREMENT,
            name VARCHAR(254) NOT NULL,
            email VARCHAR(254) NOT NULL UNIQUE,
            password VARCHAR(128) NOT NULL
        )
    """;

    await MySqlHelper.ExecuteNonQueryAsync(config.connectionString, users_table);
}





