using StopGame.Application.DTOs;

namespace StopGame.Application.Interfaces;

public interface IChatService
{
    Task SendMessageToRoomAsync(string roomCode, Guid playerId, string message);
    Task SendSystemMessageToRoomAsync(string roomCode, string message);
    Task NotifyPlayerJoinedAsync(string roomCode, string playerName);
    Task NotifyPlayerLeftAsync(string roomCode, string playerName);
    Task NotifyRoundStartedAsync(string roomCode, char letter, int durationSeconds);
    Task NotifyRoundEndedAsync(string roomCode);
    Task NotifyVotingStartedAsync(string roomCode, int durationSeconds);
    Task NotifyVotingEndedAsync(string roomCode);
    Task NotifyGameFinishedAsync(string roomCode, List<PlayerDto> finalScores);
    Task NotifyPlayerStoppedAsync(string roomCode, string playerName);
    Task NotifyGameEndedAsync(string roomCode, string winnerName);
}