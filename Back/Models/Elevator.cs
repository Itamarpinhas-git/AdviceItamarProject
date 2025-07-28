using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using ElevatorSystem.Enums;



namespace ElevatorSystem.Models;

public class Elevator
{
    public int Id { get; set; }

    public int BuildingId { get; set; }

    public int CurrentFloor { get; set; }

    public ElevatorStatus Status { get; set; } = ElevatorStatus.Idle;

    public ElevatorDirection Direction { get; set; } = ElevatorDirection.None;

    public DoorStatus DoorStatus { get; set; } = DoorStatus.Closed;

    public virtual Building Building { get; set; } = null!;

    public virtual ICollection<ElevatorCallAssignment> Assignments { get; set; } = new List<ElevatorCallAssignment>();
}
