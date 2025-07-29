namespace StopGame.Application.DTOs;

public class RoundDto
{
    public Guid Id { get; set; }
    public char Letter { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public List<AnswerDto> Answers { get; set; } = new();
    public bool IsActive { get; set; }
    public int TimeRemainingSeconds { get; set; }
}