using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ElevatorSystem.Models
{
    public class ElevatorCallAssignment
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // This prevents auto-increment
        public int CallID { get; set; }
        
        public DateTime AssignmentTime { get; set; }
        public int ElevatorCallId { get; set; }
        public int ElevatorId { get; set; }
        
        // Navigation properties
        public ElevatorCall ElevatorCall { get; set; }
        public Elevator Elevator { get; set; }
    }
}
