namespace StopGame.Application.DTOs;

public class VoteDto
{
    public Guid VoterId { get; set; }
    public string VoterName { get; set; } = string.Empty;
    public Guid AnswerOwnerId { get; set; }
    public string AnswerOwnerName { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public DateTime CreatedAt { get; set; }
}