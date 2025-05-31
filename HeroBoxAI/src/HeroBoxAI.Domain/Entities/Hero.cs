using System;
using System.Collections.Generic;
using HeroBoxAI.Domain.Enums;

namespace HeroBoxAI.Domain.Entities
{
    /// <summary>
    /// Represents a player's character in the game
    /// </summary>
    public class Hero
    {
        // Properties
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public HeroClass Class { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public User User { get; set; }
        public ICollection<Item> Items { get; set; }
        
        public Hero()
        {
            Items = new List<Item>();
        }
    }
} 