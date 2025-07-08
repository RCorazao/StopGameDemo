namespace StopGame.Application.DTOs.Requests;

public class JoinRoomRequest
{
    public string RoomCode { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
}