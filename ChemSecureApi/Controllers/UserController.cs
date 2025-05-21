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

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUser(string id)
        {
            var user = await _context.Users
                .Include(g => g.Tanks)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (user == null)
            {
                return NotFound("User was not found.");
            }
            var userDto = new UserDTO
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address
            };
            return Ok(userDto);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            var users = await _context.Users
                .Include(g => g.Tanks)
                .ToListAsync();
            var usersDTO = users.Select(user => new UserDTO
            {
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address
            }).ToList();
            return Ok(usersDTO);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}/tank")]
        public async Task<ActionResult<User>> GetUserTank(string id)
        {
            var user = await _context.Users
                .Include(g => g.Tanks)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (user == null)
            {
                return NotFound("User was not found.");
            }
           
            return Ok(user);
        }
    }
}
