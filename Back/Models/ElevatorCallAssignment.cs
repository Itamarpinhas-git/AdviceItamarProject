using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElevatorSystem.Models;

public class ElevatorCallAssignment
{
    [Key]
    public int CallID { get; set; } // Primary Key

    public int ElevatorId { get; set; } // Foreign Key to Elevator

    public int ElevatorCallId { get; set; } // Foreign Key to ElevatorCall

    public DateTime AssignmentTime { get; set; }

    // Navigation Properties - Simple approach
    public virtual ElevatorCall ElevatorCall { get; set; } = null!;
    public virtual Elevator Elevator { get; set; } = null!;
}