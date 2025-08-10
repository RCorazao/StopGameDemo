using StopGame.Application.DTOs;

namespace StopGame.Application.Interfaces;

public interface IChatService
{
    Task SendMessageToRoomAsync(string roomCode, PlayerDto player, string message);
    Task NotifyPlayerJoinedAsync(string roomCode, string playerName);
    Task NotifyPlayerLeftAsync(string roomCode, string playerName);
    Task NotifyRoundStartedAsync(string roomCode, char letter);
    Task NotifyVotingStartedAsync(string roomCode);
    Task NotifyGameEndedAsync(string roomCode, string winnerName);
    Task NotifyPlayerStoppedAsync(string roomCode, string playerName);
}