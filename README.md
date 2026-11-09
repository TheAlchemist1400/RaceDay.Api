# RaceDay API – PROG6212 PoE Part 2 Presentation Script
1. Introduction

"Hello everyone, and welcome to my PROG6212 Portfolio of Evidence Part 2 presentation.

In this video, I will be demonstrating my RaceDay application, focusing on the backend RESTful API that I developed using ASP.NET Core, Entity Framework Core and Microsoft SQL Server.

RaceDay is an event management system designed to allow event organisers to create and manage sporting events, while participants can register for events, select activities and view their results.

I will explain the database structure, the models, authentication, session management, role-based access control, the API controllers and how I tested the endpoints using Swagger."

2. Database design and models

"Before implementing the API, I worked on the database design.

The application uses Microsoft SQL Server as its database, and Entity Framework Core to communicate with the database.

I created separate models to represent the different entities in the system.

These include:

EventOrganizer
Participant
Event
EventLocation
Activity
Registration
Result
RouteInfo
Weather

Each model represents a table in the database.

For example, the Event model stores information such as the event name, date, maximum participants, organiser and location.

The Activity model represents the different activities participants can enter, such as a 5-kilometre or 10-kilometre race.

The Registration model connects a participant to an event and the selected activity.

The Result model stores information such as finish time, finishing position and result status.

I also used data annotations such as Required, StringLength and Range to validate the information being stored.

In my DbContext, I configured relationships, foreign keys, unique constraints and decimal precision using Entity Framework Core.

For example, I made sure that an activity belongs to an event and that a registration cannot reference an activity belonging to a different event.

I also used DeleteBehavior.Restrict to protect related records from being accidentally deleted."

3. Authentication and registration

"Next, I implemented authentication.

RaceDay supports two account types: Participants and Organisers.

I created an AuthController with three main endpoints:

POST api/auth/register
POST api/auth/login
POST api/auth/logout

During registration, the API receives the user's details and selected role.

It checks whether the email address already exists in either the Participant or EventOrganizer table.

The password is never stored as plain text.

Instead, I use Microsoft's PasswordHasher to generate a password hash before saving the account to SQL Server.

During login, the API finds the account using the email address and verifies the supplied password against the stored hash.

If the credentials are correct, the user is logged in successfully."

4. Session management

"After a successful login, I use ASP.NET Core session management to remember who is making requests to the API.

I store two important values in the session.

The first is UserId, which identifies the logged-in user.

The second is Role, which identifies whether the user is a Participant or an Organiser.

For example, a Participant might have UserId equal to 5 and Role equal to Participant.

This information allows the API to identify the user when they access protected endpoints.

When the user logs out, the session is cleared, preventing them from accessing protected functionality using that session."

5. Role-based access control

"An important part of RaceDay is role-based access control.

Not every user should have permission to perform every action.

Participants and Organisers have different responsibilities.

Both roles can view available events and relevant event information.

However, only Organisers can create events.

An Organiser can also update or delete only the events that they own.

Participants cannot create, update or delete events.

Similarly, only Participants can register for events and view their personal registrations.

Organisers can view registrations belonging to their own events and update their registration statuses.

I implemented these checks inside the controllers by retrieving the UserId and Role from the session.

I use HTTP status codes such as 401 Unauthorized when a user is not logged in and 403 Forbidden when a logged-in user does not have the required permissions."

6. Profile management

"I also created a ProfileController.

This controller allows both Participants and Organisers to view and update their own profiles.

It uses two endpoints:

GET api/profile/me
PUT api/profile/me

An important security decision here is that I do not allow users to provide another user's ID to update their profile.

Instead, the API retrieves the UserId from the session.

This ensures that users can only manage their own account information."

7. Event management

"Next, I implemented the EventsController.

This controller manages the creation, viewing, updating and deletion of events.

Both Participants and Organisers can view available events.

However, only Organisers can create events.

When an Organiser creates an event, the API automatically obtains the OrganizerId from the session rather than trusting an ID supplied by the frontend.

When updating or deleting an event, the API checks whether the logged-in Organiser owns that event.

This prevents one Organiser from modifying another Organiser's events."

8. Activity management

"I then created the ActivitiesController.

In my database design, I use the Activity model to represent the different categories or race options available within an event.

For example, an event might offer a 5-kilometre race and a 10-kilometre race.

Each activity stores its name, distance, entry fee, start time and maximum number of entrants.

Both roles can view activities.

However, only the Organiser who owns the event can create, update or delete its activities."

9. Participant registrations

"The RegistrationsController handles event enrolments.

When a Participant wants to enter an event, they select an event and one of its available activities.

The API checks that the event exists and that the selected activity belongs to that event.

It also checks whether the Participant has already registered for the event.

I implemented checks for the maximum number of participants allowed in an event and the maximum number of entrants allowed in an activity.

The API also generates a bib number for the registration.

Participants can view their own registrations, while Organisers can view registrations for their own events and update registration statuses.

Some of the registration statuses include Pending, Confirmed and Cancelled."

10. Results management

"After an event has taken place, the Organiser can record participant results using the ResultsController.

The result contains the registration ID, finish time, finishing position and result status.

Before recording a result, the API checks that the registration exists and that the logged-in Organiser owns the event.

It also prevents duplicate results for the same registration.

Participants can then retrieve their personal results, while Organisers can view results for their own events."

11. Additional functionality

"I also implemented three additional controllers using the remaining models in my database.

The EventLocationsController manages event venues and locations.

The RouteInfoController manages information about event routes, including the distance, start point, end point and route description.

Finally, the WeatherController manages weather information associated with an event, including temperature, weather conditions and wind speed.

These features extend the functionality of RaceDay beyond the core event registration requirements."

12. API testing using Swagger

"To test the API, I used Swagger.

Swagger allowed me to send HTTP requests to my endpoints and inspect the responses.

I tested registration and login for both Participants and Organisers.

I also tested protected endpoints to make sure users could only perform actions permitted by their roles.

For example, I tested that a Participant could not create an event and that an Organiser could not register as a Participant.

I also tested event creation, activity creation, participant registration, registration status updates and result recording.

I checked the HTTP status codes returned by the API, including 200, 201, 400, 401, 403, 404 and 409 where appropriate.

This helped me verify that the API responds correctly to successful requests and invalid operations."

13. Continuous Integration

"I also included a Continuous Integration workflow in my project.

The workflow automatically builds the application when changes are pushed to the repository.

This helps identify build errors and ensures that the project can compile successfully.

The successful workflow execution is represented by the green CI status displayed in my README."

14. Conclusion

"To conclude, I successfully developed the RaceDay RESTful API using ASP.NET Core, Entity Framework Core and Microsoft SQL Server.

The application supports two different user roles, secure password hashing, session management, role-based access control, event management, activity management, participant registrations and results management.

I also implemented additional functionality for event locations, routes and weather information.

Through this project, I gained practical experience in developing RESTful APIs, working with relational databases, implementing authentication and authorisation, and testing API endpoints.

Thank you for watching my presentation."
=
