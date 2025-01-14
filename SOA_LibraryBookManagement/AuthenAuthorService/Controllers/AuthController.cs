using AuthenAuthorService.Common;
using AuthenAuthorService.Data;
using AuthenAuthorService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AuthenAuthorService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet]
        [Route("get-all-users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = userManager.Users.ToList();
            if (!users.Any())
                return NotFound(new Response { Status = "Fail", Message = "No users found!" });

            var userList = new List<object>();
            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                userList.Add(new
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Roles = roles
                });
            }

            return Ok(new Response
            {
                Status = "Success",
                Message = "User list retrieved successfully!",
                Data = userList
            });
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpGet]
        [Route("search-user/{username}")]
        public async Task<IActionResult> SearchUser(string username)
        {
            if (string.IsNullOrEmpty(username))
                return BadRequest(new Response { Status = "Fail", Message = "Username must not be null or empty!" });

            var users = userManager.Users
                .Where(u => u.UserName.Contains(username))
                .ToList();

            if (!users.Any())
                return NotFound(new Response { Status = "Fail", Message = "No users found matching the search criteria!" });

            var userList = new List<object>();
            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                userList.Add(new
                {
                    Id = user.Id,
                    Username = user.UserName,
                    Email = user.Email,
                    Roles = roles
                });
            }

            return Ok(new Response
            {
                Status = "Success",
                Message = "Users found successfully!",
                Data = userList
            });
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
                return BadRequest(new Response { Status = "Fail", Message = "Your email and password must not be null!" });

            var user = await GetUserByName(model.Username);
            if (user == null) return BadRequest(new Response { Status = "Fail", Message = "This UserName don't have in system!" });

            bool isPasswordValid = await userManager.CheckPasswordAsync(user, model.Password);
            if (!isPasswordValid) return BadRequest(new Response { Status = "Fail", Message = "The password is incorrect!" });

            var userRoles = await userManager.GetRolesAsync(user);

            var (accessToken, expiration) = await GenerateTokenAsync(user, userRoles);
            return Ok(new Response
            {
                Status = "Success",
                Message = "Login successfully!",
                Data = new
                {
                    IdUser = user.Id,
                    Username = user.UserName,
                    AccessToken = accessToken,
                    Expiration = expiration,
                    Roles = userRoles
                }
            });
        }

        [Authorize(Roles=UserRoles.Admin)]
        [HttpPost]
        [Route("add-user")]
        public async Task<IActionResult> AddUser([FromBody] RegisterModel model)
        {
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password) 
                || string.IsNullOrEmpty(model.Role) || string.IsNullOrEmpty(model.Email))
            {
                return BadRequest(new Response 
                { 
                    Status = "Fail", 
                    Message = "Enter full information!" 
                });
            }

            var existingUser = await userManager.FindByNameAsync(model.Username);
            if (existingUser != null)
            {
                return BadRequest(new Response 
                { 
                    Status = "Fail", 
                    Message = "This username already exists!" 
                });
            }

            var user = new User
            {
                UserName = model.Username,
                Email = model.Email
            };

            var result = await userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return BadRequest(new Response 
                {
                    Status = "Fail", 
                    Message = string.Join(", ", result.Errors.Select(e => e.Description)) 
                });
            }    

            if (!await roleManager.RoleExistsAsync(model.Role))
            {
                return BadRequest(new Response
                {
                    Status = "Fail",
                    Message = "Role is not exist in this system!"
                });
            }    

            await userManager.AddToRoleAsync(user, model.Role);

            return Ok(new Response { Status = "Success", Message = "User created successfully!" });
        }

        [Authorize]
        [HttpPost]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            // Lấy thông tin jti từ token
            var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrEmpty(jti))
            {
                return BadRequest(new Response { Status = "Fail", Message = "Invalid token!" });
            }

            // Lưu jti vào danh sách đen (có thể dùng Redis hoặc database)
            // Ví dụ:
            // await _cache.SetAsync(jti, true, TimeSpan.FromHours(6));

            return Ok(new Response { Status = "Success", Message = "User logged out successfully!" });
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpPut]
        [Route("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] LoginModel model)
        {
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password))
                return BadRequest(new Response { Status = "Fail", Message = "Username and new password must not be null!" });

            var user = await userManager.FindByNameAsync(model.Username);
            if (user == null)
                return BadRequest(new Response { Status = "Fail", Message = "User does not exist!" });

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, model.Password);
            if (!result.Succeeded)
                return BadRequest(new Response { Status = "Fail", Message = string.Join(", ", result.Errors.Select(e => e.Description)) });

            return Ok(new Response { Status = "Success", Message = "Password updated successfully!" });
        }

        [Authorize(Roles = UserRoles.Admin)]
        [HttpDelete]
        [Route("delete-user/{username}")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            var user = await userManager.FindByNameAsync(username);
            if (user == null)
                return BadRequest(new Response { Status = "Fail", Message = "User does not exist!" });

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(new Response { Status = "Fail", Message = string.Join(", ", result.Errors.Select(e => e.Description)) });

            return Ok(new Response { Status = "Success", Message = "User deleted successfully!" });
        }

        private async Task<(string Token, DateTime Expiration)> GenerateTokenAsync(User? user, IList<string> userRoles)
        {
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(6),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );
            var valid = token.ValidTo;
            return (new JwtSecurityTokenHandler().WriteToken(token), valid);
        }

        private async Task<User?> GetUserByName(string userName) => await userManager.FindByNameAsync(userName);
    }
}
