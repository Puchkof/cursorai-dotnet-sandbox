using System;
using System.Collections.Generic;
using HeroBoxAI.Domain.Enums;

namespace HeroBoxAI.Domain.Entities
{
    /// <summary>
    /// Represents a game item that can be acquired by heroes
    /// </summary>
    public class Item
    {
        // Properties
        public Guid Id { get; set; }
        public Guid HeroId { get; set; }  // Foreign key to Hero
        public string Name { get; set; }
        public string Description { get; set; }
        public ItemType Type { get; set; }
        public ItemRarity Rarity { get; set; }
        public int RequiredLevel { get; set; }
        public int Quantity { get; set; }
        public bool IsEquipped { get; set; }
        public DateTime AcquiredAt { get; set; }
        
        // Navigation properties
        public Hero Hero { get; set; }
    }
} 