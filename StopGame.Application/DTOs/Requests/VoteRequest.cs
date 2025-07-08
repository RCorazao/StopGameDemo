namespace StopGame.Application.DTOs.Requests;

public class VoteRequest
{
    public List<VoteItem> Votes { get; set; } = new();
}

public class VoteItem
{
    public Guid AnswerOwnerId { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public bool IsValid { get; set; }
}