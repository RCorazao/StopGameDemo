using StopGame.Application.DTOs.Requests;

namespace StopGame.Application.Interfaces;

public interface IAnswerSubmissionService
{
    Task ProcessAnswersAsync(string roomCode, Guid playerId, SubmitAnswersRequest request);
}
