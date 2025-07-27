namespace StopGame.Application.DTOs;

public class AnswerDto
{
    public Guid Id { get; set; }
    public Guid TopicId { get; set; }
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string TopicName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<VoteDto> Votes { get; set; } = new();
}