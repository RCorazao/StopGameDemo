namespace StopGame.Application.DTOs.Requests;

public class CreateRoomRequest
{
    public string HostName { get; set; } = string.Empty;
    public List<string> CustomTopics { get; set; } = new();
    public bool UseDefaultTopics { get; set; } = true;
    public int MaxPlayers { get; set; } = 8;
    public int RoundDurationSeconds { get; set; } = 60;
    public int VotingDurationSeconds { get; set; } = 30;
    public int MaxRounds { get; set; } = 5;
}