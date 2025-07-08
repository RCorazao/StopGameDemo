namespace StopGame.Application.DTOs;

public class SubmissionDto
{
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public AnswerDto Answer { get; set; } = new();
    public DateTime SubmittedAt { get; set; }
    public bool IsValid { get; set; }
    public int VotesValid { get; set; }
    public int VotesInvalid { get; set; }
}