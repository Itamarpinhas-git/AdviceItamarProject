using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ElevatorSystem.Models;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly ElevatorDbContext _context;

    public UsersController(ElevatorDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] User user)
    {
        if (_context.Users.Any(u => u.Email == user.Email))
            return BadRequest("User already exists");

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return Ok(user);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] User loginUser)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == loginUser.Email && u.Password == loginUser.Password);

        if (user == null)
            return Unauthorized();

        return Ok(user);
    }
    

}