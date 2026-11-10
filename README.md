# RaceDay API – Event Management System

!ASP.NET Core !Entity Framework Core !SQL Server !Status

## Project Overview

RaceDay is an event management system developed for the PROG6212 Portfolio of Evidence (PoE) Part 2.

The application provides a platform where Event Organisers can create and manage sporting events, while Participants can browse events, register for activities and view their results.

The backend was developed using ASP.NET Core Web API, Entity Framework Core and Microsoft SQL Server.

The API follows RESTful principles and implements authentication, session management and role-based access control to ensure that users can only perform actions permitted by their roles.

## Technologies Used
C#
ASP.NET Core Web API
Entity Framework Core
Microsoft SQL Server
Swagger / OpenAPI
Data Annotations
ASP.NET Core Session Management
GitHub Actions (Continuous Integration)
Part 1: Design Corrections and Rationale

The original RaceDay Part 1 submission was reviewed against the assessment feedback and the PROG6212 PoE Part 2 guide. The database design, ERD and API Endpoint Plan were corrected to improve consistency and support the required functionality.

## Changes made
Completed the ERD: Added the Activity and Result entities so that the ERD represents the tables required by the database design.
Clarified primary and foreign keys: Identified the primary keys, foreign keys and unique constraints for each entity, together with relationship cardinalities.
Updated registration rules: The implemented design allows a Participant to register once per event, preventing duplicate registrations for the same event.
Improved referential integrity: Added a composite relationship between Registration and Activity so that an activity must belong to the event being entered.
Aligned API naming: Used activity-related controller functionality to manage event categories while retaining the Activity database entity.
Clarified organiser permissions: Organisers may create, update and delete only events they manage, and may manage activities, enrolments and results only for those events.
Corrected SQL Server compatibility: The database script uses Microsoft SQL Server-compatible syntax.
Aligned the planning documents: The ERD, SQL script and API Endpoint Plan were checked against the implemented database structure and API functionality.
Design decisions

The database retains separate Participant and EventOrganizer account tables. This preserves the existing design while allowing the API to apply role-specific access controls.

Passwords are hashed by the application before being stored.

The Activity entity represents the different race options or categories available within an event, such as 5km and 10km races.

These corrections are deliberate changes to the original Part 1 work. The updated documents serve as the specification for Part 2.

## Part 2: API Implementation
Database Models

The application uses the following nine models:

Model	Description
EventOrganizer	Stores organiser account information
Participant	Stores participant account information
Event	Stores sporting event information
EventLocation	Stores event venue and location details
Activity	Stores activities and race categories
Registration	Connects participants to events and activities
Result	Stores participant finishing results
RouteInfo	Stores event route information
Weather	Stores event weather information

Entity Framework Core is used to manage database relationships, foreign keys, unique constraints and database operations.

## Authentication and Session Management

RaceDay supports two user roles:

1. Participant

Participants can:

Register an account.
Log in and log out.
View and update their own profile.
View available events.
View event activities and categories.
Register for events.
View their own registrations.
View their personal results.
View event locations, routes and weather information.

2. Organiser

Organisers can:

Register an account.
Log in and log out.
View and update their own profile.
Create sporting events.
View their own events.
Update and delete their own events.
Manage event activities and categories.
View registrations for their events.
Update registration statuses.
Record participant results.
Manage event locations, routes and weather information.
Password Security

Passwords are never stored as plain text.

The API uses Microsoft's PasswordHasher to hash passwords before saving them to SQL Server.

During login, the supplied password is verified against the stored password hash.

Session Management

ASP.NET Core session management is used to identify authenticated users.

Two session values are maintained:

UserId: Identifies the logged-in user.
Role: Identifies whether the user is a Participant or Organiser.

The session is cleared when the user logs out.

Role-Based Access Control

The API checks the authenticated user's role before allowing access to protected functionality.

Organisers can only manage events and associated information belonging to them.

Participants can only access and manage their own registrations and personal results.

The API uses appropriate HTTP status codes, including:

Status Code	Meaning
200	OK
201	Created
400	Bad Request
401	Unauthorized
403	Forbidden
404	Not Found
409	Conflict
API Controllers and Endpoints

