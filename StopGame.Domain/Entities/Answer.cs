using StopGame.Domain.ValueObjects;

namespace StopGame.Domain.Entities;

public class Answer
{
    public Guid Id { get; set; }
    public Guid TopicId { get; set; }
    public Guid PlayerId { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<Vote> Votes { get; set; } = new();

    public Answer() 
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }
    
    public bool IsValid(char requiredLetter)
    {
        if (string.IsNullOrWhiteSpace(Value))
            return false;
            
        return char.ToUpperInvariant(Value[0]) == char.ToUpperInvariant(requiredLetter);
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not Answer other)
            return false;
            
        return Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase) &&
               TopicId == other.TopicId;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(
            Value.ToLowerInvariant(), 
            TopicName.ToLowerInvariant());
    }

    public void AddVote(Vote vote)
    {
        var existingVote = Votes.FirstOrDefault(v => v.VoterId == vote.VoterId);

        if (existingVote is null)
        {
            Votes.Add(vote);
            return;
        }

        if (existingVote.IsValid == vote.IsValid) return;

        existingVote.IsValid = vote.IsValid;
    }
}