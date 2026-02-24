using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Models
{
    public class MessageChat
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int MatchingUserId { get; set; }
        public string Message { get; set; } = "";
        public DateTime Date { get; set; }
        public MessageChat() { }
        public MessageChat(int userId, int matchingUserId, string message)
        {
            UserId = userId;
            MatchingUserId = matchingUserId;
            Message = message;
            Date = DateTime.UtcNow;
        }
    }
}