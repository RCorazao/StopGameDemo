namespace StopGame.Application.DTOs.Requests;

public class SubmitAnswersRequest
{
    public Dictionary<Guid, string> Answers { get; set; } = new();
}