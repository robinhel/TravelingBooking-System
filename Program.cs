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

//We want to have (Crud) of every endpoint (Create, Read, Update, Delete)

app.MapPost("/create/account", LoginHandler.CreateAccount);
app.MapPost("/login", LoginHandler.Login);
app.MapPost("/search", SearchHandler.SearchFoodAndGetHotels);


//countries
app.MapPost("/countries", Country.AddCountry);
app.MapGet("/countries", Country.GetCountry);
app.MapDelete("/countries/{id}", Country.DeleteCountry);

//cities
app.MapPost("/cities", City.AddCity);
app.MapGet("/cities", City.GetCityByCountry);
//app.MapDelete("/cities/{id}", City.DeleteCities);

//hotels
app.MapPost("/hotels", Hotel.AddHotel);
app.MapGet("/hotels", Hotel.GetHotelByCity);

//rooms
app.MapPost("/rooms", Rooms.AddRoom);
app.MapGet("/hotels/{hotelId}/rooms", Rooms.GetRooms);

// Profile
app.MapGet("/profile", Users.ViewProfile);
app.MapPut("/profile/update", UserHandler.UpdateProfile);
app.MapPut("/profile/changePassword", UserHandler.ChangePasswordRequest);

// Reset Database
app.MapDelete("/db", Sql.db_reset_to_default);

app.Run();

