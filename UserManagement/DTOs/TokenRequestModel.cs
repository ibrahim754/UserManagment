﻿using System.ComponentModel.DataAnnotations;

namespace UserManagement.DTOs
{
    public class TokenRequestModel
    {
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
