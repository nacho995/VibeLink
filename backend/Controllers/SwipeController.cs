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
    public class UserLikesController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UserLikesController(AppDbContext context)
        {
            _context = context;
        }
        
        [HttpPost]
        public async Task<IActionResult> UserLikes(SwipeDTOs SwipeDTOs)
        {
            var user = await _context.Users.FindAsync(SwipeDTOs.UserId);
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
            
            var userLike = new UserLikes
            {
                UserId = SwipeDTOs.UserId,
                ContentId = SwipeDTOs.ContentId,
                State = SwipeDTOs.State,
                Punctuation = SwipeDTOs.State == State.Liked ? 10 : 0
            };
            _context.UserLikes.Add(userLike);
            await _context.SaveChangesAsync();
            return Ok();
        }
        
        [HttpGet]
        public async Task<IActionResult> GetSwipes()
        {
            var Swipes = await _context.UserLikes.ToListAsync();
            return Ok(Swipes.Select(Swipe => new SwipeDTOs
            {
                ContentId = Swipe.ContentId,
                State = Swipe.State,
                UserId = Swipe.UserId,
                Punctuation = Swipe.Punctuation
            }));
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserLikes(int id)
        {
            var UserLikes = await _context.UserLikes.FindAsync(id);
            if (UserLikes == null)
            {
                return NotFound();
            }
            return Ok(new SwipeDTOs
            {
                ContentId = UserLikes.ContentId,
                State = UserLikes.State,
                Punctuation = UserLikes.Punctuation
            });
        }
    }
}