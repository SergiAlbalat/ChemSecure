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
        /// Retrieves a list of unmanaged warnings from the database.
        /// </summary>
        /// <returns>All the unmanaged warnings in the database or a NotFound if there is no warning there.</returns>
        [Authorize(Roles = "Admin, Manager")]
        [HttpGet("warnings")]
        public async Task<IActionResult> GetWarnings()
        {
            var warnings = await _context.Warnings.Where(w => !w.IsManaged).ToListAsync();
            if (warnings == null || !warnings.Any())
            {
                return NotFound("No warnings found.");
            }
            return Ok(warnings);
        }

        /// <summary>
        /// Retrieves a list of managed warnings from the database.
        /// </summary>
        /// <returns>All the managed warnings in the database or a NotFound if there is no managed warning there.</returns>
        [Authorize(Roles = "Admin, Manager")]
        [HttpGet("managed-warnings")]
        public async Task<IActionResult> GetManagedWarnings()
        {
            var warnings = await _context.Warnings.Where(w => w.IsManaged).ToListAsync();
            if (warnings == null || !warnings.Any())
            {
                return NotFound("No managed warnings found.");
            }
            return Ok(warnings);
        }

        /// <summary>
        /// Marks a warning as managed.
        /// </summary>
        /// <param name="id">The ID of the warning to mark as managed.</param>
        /// <returns>The updated warning or NotFound if the warning doesn't exist.</returns>
        [Authorize(Roles = "Admin, Manager")]
        [HttpPut("manage/{id}")]
        public async Task<IActionResult> ManageWarning(int id)
        {
            var warning = await _context.Warnings.FindAsync(id);
            if (warning == null)
            {
                return NotFound($"Warning with ID {id} not found.");
            }

            warning.IsManaged = true;
            warning.ManagedDate = DateTime.UtcNow;

            _context.Warnings.Update(warning);
            await _context.SaveChangesAsync();

            return Ok(warning);
        }
        
        /// <summary>
        /// Marks a warning as unmanaged (returns it to the pending list).
        /// </summary>
        /// <param name="id">The ID of the warning to mark as unmanaged.</param>
        /// <returns>The updated warning or NotFound if the warning doesn't exist.</returns>
        [Authorize(Roles = "Admin, Manager")]
        [HttpPut("unmanage/{id}")]
        public async Task<IActionResult> UnmanageWarning(int id)
        {
            var warning = await _context.Warnings.FindAsync(id);
            if (warning == null)
            {
                return NotFound($"Warning with ID {id} not found.");
            }

            warning.IsManaged = false;
            warning.ManagedDate = null;

            _context.Warnings.Update(warning);
            await _context.SaveChangesAsync();

            return Ok(warning);
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
                Type = warningDTO.Type,
                IsManaged = false,
                ManagedDate = null
            };
            _context.Warnings.Add(warning);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetWarnings), new { id = warning.Id }, warning);
        }
    }
}