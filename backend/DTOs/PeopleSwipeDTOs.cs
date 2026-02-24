using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DefaultNamespace;
namespace backend.DTOs
{
    public class PeopleSwipeDTOs
    {
        public int UserId { get; set; }
        public int MatchingUserId { get; set; }
        public SwipeState State { get; set; }
    }
}