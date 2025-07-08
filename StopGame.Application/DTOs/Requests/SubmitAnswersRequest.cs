namespace StopGame.Application.DTOs.Requests;

public class SubmitAnswersRequest
{
    public Dictionary<string, string> Answers { get; set; } = new();
}