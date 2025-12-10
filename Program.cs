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
app.MapGet("/profile", Users.ViewProfile);
app.MapPost("/login", LoginHandler.Login);
app.MapPut("/profile/update", UserHandler.UpdateProfile);
app.MapPost("/countries", Country.AddCountry);
app.MapGet("/countries", Country.GetCountry);

app.Run();

