using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UserService.Data;
using UserService.Models;
using UserService.DTOs.UserDTO;
using UserService.DTOs.PublishDTO;
using UserService.Services;
using UserService.AsyncDataServices;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UserDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public UsersController(
        UserDbContext context, 
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    // GET: api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetUsers()
    {
        var users = await _context.Users.ToListAsync();
        return users.Select(u => new UserResponseDto
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Address = u.Address,
            Contact = u.Contact
        }).ToList();
    }

    // GET: api/users/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDto>> GetUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Address = user.Address,
            Contact = user.Contact
        };
    }

    // POST: api/users/register
    [HttpPost("register")]
    public async Task<ActionResult<UserResponseDto>> RegisterUser(UserRegisterDto registerDto)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = registerDto.Name,
            Email = registerDto.Email,
            Address = registerDto.Address,
            Contact = registerDto.Contact,
            Password = _passwordHasher.HashPassword(registerDto.Password)
        };

        var responseDto = new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Address = user.Address,
            Contact = user.Contact
        };

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            _context.Users.Add(user);

            var userPublishedDto = new UserPublishedDto
            {
                MessageId = Guid.NewGuid(),
                Id = user.Id,
                Name = user.Name,
                Address = user.Address,
                Contact = user.Contact,
                Event = "User_Published"
            };

            var outboxMessage = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                Type = "User_Published",
                Content = JsonSerializer.Serialize(userPublishedDto),
                OccuredOn = DateTime.UtcNow
            };

            _context.OutboxMessages.Add(outboxMessage);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }

        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, responseDto);
    }

    // PUT: api/users/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, UserRequestDto requestDto)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        user.Name = requestDto.Name;
        user.Email = requestDto.Email;
        user.Address = requestDto.Address;
        user.Contact = requestDto.Contact;

        _context.Entry(user).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.Users.Any(e => e.Id == id)) return NotFound();
            throw;
        }

        return NoContent();
    }

    // DELETE: api/users/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null) return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}