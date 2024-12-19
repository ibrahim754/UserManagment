﻿using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.DTOs
{
    public class RegisterModel
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string Username { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public IFormFile? Image { get; set; }

        public string Password { get; set; }
    }
}
