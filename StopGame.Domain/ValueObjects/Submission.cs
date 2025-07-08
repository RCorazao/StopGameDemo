namespace StopGame.Domain.ValueObjects;

public class Submission
{
    public Guid PlayerId { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public Answer Answer { get; set; } = new();
    public DateTime SubmittedAt { get; set; }
    
    public Submission()
    {
        SubmittedAt = DateTime.UtcNow;
    }
    
    public Submission(Guid playerId, string topicName, Answer answer) : this()
    {
        PlayerId = playerId;
        TopicName = topicName;
        Answer = answer;
    }
    
    public bool IsValidForLetter(char letter)
    {
        return Answer.IsValid(letter);
    }
}