using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using ElevatorSystem.Models;
using ElevatorSystem.Enums;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;

public class ElevatorMovementService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<ElevatorHub> _hubContext;
    private readonly Dictionary<int, DateTime> _doorOpenTimes = new(); // Track when doors opened

    public ElevatorMovementService(IServiceScopeFactory scopeFactory, IHubContext<ElevatorHub> hubContext)
    {
        _scopeFactory = scopeFactory;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Console.WriteLine(" Elevator Movement Service Started!");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ElevatorDbContext>();

                var elevators = await db.Elevators.ToListAsync();

                foreach (var elevator in elevators)
                {
                    await ProcessSingleElevator(elevator, db);
                }

                await db.SaveChangesAsync();
                await Task.Delay(2000, stoppingToken); // Move every 2 seconds
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Service Error: {ex.Message}");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task ProcessSingleElevator(Elevator elevator, ElevatorDbContext db)
    {
        try
        {
            // Get active calls for this elevator
            var activeCalls = await db.ElevatorCallAssignments
                .Include(a => a.ElevatorCall)
                .Where(a => a.ElevatorId == elevator.Id && !a.ElevatorCall.IsHandled)
                .ToListAsync();

            Console.WriteLine($"🛗 Elevator {elevator.Id}: Floor {elevator.CurrentFloor}, Status {elevator.Status}, Active Calls: {activeCalls.Count}");

            switch (elevator.Status)
            {
                case ElevatorStatus.Idle:
                    await HandleIdleElevator(elevator, db);
                    break;

                case ElevatorStatus.MovingUp:
                    await HandleMovingElevator(elevator, db, activeCalls, 1);
                    break;

                case ElevatorStatus.MovingDown:
                    await HandleMovingElevator(elevator, db, activeCalls, -1);
                    break;

                case ElevatorStatus.OpeningDoors:
                    await HandleDoorsOpenElevator(elevator, db, activeCalls);
                    break;

                case ElevatorStatus.ClosingDoors:
                    await HandleDoorsClosingElevator(elevator, db);
                    break;
            }

            // Send real-time update
            await SendElevatorUpdate(elevator);
        }
        catch (Exception ex)
        {
            Console.WriteLine($" Error processing elevator {elevator.Id}: {ex.Message}");
        }
    }

    private async Task HandleIdleElevator(Elevator elevator, ElevatorDbContext db)
    {
        // Look for unassigned calls in this building
        var pendingCalls = await db.ElevatorCalls
            .Where(c => c.BuildingId == elevator.BuildingId && !c.IsHandled)
            .Where(c => !db.ElevatorCallAssignments.Any(a => a.ElevatorCallId == c.Id)) // 🔧 FIXED: Changed CallID to ElevatorCallId
            .OrderBy(c => c.CallTime)
            .ToListAsync();

        if (pendingCalls.Any())
        {
            var nextCall = pendingCalls.First();
            
            // Assign this call to the elevator
            var assignment = new ElevatorCallAssignment
            {
                CallID = nextCall.Id, // Use the same ID as the call
                ElevatorCallId = nextCall.Id,
                ElevatorId = elevator.Id,
                AssignmentTime = DateTime.Now
            };
            db.ElevatorCallAssignments.Add(assignment);

            // Start moving to the requested floor
            var targetFloor = nextCall.RequestedFloor;
            
            if (targetFloor > elevator.CurrentFloor)
            {
                elevator.Status = ElevatorStatus.MovingUp;
                elevator.Direction = ElevatorDirection.Up;
                Console.WriteLine($" Elevator {elevator.Id} starting to move UP to floor {targetFloor}");
            }
            else if (targetFloor < elevator.CurrentFloor)
            {
                elevator.Status = ElevatorStatus.MovingDown;
                elevator.Direction = ElevatorDirection.Down;
                Console.WriteLine($" Elevator {elevator.Id} starting to move DOWN to floor {targetFloor}");
            }
            else
            {
                // Already at the floor, open doors
                elevator.Status = ElevatorStatus.OpeningDoors;
                elevator.Direction = ElevatorDirection.None;
                _doorOpenTimes[elevator.Id] = DateTime.Now; // Track door open time
                Console.WriteLine($" Elevator {elevator.Id} opening doors at floor {targetFloor}");
            }
        }
    }

    private async Task HandleMovingElevator(Elevator elevator, ElevatorDbContext db, List<ElevatorCallAssignment> activeCalls, int direction)
    {
        // Move one floor
        elevator.CurrentFloor += direction;
        Console.WriteLine($" Elevator {elevator.Id} moved to floor {elevator.CurrentFloor}");

        // Check if we need to stop at this floor
        var callsAtThisFloor = activeCalls
            .Where(a => a.ElevatorCall.RequestedFloor == elevator.CurrentFloor || 
                       a.ElevatorCall.DestinationFloor == elevator.CurrentFloor)
            .ToList();

        if (callsAtThisFloor.Any())
        {
            elevator.Status = ElevatorStatus.OpeningDoors;
            elevator.Direction = ElevatorDirection.None;
            elevator.DoorStatus = DoorStatus.Open;
            _doorOpenTimes[elevator.Id] = DateTime.Now; // Track door open time
            Console.WriteLine($" Elevator {elevator.Id} opening doors at floor {elevator.CurrentFloor}");
        }
    }

    private async Task HandleDoorsOpenElevator(Elevator elevator, ElevatorDbContext db, List<ElevatorCallAssignment> activeCalls)
    {
        // Check how long doors have been open
        var doorOpenTime = _doorOpenTimes.ContainsKey(elevator.Id) ? _doorOpenTimes[elevator.Id] : DateTime.Now;
const int maxDoorOpenSeconds = 10;
        // Mark pickup calls at this floor as handled immediately
        var pickupCallsAtThisFloor = activeCalls
            .Where(a => a.ElevatorCall.RequestedFloor == elevator.CurrentFloor && 
                       a.ElevatorCall.DestinationFloor == null) // Only pickup calls
            .ToList();

        foreach (var assignment in pickupCallsAtThisFloor)
        {
            if (!assignment.ElevatorCall.IsHandled)
            {
                assignment.ElevatorCall.IsHandled = true;
                Console.WriteLine($"Handled pickup call {assignment.ElevatorCall.Id} at floor {elevator.CurrentFloor}");
            }
        }

        // Mark destination calls at this floor as handled
        var destinationCallsAtThisFloor = activeCalls
            .Where(a => a.ElevatorCall.DestinationFloor == elevator.CurrentFloor)
            .ToList();

        foreach (var assignment in destinationCallsAtThisFloor)
        {
            if (!assignment.ElevatorCall.IsHandled)
            {
                assignment.ElevatorCall.IsHandled = true;
                Console.WriteLine($"✅ Handled destination call {assignment.ElevatorCall.Id} at floor {elevator.CurrentFloor}");
            }
        }

      
var elapsed = DateTime.Now - doorOpenTime;
var remainingSeconds = maxDoorOpenSeconds - elapsed.TotalSeconds;

if (remainingSeconds <= 0)
{
    elevator.Status = ElevatorStatus.ClosingDoors;
    _doorOpenTimes.Remove(elevator.Id);
    Console.WriteLine($" Elevator {elevator.Id} doors closing automatically (0 seconds left)");
}
else
{
    Console.WriteLine($" Elevator {elevator.Id} doors open at floor {elevator.CurrentFloor} - {remainingSeconds:F1} seconds left before closing");
}
    }

    private async Task HandleDoorsClosingElevator(Elevator elevator, ElevatorDbContext db)
    {
        elevator.DoorStatus = DoorStatus.Closed;
        
        // Remove completed assignments
        var completedAssignments = await db.ElevatorCallAssignments
            .Include(a => a.ElevatorCall)
            .Where(a => a.ElevatorId == elevator.Id && a.ElevatorCall.IsHandled)
            .ToListAsync();

        db.ElevatorCallAssignments.RemoveRange(completedAssignments);

        // Check for remaining destinations
        var remainingCalls = await db.ElevatorCallAssignments
            .Include(a => a.ElevatorCall)
            .Where(a => a.ElevatorId == elevator.Id && !a.ElevatorCall.IsHandled)
            .ToListAsync();

        if (remainingCalls.Any())
        {
            var nextTarget = remainingCalls.First();
            var targetFloor = nextTarget.ElevatorCall.DestinationFloor ?? nextTarget.ElevatorCall.RequestedFloor;

            if (targetFloor > elevator.CurrentFloor)
            {
                elevator.Status = ElevatorStatus.MovingUp;
                elevator.Direction = ElevatorDirection.Up;
            }
            else if (targetFloor < elevator.CurrentFloor)
            {
                elevator.Status = ElevatorStatus.MovingDown;
                elevator.Direction = ElevatorDirection.Down;
            }
            else
            {
                elevator.Status = ElevatorStatus.OpeningDoors;
                _doorOpenTimes[elevator.Id] = DateTime.Now;
            }

            Console.WriteLine($" Elevator {elevator.Id} continuing to floor {targetFloor}");
        }
        else
        {
            // 🔑 KEY FIX: Check for other waiting calls before going idle
            var otherWaitingCalls = await db.ElevatorCalls
                .Where(c => c.BuildingId == elevator.BuildingId && !c.IsHandled)
                .Where(c => !db.ElevatorCallAssignments.Any(a => a.ElevatorCallId == c.Id)) // 🔧 FIXED: Changed CallID to ElevatorCallId
                .OrderBy(c => c.CallTime)
                .ToListAsync();

            if (otherWaitingCalls.Any())
            {
                Console.WriteLine($" Elevator {elevator.Id} found {otherWaitingCalls.Count} waiting calls - not going idle yet");
                // Don't go idle, let the next cycle handle the waiting calls
                elevator.Status = ElevatorStatus.Idle; // Will be picked up in next HandleIdleElevator
            }
            else
            {
                elevator.Status = ElevatorStatus.Idle;
                elevator.Direction = ElevatorDirection.None;
                Console.WriteLine($" Elevator {elevator.Id} going idle at floor {elevator.CurrentFloor} - no more calls");
            }
        }
    }

    private async Task SendElevatorUpdate(Elevator elevator)
    
    {
        
        await _hubContext.Clients.All.SendAsync("ReceiveElevatorUpdate", new
        {
            elevatorId = elevator.Id,
            floor = elevator.CurrentFloor,
            status = GetHebrewStatus(elevator.Status),
            direction = GetDirectionSymbol(elevator.Direction),
            isAtFloor = elevator.Status == ElevatorStatus.OpeningDoors,
            doorStatus = elevator.DoorStatus.ToString()
        });
    }

    private string GetHebrewStatus(ElevatorStatus status)
    {
        return status switch
        {
            ElevatorStatus.Idle => "עומדת",
            ElevatorStatus.MovingUp => "עולה",
            ElevatorStatus.MovingDown => "יורדת",
            ElevatorStatus.OpeningDoors => "דלתות פתוחות",
            ElevatorStatus.ClosingDoors => "דלתות נסגרות",
            _ => "לא ידוע"
        };
    }

    private string GetDirectionSymbol(ElevatorDirection direction)
    {
        return direction switch
        {
            ElevatorDirection.Up => "▲",
            ElevatorDirection.Down => "▼",
            ElevatorDirection.None => "",
            _ => ""
        };
    }
}