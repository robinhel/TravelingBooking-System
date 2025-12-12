



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


### lägga till Countires

Metod: POST
URL: /countries

Body raw JSON:
{


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


### DELETE cities(AS A ADMIN)