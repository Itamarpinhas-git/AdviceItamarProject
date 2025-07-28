
using Microsoft.AspNetCore.SignalR;

public class ElevatorHub : Hub
{

    public async Task SendElevatorUpdate(int elevatorId, int currentFloor, string status, string direction, string doorStatus)
    {
        //sending for everyone
        await Clients.All.SendAsync("ReceiveElevatorUpdate", elevatorId, currentFloor, status, direction, doorStatus);
    }
}