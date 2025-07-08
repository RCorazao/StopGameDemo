namespace StopGame.Application.DTOs;

public class PlayerDto
{
    public Guid Id { get; set; }
    public string ConnectionId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
    public bool IsConnected { get; set; }
    public DateTime JoinedAt { get; set; }
    public bool IsHost { get; set; }
}