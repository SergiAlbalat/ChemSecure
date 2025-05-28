using ChemSecureApi.Data;
using ChemSecureApi.DTOs;
using ChemSecureApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace ChemSecureApi.Controllers
{
    [Route("api/User")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        public UserController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user to retrieve. Cannot be null or empty.</param>
        /// <returns>The user information</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(string id)
        {
            User user = null;
            try
            {
                 user = await _context.Users
                    .Include(u => u.Tanks )
                    .FirstOrDefaultAsync(g => g.Id == id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


            if (user == null)
            {
                return NotFound("User was not found.");
            }
            var userDto = new UserDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address
            };
            return Ok(userDto);
        }

        /// <summary>
        /// Retrieves a list of users with their associated details.
        /// </summary>
        /// <returns>All the users in the database</returns>
        [Authorize(Roles = "Admin")]
        [HttpGet("")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            var users = await _context.Users
                .Include(g => g.Tanks)
                .ToListAsync();
            var usersDTO = users.Select(user => new UserDTO
            {
                Id=user.Id,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address
            }).ToList();
            return Ok(usersDTO);
        }
    }
}
