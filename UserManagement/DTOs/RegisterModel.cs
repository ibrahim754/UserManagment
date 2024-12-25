using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.DTOs
{
    public class RegisterModel
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string Username { get; set; }
        public string Email { get; set; }
        public IFormFile? Image { get; set; }

        public string Password { get; set; }
    }
}
