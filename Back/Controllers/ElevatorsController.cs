using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElevatorSystem.Models;

[ApiController]
[Route("api/[controller]")]
public class ElevatorsController : ControllerBase
{
    private readonly ElevatorDbContext _context;

    public ElevatorsController(ElevatorDbContext context)
    {
        _context = context;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddElevator([FromBody] Elevator elevator)
    {
        try
        {
            // Optionally check if building exists
            var buildingExists = await _context.Buildings.AnyAsync(b => b.Id == elevator.BuildingId);
            if (!buildingExists)
                return NotFound("Building not found");

            _context.Elevators.Add(elevator);
            await _context.SaveChangesAsync();

            return Ok(elevator);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to add elevator", details = ex.Message });
        }
    }

    [HttpGet("by-building/{buildingId}")]
    public async Task<IActionResult> GetElevatorsByBuilding(int buildingId)
    {
        try
        {
            var elevators = await _context.Elevators
                .Where(e => e.BuildingId == buildingId)
                .ToListAsync();

            return Ok(elevators);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve elevators", details = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetElevatorById(int id)
    {
        try
        {
            var elevator = await _context.Elevators
                .Include(e => e.Building)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (elevator == null)
                return NotFound("Elevator not found");

            return Ok(elevator);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve elevator", details = ex.Message });
        }
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateElevatorStatus(int id, [FromBody] dynamic statusUpdate)
    {
        try
        {
            var elevator = await _context.Elevators.FindAsync(id);
            if (elevator == null)
                return NotFound("Elevator not found");

            // This endpoint can be used for manual testing/debugging
            if (statusUpdate.currentFloor != null)
                elevator.CurrentFloor = statusUpdate.currentFloor;
            
            if (statusUpdate.status != null)
                elevator.Status = Enum.Parse<ElevatorSystem.Enums.ElevatorStatus>(statusUpdate.status.ToString());

            await _context.SaveChangesAsync();
            return Ok(elevator);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to update elevator", details = ex.Message });
        }
    }
}