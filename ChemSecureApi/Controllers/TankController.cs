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

        /// <summary>
        /// Retrieves a list of all t tanks in the database.
        /// </summary>
        /// <returns>A list of all tanks</returns>
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

        /// <summary>
        /// Retrieves a tank by its unique identifier
        /// </summary>
        /// <param name="id">The unique identifier of the tank to retrieve</param>
        /// <returns>The wanted tank or a NotFound response if the tank don't exists</returns>
        [Authorize]
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

        /// <summary>
        /// Creates a new tank record in the database
        /// </summary>
        /// <param name="tankDto">The transfer data object containing the new tank data</param>
        /// <returns></returns>
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

        /// <summary>
        /// Deletes a tank with the specified identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the tank to delete.</param>
        /// <returns></returns>
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

        /// <summary>
        /// Updates the details of an existing tank.
        /// </summary>
        /// <param name="tankDto">The data transfer object containing the updated tank details.</param>
        /// <param name="id">The unique identifier of the tank to be updated.</param>
        /// <returns></returns>
        [Authorize(Roles = "Admin")]
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

        /// <summary>
        /// Updates the current volume of a specified tank.
        /// </summary>
        /// <param name="id">The unique identifier of the tank to update.</param>
        /// <param name="newVolume">The new volume to set for the tank. Must not exceed the tank's capacity.</param>
        /// <returns></returns>
        [HttpPatch("update-volume/{id}")]
        public async Task<IActionResult> UpdateTankVolume(int id, [FromBody] double newVolume)
        {
            var tank = await _context.Tanks.FindAsync(id);
            if (tank == null)
            {
                return NotFound("Tank does not exist.");
            }

            // Update the current volume property
            tank.CurrentVolume = newVolume;

            if(newVolume > tank.Capacity)
            {
                return BadRequest("The volume exceeds the tank capacity.");
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Error consulting volum");
            }

            return NoContent();
        }

        /// <summary>
        /// Determines whether a tank with the specified identifier exists in the data source.
        /// </summary>
        /// <param name="id">The unique identifier of the tank to check for existence.</param>
        /// <returns>True if the tank exists, false if it doesn't</returns>
        private bool TankExists(int id)
        {
            return _context.Tanks.Any(e => e.Id == id);
        }

        /// <summary>
        /// Retrieves a list of tanks associated with the currently authenticated user using its autentication token.
        /// </summary>
        /// <returns></returns>
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
