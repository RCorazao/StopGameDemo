namespace StopGame.Application.DTOs.Requests;

public class VoteRequest
{
    public Guid AnswerId { get; set; }
    public bool IsValid { get; set; }
}