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

        /// <summary>
        /// POST /api/UserLikes/external
        /// Recibe likes desde el onboarding usando ExternalId (ej: "tmdb-movie-1236153").
        /// Si el contenido no existe en la BD, lo crea automaticamente.
        /// </summary>
        [HttpPost("external")]
        public async Task<IActionResult> ExternalLike(ExternalLikeDTO dto)
        {
            var user = await _context.Users.FindAsync(dto.UserId);
            if (user == null)
                return NotFound("Usuario no encontrado");

            // Parsear ExternalId:
            // TMDB: "tmdb-movie-1236153" o "tmdb-tv-1399"
            // IGDB: "igdb-1942" (todos son videojuegos)
            var parts = dto.ExternalId.Split('-');
            if (parts.Length < 2)
                return BadRequest("ExternalId invalido. Formato: tmdb-movie-123, tmdb-tv-123, o igdb-123");

            var source = parts[0]; // "tmdb" o "igdb"
            string typeStr;
            string apiIdStr;

            if (source == "igdb")
            {
                // IGDB: "igdb-1942" → type=game, apiId=1942
                typeStr = "game";
                apiIdStr = string.Join("-", parts.Skip(1));
            }
            else
            {
                // TMDB: "tmdb-movie-1236153" → type=movie, apiId=1236153
                if (parts.Length < 3)
                    return BadRequest("ExternalId TMDB invalido. Formato: tmdb-movie-123 o tmdb-tv-123");
                typeStr = parts[1];
                apiIdStr = string.Join("-", parts.Skip(2));
            }

            if (!int.TryParse(apiIdStr, out int apiId))
                return BadRequest("ExternalId invalido: el ID debe ser numerico");

            // Mapear type string al enum ContentType de la BD
            ContentType contentType = typeStr switch
            {
                "movie" => ContentType.pelicula,
                "tv" => ContentType.serie,
                _ => ContentType.videojuego
            };

            // Buscar si ya existe en la BD por ApiId + Type
            var content = await _context.Content
                .FirstOrDefaultAsync(c => c.ApiId == apiId && c.Type == contentType);

            if (content == null)
            {
                // Crear el contenido en la BD
                content = new Content
                {
                    ApiId = apiId,
                    Type = contentType,
                    Titulo = dto.Title,
                    ImagenUrl = dto.ImageUrl,
                    Año = DateTime.UtcNow.Year
                };
                _context.Content.Add(content);
                await _context.SaveChangesAsync();
            }

            // Control de swipes (igual que el endpoint normal)
            if (!user.IsPremium)
            {
                if (user.LastSwipeUpdate.Date != DateTime.UtcNow.Date)
                {
                    user.Swipes = 10;
                    user.LastSwipeUpdate = DateTime.UtcNow;
                }
                if (user.Swipes <= 0)
                    return BadRequest("No te quedan Swipes. ¡Hazte premium!");
                user.Swipes--;
            }

            // Verificar que no exista un like duplicado
            var existingLike = await _context.UserLikes
                .FirstOrDefaultAsync(ul => ul.UserId == dto.UserId && ul.ContentId == content.Id);

            if (existingLike != null)
                return Ok(new { message = "Ya tienes este contenido en tus likes", contentId = content.Id });

            var userLike = new UserLikes
            {
                UserId = dto.UserId,
                ContentId = content.Id,
                State = dto.State,
                Punctuation = dto.State == State.Liked ? 10 : 0
            };
            _context.UserLikes.Add(userLike);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Like guardado", contentId = content.Id });
        }
    }
}