### 1. AuthController
Method	Endpoint	Description
POST	/api/auth/register	Register an account
POST	/api/auth/login	Log in
POST	/api/auth/logout	Log out
### 2. ProfileController
Method	Endpoint	Description
GET	/api/profile/me	View own profile
PUT	/api/profile/me	Update own profile
### 3. EventsController
Method	Endpoint	Description
GET	/api/events	View all events
GET	/api/events/{id}	View a specific event
GET	/api/events/mine	View own events
POST	/api/events	Create an event
PUT	/api/events/{id}	Update an event
DELETE	/api/events/{id}	Delete an event
### 4. ActivitiesController
Method	Endpoint	Description
GET	/api/events/{eventId}/categories	View event categories
POST	/api/events/{eventId}/categories	Create a category
PUT	/api/categories/{id}	Update a category
DELETE	/api/categories/{id}	Delete a category
### 5. RegistrationsController
Method	Endpoint	Description
POST	/api/enrolments	Register for an event
GET	/api/enrolments/mine	View own registrations
GET	/api/enrolments/event/{eventId}	View event registrations
PUT	/api/enrolments/{id}/status	Update registration status
### 6. ResultsController
Method	Endpoint	Description
POST	/api/results	Record a participant result
GET	/api/results/mine	View personal results
GET	/api/results/event/{eventId}	View event results
### 7. EventLocationsController
Method	Endpoint	Description
GET	/api/locations	View all locations
GET	/api/locations/{id}	View a location
POST	/api/locations	Create a location
PUT	/api/locations/{id}	Update a location
DELETE	/api/locations/{id}	Delete a location
### 8. RouteInfoController
Method	Endpoint	Description
GET	/api/routes/event/{eventId}	View event route
POST	/api/routes/event/{eventId}	Create event route
PUT	/api/routes/{id}	Update event route
DELETE	/api/routes/{id}	Delete event route
### 9. WeatherController
Method	Endpoint	Description
GET	/api/weather/event/{eventId}	View event weather
POST	/api/weather/event/{eventId}	Create weather information
PUT	/api/weather/{id}	Update weather information
DELETE	/api/weather/{id}	Delete weather information
Installation and Setup
Prerequisites

Before running the project, ensure that you have installed:

Visual Studio 2022 or a compatible development environment.
The .NET SDK version matching the project target framework.
Microsoft SQL Server.
SQL Server Management Studio (SSMS).
Git.
1. Clone the repository

Open a terminal and run:

bash
git clone YOUR_GITHUB_REPOSITORY_URL


Navigate to the API project directory:

bash
cd RaceDay.Api

2. Configure the database connection

Open appsettings.json and configure the SQL Server connection string.

Example:

json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=RaceDayDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}


Replace YOUR_SERVER_NAME with your SQL Server instance name.

For local development, TrustServerCertificate=True can be used where appropriate. Use proper certificate validation in production.

3. Restore NuGet packages

Run:

bash
dotnet restore

4. Build the project

Run:

bash
dotnet build

5. Update the database

If the project contains Entity Framework Core migrations, run:

bash
dotnet ef database update


If the EF Core CLI tool is not installed, install it using:

bash
dotnet tool install --global dotnet-ef


If no migrations exist yet, create an initial migration before updating the database:

bash
dotnet ef migrations add InitialCreate
dotnet ef database update

6. Run the application

Run:

bash
dotnet run


The API will start using the configured application URLs.

7. Access Swagger

When running in Development mode, open the Swagger URL displayed in the terminal.

Swagger provides an interactive interface for testing the API endpoints.

API Testing

Swagger was used to test the API functionality.

## Testing included:

Participant registration and login.
Organiser registration and login.
Profile retrieval and updates.
Event creation and management.
Activity and category management.
Participant event registration.
Registration status updates.
Result recording and retrieval.
Event location management.
Route information management.
Weather information management.
Role-based access restrictions.

The tests also checked appropriate HTTP status codes and validation responses.

Continuous Integration

A GitHub Actions Continuous Integration workflow is included in the repository.

The workflow builds the application to check for compilation errors when changes are pushed to the repository.

The successful CI build is represented by the green status screenshot included below.

!Successful GitHub Actions CI build

YouTube Demonstration

A video demonstration of the RaceDay API implementation is available below.

YouTube Video: [INSERT YOUR UNLISTED YOUTUBE VIDEO LINK HERE]

## The demonstration covers:

Database models and relationships.
Authentication and session management.
Role-based access control.
API controllers and endpoints.
Swagger testing.
Additional event functionality.
Project Structure
text
RaceDay.Api/
│
├── Controllers/
│   ├── AuthController.cs
│   ├── ProfileController.cs
│   ├── EventsController.cs
│   ├── ActivitiesController.cs
│   ├── RegistrationsController.cs
│   ├── ResultsController.cs
│   ├── EventLocationsController.cs
│   ├── RouteInfoController.cs
│   └── WeatherController.cs
│
├── Data/
│   └── RaceDayDbContext.cs
│
├── Models/
│   ├── Activity.cs
│   ├── Event.cs
│   ├── EventLocation.cs
│   ├── EventOrganizer.cs
│   ├── Participant.cs
│   ├── Registration.cs
│   ├── Result.cs
│   ├── RouteInfo.cs
│   └── Weather.cs
│
├── Migrations/
│
├── Properties/
│
├── appsettings.json
├── Program.cs
└── RaceDay.Api.csproj


## Conclusion

RaceDay demonstrates the implementation of a RESTful API for managing sporting events and participant registrations.

The project applies database design principles, Entity Framework Core relationships, password hashing, session management, role-based access control and RESTful API conventions.

The additional location, route and weather functionality further extends the capabilities of the application.

This project provided practical experience in backend development, database integration, API design, authentication and software testing.

Module: PROG6212
Assessment: Portfolio of Evidence Part 2
Project: RaceDay API

Youtube - https://youtu.be/Ll04i5GKFdc

