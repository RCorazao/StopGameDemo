namespace StopGame.Domain.ValueObjects;

public class Vote
{
    public Guid VoterId { get; set; }
    public Guid AnswerOwnerId { get; set; }
    public Guid TopicId { get; set; }
    public bool IsValid { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    
    public Vote()
    {
        CreatedAt = DateTime.UtcNow;
    }
    
    public Vote(Guid voterId, Guid answerOwnerId, Guid topicId, bool isValid) : this()
    {
        VoterId = voterId;
        AnswerOwnerId = answerOwnerId;
        TopicId = topicId;
        IsValid = isValid;
    }
}