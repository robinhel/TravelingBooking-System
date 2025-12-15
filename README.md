



## starta projektet

Kör:

dotnet run

Servern körs sedan på http://localhost:5000

ändra 5000 till det som din server körs på. Ex: 5077



## Funktioner

### Återställ databas

Metod: DELETE

URL: /db

Den bygger tabeller för users, countries, cities, hotels, rooms, bookings och rooms_by_booking. 
Den lägger även in en testanvändare. Detta gör arbetet enklare under utveckling.


### Skapa konto

Metod: POST
URL: /create/account

Body raw JSON:

{

    "name": "Test",

    "email": "test@test.com",

    "password": "123"
}


### Logga in

Metod: POST
URL: /login

Body raw JSON:

{ 

    "email": "test@test.com",

    "password": "123"
}


### View profile

Metod: GET 
URL: /profile 


### hämta Countries
Metod: GET
URL: /countries
 - Displays all the countries available in the database.


### lägga till Countries
Must login as admin {"email": "admin@site.com", "password": "123"}
Metod: POST
URL: /countries

Body raw JSON:
{
    "Name": "Germany"
}  

### hämta cities
Metod: GET
URL: /cities/{countryId}    // countryId: 1 = Sweden; 2 = Norway; 3 = Italy
 - Tex. /cities/1 --> displays all the cities in Sweden and their food.


### lägga till cities
Must login as admin
Metod: POST
URL: /cities

Body raw JSON:
{
    "CountryId": 4,
    "CityName": "Munich",
    "FoodName": "Schnitzel"
} 

### hämta hotels
Metod: GET
URL: /hotels/{cityId}
Tex. /hotels/1 --> display all the hotels in Stockholm (cityId = 1).


### lägga till hotels
Must login as admin 
Metod: POST
URL: /hotels

Body raw JSON:
{
    "Name": "Lizz Hotel Munich",
    "CityId": 7
} 

### hämta rooms
Metod: GET
URL: /rooms/{hotelId}
Tex. /rooms/1 --> display all the rooms in the Stockholm Hotel (hotelId = 1).


### lägga till rooms
Must login as admin 
Metod: POST
URL: /rooms

Body raw JSON:
{
    "HotelId": 8,
    "RoomNumber": 107
    "RoomCapacity": 2
    "Price": 150
} 

### Update profile (email, username, password)

Metod: PUT
URL: /profile/update 

Body raw JSON:
{

    "name" : ".....",
    "email" : ".....",
    "password" : "....",
    "ConfirmPassword" : "...."

}


### UPDATE password(password)

Metod: PUT 
URL: /profile/changePassword 

Body raw JSON: 
{

    "oldPassword" : "GAMLA LÖSENORD (123)",
    "newPassword" : "NYTT LÖSENORD",
    "ConfirmPassword" : "CONFIRMA DITT LÖSENORD"

}

### DELETE country(AS A ADMIN)

(Hämta countries genom att ändra METOD: GET, URL: /countries. För att enklare se vilka ID:s alla länder har. )


Metod: DELETE
URL: /countries/{id} (Skriv in det ID landet har som du vill ta bort)


### Find hotels by searching for different food names

Metod: POST
URL: /search/food

Body raw JSON: 
{
    "food" = "Swe"
}

Will give you information about the hotel and country that serves Swedish Meatballs.

### Find available rooms by date

Metod: POST
URL: /search/rooms

Body raw JSON: 
{
    {
    "CheckInDate": "2025-10-01",
    "CheckOutDate": "2025-10-05"
    }
}

Its also possible to search for specific Countries or Hotels between the dates.

{
    "CheckInDate": "2025-06-10",
    "CheckOutDate": "2025-06-12",
    "CountryName": "input",
    "CityName": "input",
    "HotelName": "input"
}


