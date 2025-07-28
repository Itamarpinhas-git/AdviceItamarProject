using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElevatorSystem.Models;
using ElevatorSystem.Enums;


[ApiController]
[Route("api/[controller]")]
public class BuildingsController : ControllerBase
{
    private readonly ElevatorDbContext _context;

    public BuildingsController(ElevatorDbContext context)
    {
        _context = context;
    }

    // ✅ Get all buildings for a user
    [HttpGet("by-user/{userId}")]
    public async Task<IActionResult> GetBuildingsByUser(int userId)
    {
        try
        {
            var buildings = await _context.Buildings
                .Where(b => b.UserId == userId)
                //.Include(b => b.Elevators)
                .ToListAsync();

            return Ok(buildings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve buildings", details = ex.Message });
        }
    }

    // ✅ Add a new building for user + create 1 elevator
    [HttpPost("AddBuilding")]
    public async Task<IActionResult> AddBuilding([FromBody] BuildingDTO dto)
    {
        try
        {
            var exists = await _context.Buildings
                .AnyAsync(b => b.Name == dto.Name && b.UserId == dto.UserId);

            if (exists)
                return BadRequest("Building with the same name already exists for this user");

            var building = new Building
            {
                UserId = dto.UserId,
                Name = dto.Name,
                NumberOfFloors = dto.NumberOfFloors
            };

            _context.Buildings.Add(building);
             await _context.SaveChangesAsync(); // saving the building that i could create for it an elevator
            
            // Create default elevator
        var elevator = new Elevator
        {
            BuildingId = building.Id,
            CurrentFloor = 0,
            Status = ElevatorStatus.Idle,
            Direction = ElevatorDirection.None,
            DoorStatus = DoorStatus.Closed
        };

            _context.Elevators.Add(elevator);
            await _context.SaveChangesAsync();
            return Ok(new {
    building.Id,
    building.Name,
    building.NumberOfFloors
});
        }
       catch (Exception ex)
         {
        return StatusCode(500, new
        {
            error = "Unexpected error occurred while adding building.",
            details = ex.Message // <== THIS will reveal the real problem
        });
     }
}
    // ✅ Get building by ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetBuildingById(int id)
    {
        try
        {
            var building = await _context.Buildings
                .Include(b => b.Elevator) // adjust if one-to-many
                .FirstOrDefaultAsync(b => b.Id == id);

            if (building == null)
                return NotFound("Building not found");

            return Ok(new {
    building.Id,
    building.Name,
    building.NumberOfFloors
});
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to retrieve building", details = ex.Message });
        }
    }
}
