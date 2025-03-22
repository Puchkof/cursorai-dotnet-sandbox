namespace HeroBoxAI.Domain.Enums
{
    /// <summary>
    /// Represents different roles a user can have in the system
    /// </summary>
    public enum UserRole
    {
        Player,
        Moderator,
        Admin
    }
    
    /// <summary>
    /// Represents different statuses a user account can have
    /// </summary>
    public enum UserStatus
    {
        Active,
        Inactive,
        Banned
    }
    
    /// <summary>
    /// Represents different classes a hero can be
    /// </summary>
    public enum HeroClass
    {
        Warrior,
        Mage,
        Rogue
    }
    
    /// <summary>
    /// Represents different rarities of items in the game
    /// </summary>
    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    
    /// <summary>
    /// Represents different types of items in the game
    /// </summary>
    public enum ItemType
    {
        Weapon,
        Armor,
        Consumable,
        Miscellaneous
    }
} 