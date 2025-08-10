using StopGame.Application.DTOs;
using StopGame.Application.Interfaces;

namespace StopGame.Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly ISignalRService _signalRService;

    public ChatService(ISignalRService signalRService)
    {
        _signalRService = signalRService;
    }

    public async Task SendMessageToRoomAsync(string roomCode, PlayerDto player, string message)
    {
        await _signalRService.SendToGroupAsync(roomCode, "ChatNotification", new
        {
            Player = player,
            Message = message,
            Timestamp = DateTime.UtcNow,
            Source = "Player"
        });
    }

    public async Task NotifyPlayerJoinedAsync(string roomCode, string playerName)
    {
        await _signalRService.SendToGroupAsync(roomCode, "ChatNotification", new
        {
            Message = $"<< {playerName} >> joined the room",
            Timestamp = DateTime.UtcNow,
            Source = "System"
        });
    }

    public async Task NotifyPlayerLeftAsync(string roomCode, string playerName)
    {
        await _signalRService.SendToGroupAsync(roomCode, "ChatNotification", new
        {
            Message = $"<< {playerName} >> left the room",
            Timestamp = DateTime.UtcNow,
            Source = "System"
        });
    }

    public async Task NotifyRoundStartedAsync(string roomCode, char letter)
    {
        await _signalRService.SendToGroupAsync(roomCode, "ChatNotification", new
        {
            Message = $"Round started! Letter: {letter}",
            Timestamp = DateTime.UtcNow,
            Source = "System"
        });
    }

    public async Task NotifyVotingStartedAsync(string roomCode)
    {
        await _signalRService.SendToGroupAsync(roomCode, "ChatNotification", new
        {
            Message = "Voting phase started!",
            Timestamp = DateTime.UtcNow,
            Source = "System"
        });
    }

    public async Task NotifyGameEndedAsync(string roomCode, string winnerName)
    {
        await _signalRService.SendToGroupAsync(roomCode, "ChatNotification", new
        {
            Message = $"Game ended! Winner: {winnerName}",
            Timestamp = DateTime.UtcNow,
            Source = "System"
        });
    }

    public async Task NotifyPlayerStoppedAsync(string roomCode, string playerName)
    {
        await _signalRService.SendToGroupAsync(roomCode, "ChatNotification", new
        {
            Message = $"{playerName} stopped the round!",
            Timestamp = DateTime.UtcNow,
            Source = "System"
        });
    }
}