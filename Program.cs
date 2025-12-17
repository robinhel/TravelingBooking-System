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
app.MapPost("/logout", Logout.LogoutCookie);
app.MapPost("/search/food", SearchHandler.SearchFoodAndGetHotels);

//booking
app.MapPost("/booking", BookingHandler.CreateBooking);
app.MapGet("/my/bookings", UserBooking.GetMyBookings);


//countries
app.MapPost("/countries", Country.AddCountry);
app.MapGet("/countries", Country.GetCountry);

//cities
app.MapPost("/cities", City.AddCity);
app.MapGet("/cities/{countryId}", City.GetCityByCountry);

//hotels
app.MapPost("/hotels", Hotel.AddHotel);
app.MapGet("/hotels/{cityId}", Hotel.GetHotelByCity);
app.MapDelete("/hotels/{id}", Hotel.DeleteHotel);

//rooms
app.MapPost("/rooms", Rooms.AddRoom);
app.MapGet("/hotels/{hotelId}/rooms", Rooms.GetRooms);
app.MapPost("/search/rooms", SearchHandler.SearchAvailableRooms);
app.MapDelete("/rooms/{id}", Rooms.DeleteRoom);

// Profile
app.MapGet("/profile", Users.ViewProfile);
app.MapPut("/profile/update", UserHandler.UpdateProfile);
app.MapPut("/profile/changePassword", UserHandler.ChangePasswordRequest);

// Reset Database
app.MapDelete("/db", Sql.db_reset_to_default);

app.Run();

