using System;
using System.Collections.Generic;

namespace HeroBoxAI.Domain.Entities
{
    /// <summary>
    /// Represents a clan or guild that users can join
    /// </summary>
    public class Clan
    {
        // Properties
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Tag { get; set; }
        public string Description { get; set; }
        public Guid FounderId { get; set; }
        public int Level { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public User Founder { get; set; }
        public ICollection<User> Members { get; set; }
        
        public Clan()
        {
            Members = new List<User>();
        }
    }
} 