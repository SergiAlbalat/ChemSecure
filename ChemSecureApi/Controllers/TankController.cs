using ChemSecureApi.Data;
using ChemSecureApi.DTOs;
using ChemSecureApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ChemSecureApi.Controllers
{
    [Route("api/Tank")]
    [ApiController]
    public class TankController : Controller
    {
        private readonly AppDbContext _context;
        public TankController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("")]
        public async Task<ActionResult<IEnumerable<Tank>>> GetTanks()
        {
            var tanks = await _context.Tanks
                                  .Include(g => g.Client)
                                  .ToListAsync();
            var tanksDTO = tanks.Select(tank => new TankGetDTO
            {
                Id = tank.Id,
                Capacity = tank.Capacity,
                CurrentVolume = tank.CurrentVolume,
                Type = tank.Type,
            }).ToList();

            return Ok(tanksDTO);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<ActionResult<Tank>> GetTank(int id)
        {
            var tank = await _context.Tanks
                .Include(g => g.Client)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (tank == null)
            {
                return NotFound("Tank was not found.");
            }
            var tankDto = new TankGetDTO
            {
                Id = tank.Id,
                Capacity = tank.Capacity,
                CurrentVolume = tank.CurrentVolume,
                Type = tank.Type,
            };
            return Ok(tankDto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Tank>> PostTank(TankInsertDTO tankDto)
        {
            var tank = new Tank
            {

                Id = tankDto.Id,
                Capacity = tankDto.Capacity,
                CurrentVolume = tankDto.CurrentVolume,
                Type = tankDto.Type,
                ClientId = tankDto.ClientId,
            };
            try
            {
                await _context.Tanks.AddAsync(tank);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
            return CreatedAtAction(nameof(GetTank), new { id = tank.Id }, tank);
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteTank(int id)
        {
            var tank = await _context.Tanks.FindAsync(id);

            if (tank == null)
            {
                return NotFound("Tank was not found.");
            }
            _context.Tanks.Remove(tank);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //[Authorize(Roles = "Admin")]
        [HttpPut("put/{id}")]
        public async Task<IActionResult> PutTank(TankInsertDTO tankDto, int id)
        {
            if (tankDto.Id != id)
            {
                return BadRequest("The ID does not  match with the ID tanck.");
            }

            var tank = await _context.Tanks.FindAsync(id);
            if (tank == null)
            {
                return NotFound("Tank was not found.");
            }
            tank.Capacity = tankDto.Capacity;
            tank.CurrentVolume = tankDto.CurrentVolume;
            tank.Type = tankDto.Type;
            tank.ClientId = tankDto.ClientId;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TankExists(id))
                {
                    return NotFound("Tank does not exist.");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool TankExists(int id)
        {
            return _context.Tanks.Any(e => e.Id == id);
        }

        [Authorize]
        [HttpGet("user")]
        public async Task<ActionResult<IEnumerable<Tank>>> GetUserTanks()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized("User not found.");
            }
            var tanks = await _context.Tanks
                .Include(g => g.Client)
                .Where(g => g.Client.Id == userId)
                .ToListAsync();
            if (tanks == null || tanks.Count == 0)
            {
                return NotFound("No tanks were found for this user.");
            }
            var tanksDTO = tanks.Select(tank => new TankGetDTO
            {
                Id = tank.Id,
                Capacity = tank.Capacity,
                CurrentVolume = tank.CurrentVolume,
                Type = tank.Type,
            }).ToList();
            return Ok(tanksDTO);
        }
    }
}
