using StopGame.Domain.Enums;

namespace StopGame.Application.DTOs;

public class RoomDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid HostUserId { get; set; }
    public List<TopicDto> Topics { get; set; } = new();
    public List<PlayerDto> Players { get; set; } = new();
    public RoomState State { get; set; }
    public List<RoundDto> Rounds { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int MaxPlayers { get; set; }
    public int RoundDurationSeconds { get; set; }
    public int VotingDurationSeconds { get; set; }
    public int MaxRounds { get; set; }
    public RoundDto? CurrentRound { get; set; }
}