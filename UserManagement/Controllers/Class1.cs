using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserManagement.Constans;
using UserManagement.Extensions;
using UserManagement.Interfaces;

namespace UserManagement.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Class1 (IRoleService roleService,RoleManager<IdentityRole> roleManager):ControllerBase
    {
        private UserPermissions UserPermissions { get; set; }
        [HttpGet]
        public async Task< IActionResult> TestEndpoint()
        {
            var role = await roleService.IsExistAsync(DefaultRoles.Adminstrator.ToString());
            if (role.IsError)
            {
                return BadRequest("Error Occurred!!");
            }

            await roleManager.AddPermissionClaim(role.Value);
            return Ok();
        }


    }
}
