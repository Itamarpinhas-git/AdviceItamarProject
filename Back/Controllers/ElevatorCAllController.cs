using ElevatorSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class ElevatorCallsController : ControllerBase
{
    private readonly ElevatorDbContext _context;

    public ElevatorCallsController(ElevatorDbContext context)
    {
        _context = context;
    }

    [HttpPost("createCall")]
    public async Task<IActionResult> CreateElevatorCall([FromBody] ElevatorCallDTO dto)
    {
        try
        {       // searching the building using buildingID
            var building = await _context.Buildings // because its running query without blocking the curent thread. so i used await 
                .Include(b => b.Elevator)
                .FirstOrDefaultAsync(b => b.Id == dto.BuildingId);
            //validations...
            if (building == null)
                return NotFound("Building not found");

            if (dto.RequestedFloor < 0 ||
     (dto.DestinationFloor.HasValue && dto.DestinationFloor.Value < 0) ||
     (dto.DestinationFloor.HasValue && dto.RequestedFloor == dto.DestinationFloor.Value))
            {
                return BadRequest("Invalid floor values");
            }

            //now im creating the call. 
            var call = new ElevatorCall
            {
                BuildingId = dto.BuildingId,
                RequestedFloor = dto.RequestedFloor,
                DestinationFloor = dto.DestinationFloor,
                CallTime = DateTime.UtcNow,
                IsHandled = false,

            };

            _context.ElevatorCalls.Add(call);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                call.Id,
                call.RequestedFloor,
                call.DestinationFloor,
                call.CallTime
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Failed to create elevator call", details = ex.Message });
        }
    }

    [HttpPost("insideCall")]
public async Task<IActionResult> CreateInsideCall([FromBody] InsideElevatorCallDTO dto)
{
    try
    {
        Console.WriteLine($"🎯 Inside call request: ElevatorId={dto.ElevatorId}, DestinationFloor={dto.DestinationFloor}");

        var elevator = await _context.Elevators
            .Include(e => e.Building)
            .FirstOrDefaultAsync(e => e.Id == dto.ElevatorId);

        if (elevator == null)
        {
            Console.WriteLine($"❌ Elevator {dto.ElevatorId} not found");
            return NotFound("Elevator not found");
        }

        // Validate floor range
        var maxFloor = elevator.Building.NumberOfFloors;
        if (dto.DestinationFloor < 0 || dto.DestinationFloor >= maxFloor)
        {
            return BadRequest($"Destination floor must be between 0 and {maxFloor - 1}");
        }

        // REQUIREMENT: Only allow when doors are open
        if (elevator.Status != ElevatorSystem.Enums.ElevatorStatus.OpeningDoors)
        {
            return BadRequest("Cannot select destination - elevator doors are not open. Call elevator first!");
        }

        Console.WriteLine($"✅ Doors are open, creating destination call");

        // Create destination call
        var call = new ElevatorCall
        {
            BuildingId = elevator.BuildingId,
            RequestedFloor = elevator.CurrentFloor,
            DestinationFloor = dto.DestinationFloor,
            CallTime = DateTime.UtcNow,
            IsHandled = false
        };

        _context.ElevatorCalls.Add(call);
        await _context.SaveChangesAsync();

        // Assign to current elevator
       var assignment = new ElevatorCallAssignment
{
    CallID = call.Id, // Use the same ID as the call
    ElevatorCallId = call.Id,
    ElevatorId = elevator.Id,
    AssignmentTime = DateTime.UtcNow
};
_context.ElevatorCallAssignments.Add(assignment);

        // 🔑 KEY FIX: Immediately close doors when destination is selected
        elevator.Status = ElevatorSystem.Enums.ElevatorStatus.ClosingDoors;
        elevator.DoorStatus = ElevatorSystem.Enums.DoorStatus.Closed;

        await _context.SaveChangesAsync();

        Console.WriteLine($"✅ Destination call created successfully - elevator will move to floor {dto.DestinationFloor}");
        Console.WriteLine($"🚪 Doors closing immediately - elevator will not wait for other passengers");

        return Ok(new {
            success = true,
            message = $"יעד נבחר: קומה {dto.DestinationFloor}. המעלית סוגרת דלתות ותתחיל לנוע.",
            call = new {
                call.Id,
                call.RequestedFloor,
                call.DestinationFloor
            }
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error in CreateInsideCall: {ex.Message}");
        return StatusCode(500, new { error = "Failed to register internal elevator call", details = ex.Message });
    }
}
}