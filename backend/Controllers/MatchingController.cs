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
        // 1. Obtener todos los usuarios excepto el actual
        var usuarios = await _context.Users.Where(u => u.Id != userId).ToListAsync();

        // 2. Para cada usuario, calcular compatibilidad
        var resultados = new List<object>();
        foreach (var usuario in usuarios)
        {
            int compatibilidad = _compatibilityService.CalculateCompatibility(userId, usuario.Id);
            resultados.Add(new
            {
                Usuario = usuario,
                Compatibilidad = compatibilidad
            });
        }
        // 3. Ordenar y devolver
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