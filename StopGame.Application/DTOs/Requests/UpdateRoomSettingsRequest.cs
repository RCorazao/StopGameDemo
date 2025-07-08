
namespace StopGame.Application.DTOs.Requests
{
    public class UpdateRoomSettingsRequest
    {
        public List<string> Topics { get; set; } = new();
        public int MaxPlayers { get; set; } = 8;
        public int RoundDurationSeconds { get; set; } = 60;
        public int VotingDurationSeconds { get; set; } = 30;
        public int MaxRounds { get; set; } = 5;
    }
}
