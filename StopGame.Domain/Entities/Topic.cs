namespace StopGame.Domain.Entities;

public class Topic
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsDefault { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Topic()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }
    
    public Topic(string name, bool isDefault = false, Guid? createdByUserId = null) : this()
    {
        Name = name;
        IsDefault = isDefault;
        CreatedByUserId = createdByUserId;
    }
    
    public static List<Topic> GetDefaultTopics()
    {
        return new List<Topic>
        {
            new("Animal", true),
            new("Country", true),
            new("City", true),
            new("Food", true),
            new("Color", true),
            new("Name", true),
            new("Profession", true),
            new("Movie", true),
            new("Brand", true),
            new("Sport", true)
        };
    }
}