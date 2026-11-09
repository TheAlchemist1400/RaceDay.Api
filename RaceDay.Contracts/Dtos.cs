using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceDay.Contracts
{
    // AUTHENTICATION DTOs

    public class RegisterRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Passowrd { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    // PROFILE DTOs

    public class ProfileDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class UpdateProfileRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

    }

    // EVENT DTOs
    
    public class CreateEventRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;

    }

    public class UpdateEventRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Descriptn { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
    }

    public class EventDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; }
        public string Location { get; set; } = string.Empty;
    }

    // CATEGORY DTOs

    public class CategoryRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }

    // ENROLMENT DTOs
    public class CreateEnrolmentRequest
    {
        public int EventId { get; set; }
        public int categoryId { get; set; }
    }

    public class UpdateEnrolmentStatusRequest
    {
        public string Status { get; set; } = string.Empty;
    }

    public class EnrolmentDto
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public int Category { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // RESULT DTOs
    public class ResultRequest
    {

        public int EnrolmentId { get; set; }
        public int Position { get; set; }
        public decimal Time { get; set; }
    }

    public class ResultDto
    {
        public int Id { get; set; }
        public int EnrolmentId { get; set; }
        public decimal Time { get; set; }
        public int Position { get; set; }
    }

    // IMAGEUPLOAD DTA
    public class UploadResponse
    {
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
    }
}

