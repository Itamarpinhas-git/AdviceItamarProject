namespace ElevatorSystem.Enums // or use ElevatorSystem.Enums if you put it in a folder called Enums
{

  public enum ElevatorStatus
{
    Idle,           // עומדת
    MovingUp,       // עולה
    MovingDown,     // יורדת
    OpeningDoors,   // פתיחת דלתות
    ClosingDoors    // סגירת דלתות
}
    public enum ElevatorDirection
    {
        None,
        Up,
        Down
    }

    public enum DoorStatus
    {
        Open,
        Closed
    }
}