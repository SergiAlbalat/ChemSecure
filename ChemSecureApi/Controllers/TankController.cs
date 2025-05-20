using ChemSecureApi.Data;
using ChemSecureApi.DTOs;
using ChemSecureApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [HttpGet("")]
        public async Task<ActionResult<IEnumerable<Tank>>> GetTanks()
        {
            var tanks = await _context.Tanks
                                  .Include(g => g.Client)
                                  .ToListAsync();
            var tanksDTO = tanks.Select(tank => new TankGetDTO
            {
                Capacity = tank.Capacity,
                CurrentVolume = tank.CurrentVolume,
                Type = tank.Type,
            }).ToList();

            return Ok(tanksDTO);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Tank>> GetTank(int id)
        {
            var tank = await _context.Tanks
                .Include(g => g.Client)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (tank == null)
            {
                return NotFound("No s'ha trobat aquest joc.");
            }
            var tankDto = new TankGetDTO
            {
                Capacity = tank.Capacity,
                CurrentVolume = tank.CurrentVolume,
                Type = tank.Type,
            };
            return Ok(tankDto);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<Tank>> PostGame(TankInsertDTO tankDto)
        {
            var tank = new Tank
            {

                Id = tankDto.Id,
                Capacity = tankDto.Capacity,
                CurrentVolume = tankDto.CurrentVolume,
                Type = tankDto.Type,
                Client = tankDto.Client
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
        public async Task<IActionResult> DeleteGame(int id)
        {
            var game = await _context.Tanks.FindAsync(id);

            if (game == null)
            {
                return NotFound("Tank wasn't not found.");
            }
            _context.Tanks.Remove(game);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("put/{id}")]
        public async Task<IActionResult> PutGame(Tank tank, int id)
        {
            if (tank.Id != id)
            {
                return BadRequest();
            }
            _context.Entry(tank).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TankExists(id))
                {
                    return NotFound();
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
    }
}
