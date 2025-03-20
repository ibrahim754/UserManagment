using UserManagement.DTOs;
using UserManagement.Models;

namespace UserManagement.Constans
{
    internal static class SeedUsersData
    {
        public static List<SeedingUserDto> Users => new()
        {
            new SeedingUserDto
            {
               user = new User
               {
                 UserName = "Adminstrator",
                 Email = "IbrahimSalman277@gmail.com",
                 FirstName = "Ibrahim",
                 LastName = "Salman"
               },
                password = "123456"
            }
        };
    }
}