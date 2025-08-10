using StopGame.Application.DTOs;
using StopGame.Application.DTOs.Requests;

namespace StopGame.Application.Interfaces;

public interface IRoomService
{
    Task<RoomDto> CreateRoomAsync(CreateRoomRequest request, string connectionId);
    Task<RoomDto> JoinRoomAsync(JoinRoomRequest request, string connectionId);
    Task<RoomDto> ReconnectRoom(ReconnectRoomRequest request, string connectionId);
    Task<RoomDto> UpdateRoomSettings(string roomCode, UpdateRoomSettingsRequest request);
    Task<RoomDto?> GetRoomAsync(string roomCode);
    Task<RoomDto?> GetRoomByConnectionIdAsync(string connectionId);
    Task LeaveRoomAsync(string connectionId);
    Task<RoomDto> StartRoundAsync(string roomCode, Guid hostUserId);
    Task SubmitAnswersAsync(string roomCode, Guid playerId, SubmitAnswersRequest request);
    Task StopRoundAsync(string roomCode, Guid playerId);
    Task<RoomDto> VoteAsync(string roomCode, Guid voterId, VoteRequest request);
    Task<List<VoteAnswerDto>> GetAnswersDataAsync(string roomCode);
    Task<RoomDto> FinishVotingPhase(string roomCode, Guid playerId);
    Task<List<RoomDto>> GetActiveRoomsAsync();
    Task CleanupExpiredRoomsAsync();
    Task UpdatePlayerConnectionAsync(string oldConnectionId, string newConnectionId);
}