using System;
using System.Collections.Generic;

namespace ElevatorSystem.Models;

public class ElevatorCall
{
    public int Id { get; set; }

    public int BuildingId { get; set; }

    public int RequestedFloor { get; set; }

    public int? DestinationFloor { get; set; }

    public DateTime CallTime { get; set; }

    public bool IsHandled { get; set; }

    public virtual Building Building { get; set; } = null!;

    public virtual ICollection<ElevatorCallAssignment> Assignments { get; set; } = new List<ElevatorCallAssignment>();
}
