



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