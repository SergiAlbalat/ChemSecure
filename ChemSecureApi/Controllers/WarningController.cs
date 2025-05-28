using ChemSecureApi.Data;
using ChemSecureApi.DTOs;
using ChemSecureApi.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChemSecureApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarningController : ControllerBase
    {
        private readonly AppDbContext _context;
        public WarningController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves a list of warnings from the database.
        /// </summary>
        /// <returns>All the warning in the database or a NotFound if there is no warning there.</returns>
        [Authorize(Roles = "Admin, Manager")]
        [HttpGet("warnings")]
        public async Task<IActionResult> GetWarnings()
        {
            var warnings = await _context.Warnings.ToListAsync();
            if (warnings == null || !warnings.Any())
            {
                return NotFound("No warnings found.");
            }
            return Ok(warnings);
        }

        /// <summary>
        /// Adds a new warning to the system based on the provided data.
        /// </summary>
        /// <param name="warningDTO">The data transfer object with the new warning data.</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("add-warning")]
        public async Task<IActionResult> AddWarning([FromBody] WarningDTO warningDTO)
        {
            if (warningDTO == null)
            {
                return BadRequest("Warning data is required.");
            }
            Warning warning = new Warning
            {
                ClientName = warningDTO.ClientName,
                Capacity = warningDTO.Capacity,
                CurrentVolume = warningDTO.CurrentVolume,
                CreationDate = DateTime.UtcNow,
                TankId = warningDTO.TankId,
                Type = warningDTO.Type
            };
            _context.Warnings.Add(warning);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetWarnings), new { id = warning.Id }, warning);
        }
    }
}