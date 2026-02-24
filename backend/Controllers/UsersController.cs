using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using backend.DTOs;
namespace DefaultNamespace;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;
    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponseDTOs>>> GetUsers()
    {
        var users = await _context.Users.ToListAsync();
        var usersResponse = users.Select(user => new UserResponseDTOs
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            CodigoInvitacion = user.CodigoInvitacion,
            FechaRegistro = user.FechaRegistro,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            Bio = user.Bio,
            IsPremium = user.IsPremium,
            Swipes = user.Swipes,
        }).ToList();
        return Ok(usersResponse);
    }
    [HttpGet("{id}")]
    public async Task<ActionResult<UserResponseDTOs>> GetUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }
        var userResponse = new UserResponseDTOs
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            AvatarUrl = user.AvatarUrl,
            CodigoInvitacion = user.CodigoInvitacion,
            FechaRegistro = user.FechaRegistro,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            Bio = user.Bio,
            IsPremium = user.IsPremium,
            Swipes = user.Swipes,
        };
        return Ok(userResponse);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, ProfileDataDTOs profileDataDTOs)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound();
        }
    user.AvatarUrl = profileDataDTOs.AvatarUrl ?? user.AvatarUrl;
    user.Gender = profileDataDTOs.Gender;
    user.DateOfBirth = DateTime.SpecifyKind(profileDataDTOs.DateOfBirth, DateTimeKind.Utc);
    user.Bio = profileDataDTOs.Bio ?? user.Bio;

    await _context.SaveChangesAsync();
    return Ok(new UserResponseDTOs
{
    Id = user.Id,
    Username = user.Username,
    Email = user.Email,
    AvatarUrl = user.AvatarUrl,
    CodigoInvitacion = user.CodigoInvitacion,
    FechaRegistro = user.FechaRegistro,
    Gender = user.Gender,
    DateOfBirth = user.DateOfBirth,
    Bio = user.Bio,
        IsPremium = user.IsPremium,
        Swipes = user.Swipes,
    });
    }
}
