using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DefaultNamespace;

namespace backend.DTOs
{
    public class UserResponseDTOs
    {
        public int Id { get; set; }
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string? AvatarUrl { get; set; }
        public string? CodigoInvitacion { get; set; }
        public DateTime FechaRegistro { get; set; }
        public Gender Gender { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string? Bio { get; set; }
        public bool IsPremium { get; set; }
        public int Swipes { get; set; }
    }
}