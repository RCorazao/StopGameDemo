

namespace StopGame.Application.DTOs.Requests;

public class ReconnectRoomRequest
{
    public string RoomCode { get; set; } = string.Empty;
    public Guid PlayerId { get; set;  } = Guid.Empty;
}
