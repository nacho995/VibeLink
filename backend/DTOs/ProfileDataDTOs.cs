using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DefaultNamespace;

namespace backend.DTOs
{
    public class ProfileDataDTOs
    {
        public string? AvatarUrl { get; set; }
        public Gender Gender { get; set; }
        public DateTime DateOfBirth { get; set; } = DateTime.UtcNow;
        public string? Bio { get; set; } = "";
    }
}