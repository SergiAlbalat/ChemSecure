using ChemSecureApi.Data;
using ChemSecureApi.DTOs;
using ChemSecureApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChemSecureApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        public AuthController(UserManager<User> userManager, IConfiguration configuration, AppDbContext context)
        {
            _userManager = userManager;
            _configuration = configuration;
            _context = context;
        }

        /// <summary>
        /// Method for registering a new user
        /// </summary>
        /// <param name="userDTO">The data for the new entry</param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO userDTO)
        {
            var user = new User { UserName = userDTO.Name, Email = userDTO.Email, PhoneNumber = userDTO.Phone, Address = userDTO.Address };
            var result = await _userManager.CreateAsync(user, userDTO.Password);
            if (result.Succeeded)
            {
                return Ok("User registered");
            }
            return BadRequest(result.Errors);
        }

        /// <summary>
        /// Method for logging in a user
        /// </summary>
        /// <param name="userDTO">The data that the user have input for log in</param>
        /// <returns>A token with the user session</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO userDTO)
        {
            var user = await _userManager.FindByEmailAsync(userDTO.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, userDTO.Password))
            {
                return Unauthorized("Invalid email or password");
            }
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };
            var roles = await _userManager.GetRolesAsync(user);
            if (roles != null && roles.Count > 0)
            {
                foreach (var rol in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, rol));
                }
            }
            var token = CreateToken(claims.ToArray());
            return Ok(token);
        }

        /// <summary>
        /// Method for creating a JWT token
        /// </summary>
        /// <param name="claims">The claims of the new token</param>
        /// <returns>The token in string format</returns>
        private string CreateToken(Claim[] claims)
        {
            var jwtConfig = _configuration.GetSection("JwtSettings");
            var secretKey = jwtConfig["Key"];
            var issuer = jwtConfig["Issuer"];
            var audience = jwtConfig["Audience"];
            var expirationMinutes = int.Parse(jwtConfig["ExpirationMinutes"]);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(expirationMinutes),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Method for registering a new admin
        /// </summary>
        /// <param name="userDTO">The data for the new entry</param>
        /// <returns></returns>
        [HttpPost("admin/register")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterDTO userDTO)
        {
            var user = new User { UserName = userDTO.Name, Email = userDTO.Email, PhoneNumber = userDTO.Phone, Address = userDTO.Address };
            var result = await _userManager.CreateAsync(user, userDTO.Password);
            var roleResult = new IdentityResult();
            if (result.Succeeded)
            {
                roleResult = await _userManager.AddToRoleAsync(user, "Admin");
            }
            if (result.Succeeded && roleResult.Succeeded)
            {
                return Ok("Admin registered");
            }
            return BadRequest(result.Errors);
        }

        /// <summary>
        /// Method for registering a new manager
        /// </summary>
        /// <param name="userDTO">The new manager information</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
        [HttpPost("manager/register")]
        public async Task<IActionResult> RegisterManager([FromBody] RegisterDTO userDTO)
        {
            var user = new User { UserName = userDTO.Name, Email = userDTO.Email, PhoneNumber = userDTO.Phone, Address = userDTO.Address };
            var result = await _userManager.CreateAsync(user, userDTO.Password);
            var roleResult = new IdentityResult();
            if (result.Succeeded)
            {
                roleResult = await _userManager.AddToRoleAsync(user, "Manager");
            }
            if (result.Succeeded && roleResult.Succeeded)
            {
                return Ok("Manager registered");
            }
            return BadRequest(result.Errors);
        }
    }
}
