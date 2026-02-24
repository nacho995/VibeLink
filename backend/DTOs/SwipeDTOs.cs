using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DefaultNamespace;

namespace backend.DTOs
{
    public class SwipeDTOs
    {
        public int UserId { get; set; }
        public int ContentId { get; set; } = 0;
        public int MatchingUserId { get; set; }
        public State State { get; set; } = State.Liked;
        public int Punctuation { get; set; } = 0;
    }

    /// <summary>
    /// DTO para likes desde el onboarding, usando ExternalId de APIs externas.
    /// </summary>
    public class ExternalLikeDTO
    {
        public int UserId { get; set; }
        public string ExternalId { get; set; } = "";  // ej: "tmdb-movie-1236153"
        public string Title { get; set; } = "";
        public string? ImageUrl { get; set; }
        public State State { get; set; } = State.Liked;
    }
}