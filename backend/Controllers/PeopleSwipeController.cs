using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using backend.DTOs;
using DefaultNamespace;
namespace backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SwipeController : ControllerBase
    {
        private readonly AppDbContext _context;
        public SwipeController(AppDbContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> Swipe(PeopleSwipeDTOs peopleSwipeDTOs)
        {
            // Prevenir swipes duplicados
            var existingSwipe = await _context.Swipes.FirstOrDefaultAsync(s =>
                s.UserId == peopleSwipeDTOs.UserId &&
                s.MatchingUserId == peopleSwipeDTOs.MatchingUserId);
            if (existingSwipe != null)
            {
                return BadRequest("Ya has swipeado a este usuario");
            }

            var user = await _context.Users.FindAsync(peopleSwipeDTOs.UserId);
            if (user == null)
            {
                return NotFound("Usuario no encontrado");
            }
            if (!user.IsPremium)
            {
                if (user.LastSwipeUpdate.Date != DateTime.UtcNow.Date)
                {
                    user.Swipes = 10;
                    user.LastSwipeUpdate = DateTime.UtcNow;
                }
                if (user.Swipes <= 0)
                {
                    return BadRequest("No te quedan Swipes. ¡Hazte premium!");
                }
                user.Swipes--;
            }
            var swipe = new Swipe
            {
                UserId = peopleSwipeDTOs.UserId,
                MatchingUserId = peopleSwipeDTOs.MatchingUserId,
                State = peopleSwipeDTOs.State
            };
            _context.Swipes.Add(swipe);
            
            

            if (peopleSwipeDTOs.State == SwipeState.Like)
            {
                var matchMutuo = await _context.Swipes.FirstOrDefaultAsync(s =>
                    s.UserId == peopleSwipeDTOs.MatchingUserId &&
                    s.MatchingUserId == peopleSwipeDTOs.UserId &&
                    s.State == SwipeState.Like);

                if (matchMutuo != null)
                {
                    var existingMatch = await _context.Matches.FirstOrDefaultAsync(m =>
                        (m.UserId == peopleSwipeDTOs.UserId && m.MatchingUserId == peopleSwipeDTOs.MatchingUserId) ||
                        (m.UserId == peopleSwipeDTOs.MatchingUserId && m.MatchingUserId == peopleSwipeDTOs.UserId));

                    if (existingMatch == null)
                    {
                        var newMatch = new Match
                        {
                            UserId = peopleSwipeDTOs.UserId,
                            MatchingUserId = peopleSwipeDTOs.MatchingUserId,
                            Date = DateTime.UtcNow
                        };
                        _context.Matches.Add(newMatch);
                        await _context.SaveChangesAsync();
                        return Ok(newMatch);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> GetSwipe()
        {
            var Swipe = await _context.Swipes.ToListAsync();
            return Ok(Swipe);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSwipe(int id)
        {
            var Swipe = await _context.Swipes.FindAsync(id);
            return Ok(Swipe);
        }
    }
}