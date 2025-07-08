using StopGame.Application.Interfaces;

namespace StopGame.Infrastructure.Services;

public class ChatService : IChatService
{
    private readonly ISignalRService _signalRService;

    public ChatService(ISignalRService signalRService)
    {
        _signalRService = signalRService;
    }

    public async Task SendMessageToRoomAsync(string roomCode, Guid playerId, string message)
    {
        await _signalRService.SendToGroupAsync(roomCode, "ChatMessage", new
        {
            PlayerId = playerId,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task NotifyPlayerJoinedAsync(string roomCode, string playerName)
    {
        await _signalRService.SendToGroupAsync(roomCode, "PlayerJoined", new
        {
            PlayerName = playerName,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task NotifyPlayerLeftAsync(string roomCode, string playerName)
    {
        await _signalRService.SendToGroupAsync(roomCode, "PlayerLeft", new
        {
            PlayerName = playerName,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task NotifyRoundStartedAsync(string roomCode, char letter, int durationSeconds)
    {
        await _signalRService.SendToGroupAsync(roomCode, "RoundStartedNotification", new
        {
            Letter = letter,
            Message = $"Round started! Letter: {letter}",
            DurationSeconds = durationSeconds,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task NotifyRoundEndedAsync(string roomCode)
    {
        await _signalRService.SendToGroupAsync(roomCode, "RoundEndedNotification", new
        {
            Message = "Round ended! Time to vote!",
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task NotifyVotingStartedAsync(string roomCode, int durationSeconds)
    {
        await _signalRService.SendToGroupAsync(roomCode, "VotingStartedNotification", new
        {
            Message = "Voting phase started!",
            DurationSeconds = durationSeconds,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task NotifyVotingEndedAsync(string roomCode)
    {
        await _signalRService.SendToGroupAsync(roomCode, "VotingEndedNotification", new
        {
            Message = "Voting ended! Check the results!",
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task NotifyGameEndedAsync(string roomCode, string winnerName)
    {
        await _signalRService.SendToGroupAsync(roomCode, "GameEndedNotification", new
        {
            WinnerName = winnerName,
            Message = $"Game ended! Winner: {winnerName}",
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task NotifyPlayerStoppedAsync(string roomCode, string playerName)
    {
        await _signalRService.SendToGroupAsync(roomCode, "PlayerStoppedNotification", new
        {
            PlayerName = playerName,
            Message = $"{playerName} stopped the round!",
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task SendSystemMessageAsync(string roomCode, string message)
    {
        await _signalRService.SendToGroupAsync(roomCode, "SystemMessage", new
        {
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task SendSystemMessageToRoomAsync(string roomCode, string message)
    {
        await SendSystemMessageAsync(roomCode, message);
    }

    public async Task NotifyGameFinishedAsync(string roomCode, List<StopGame.Application.DTOs.PlayerDto> finalScores)
    {
        var winner = finalScores.FirstOrDefault();
        await _signalRService.SendToGroupAsync(roomCode, "GameFinishedNotification", new
        {
            FinalScores = finalScores,
            Winner = winner?.Name,
            Message = winner != null ? $"Game finished! Winner: {winner.Name}" : "Game finished!",
            Timestamp = DateTime.UtcNow
        });
    }
}