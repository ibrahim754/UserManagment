using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.ComponentModel.DataAnnotations;

namespace UserManagement.DTOs
{
    public class TokenRequestModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
