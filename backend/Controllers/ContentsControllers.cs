using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
namespace DefaultNamespace;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ContentsControllers : ControllerBase
{
    
    private readonly AppDbContext _context;
    
    public ContentsControllers(AppDbContext context)
    {
        _context = context;
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Content>>> GetContent()
    {
        var content = await _context.Content.ToListAsync();
        return Ok(content);
    }
    
    [HttpGet("{id}")]
    public async Task<ActionResult<Content>> GetContentById(int id)
    {
        var content = await _context.Content.FindAsync(id);
        if (content == null)
        {
            return NotFound();
        }
        return Ok(content);
    }

    [HttpPost]
    public async Task<IActionResult> CreateContent(Content content)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        _context.Content.Add(content);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetContentById), new { id = content.Id }, content);
    }
}