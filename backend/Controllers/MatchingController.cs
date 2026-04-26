namespace DefaultNamespace;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MatchingController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly CompatibilityService _compatibilityService;

    public MatchingController(AppDbContext context, CompatibilityService compatibilityService)
    {
        _context = context;
        _compatibilityService = compatibilityService;
    }
    [HttpGet("{userId}")]
    public async Task<ActionResult<IEnumerable<object>>> GetMatches(int userId)
    {
        // 1. Obtener IDs de usuarios ya swipeados
        var swipedUserIds = await _context.Swipes
            .Where(s => s.UserId == userId)
            .Select(s => s.MatchingUserId)
            .ToListAsync();

        // 2. Obtener todos los usuarios excepto el actual y los ya swipeados
        var usuarios = await _context.Users
            .Where(u => u.Id != userId && !swipedUserIds.Contains(u.Id))
            .ToListAsync();

        // 3. Para cada usuario, calcular compatibilidad
        var resultados = new List<object>();
        foreach (var usuario in usuarios)
        {
            int compatibilidad = _compatibilityService.CalculateCompatibility(userId, usuario.Id);
            resultados.Add(new
            {
                Usuario = new
                {
                    Id = usuario.Id,
                    Username = usuario.Username,
                    AvatarUrl = usuario.AvatarUrl,
                    Bio = usuario.Bio,
                    Gender = usuario.Gender,
                    DateOfBirth = usuario.DateOfBirth
                },
                Compatibilidad = compatibilidad
            });
        }
        // 4. Ordenar y devolver
        var sorted = resultados.OrderByDescending(r => ((dynamic)r).Compatibilidad).ToList();

        return Ok(sorted);

    }

    [HttpGet("mymatch/{userId}")]
    public async Task<ActionResult<IEnumerable<Match>>> SeeMatches(int userId)
    {
        var matches = await _context.Matches
        .Where(u => u.UserId == userId || u.MatchingUserId == userId)
        .ToListAsync();

        var results = new List<object>();
        foreach (var match in matches)
        {
            int anotherUserId = match.UserId == userId ? match.MatchingUserId : match.UserId;
            var anotherUser = await _context.Users.FindAsync(anotherUserId);
            if (anotherUser == null) continue;

            results.Add(new {
                MatchId = match.Id,
                MatchDate = match.Date,
                AnotherUser = new {
                    Id = anotherUserId,
                    Username = anotherUser.Username,
                    AvatarUrl = anotherUser.AvatarUrl,
                    Bio = anotherUser.Bio
                }
            });
        }

        return Ok(results);
    }
}