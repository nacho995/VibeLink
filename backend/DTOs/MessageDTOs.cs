using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.DTOs
{
    public class MessageDTOs
    {
        public int UserId { get; set; }
        public int MatchingUserId { get; set; }
        public string Message { get; set; } = "";
    }
}