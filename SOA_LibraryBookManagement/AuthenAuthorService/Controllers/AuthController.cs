using AuthenAuthorService.Data;
using AuthenAuthorService.Models;
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
                Data = new {
                    IdUser = user.Id,
                    Username = user.UserName,
                    AccessToken = accessToken,
                    Expiration = expiration,
                    Roles = userRoles
                }
            });
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
