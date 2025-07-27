
namespace StopGame.Application.DTOs;

public class VoteAnswerDto
{
    public Guid TopicId { get; set; }
    public string TopicName { get; set; } = string.Empty;
    public List<AnswerDto> Answers { get; set; } = new();
}
