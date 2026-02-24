using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using backend.DTOs;
using DefaultNamespace;
using backend.Models;
namespace backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;
        
        public ChatController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage(MessageDTOs messageDto)
        {
            var match = await _context.Matches.FirstOrDefaultAsync(m =>
                    (m.UserId == messageDto.UserId && m.MatchingUserId == messageDto.MatchingUserId) ||
                    (m.UserId == messageDto.MatchingUserId && m.MatchingUserId == messageDto.UserId));
            if (match == null)
            {
                return BadRequest("No tienen match, no pueden chatear");
            }
            var Message = new MessageChat
            {
                UserId = messageDto.UserId,
                MatchingUserId = messageDto.MatchingUserId,
                Message = messageDto.Message,
                Date = DateTime.UtcNow
            };
            _context.MessageChats.Add(Message);
            await _context.SaveChangesAsync();

            return Ok(Message);
        }
        
        [HttpGet]
        public async Task<IActionResult> GetMessages(int userId, int matchingUserId)
        {
            var messages = await _context.MessageChats
            .Where(m =>
            (m.UserId == userId && m.MatchingUserId == matchingUserId) ||
            (m.UserId == matchingUserId && m.MatchingUserId == userId))
            .OrderBy(m => m.Date)
            .ToListAsync();
            return Ok(messages);
        }
    }
}