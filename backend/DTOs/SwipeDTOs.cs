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
}