namespace StopGame.Domain.ValueObjects;

public class Vote
{
    public Guid VoterId { get; set; }
    public Guid AnswerOwnerId { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public Vote()
    {
        CreatedAt = DateTime.UtcNow;
    }
    
    public Vote(Guid voterId, Guid answerOwnerId, string topicName, bool isValid) : this()
    {
        VoterId = voterId;
        AnswerOwnerId = answerOwnerId;
        TopicName = topicName;
        IsValid = isValid;
    }
}