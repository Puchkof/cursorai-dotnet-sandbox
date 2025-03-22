using System;
using System.Collections.Generic;
using HeroBoxAI.Domain.Enums;

namespace HeroBoxAI.Domain.Entities
{
    /// <summary>
    /// Represents a registered user in the system
    /// </summary>
    public class User
    {
        // Properties
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? ClanId { get; set; }  // Foreign key to Clan (nullable for users not in a clan)
        
        // Navigation properties
        public ICollection<Hero> Heroes { get; set; }
        public Clan Clan { get; set; }  // One-to-one relationship with Clan
        
        public User()
        {
            Heroes = new List<Hero>();
        }
    }
} 