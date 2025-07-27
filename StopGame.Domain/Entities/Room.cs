using StopGame.Domain.Enums;
using StopGame.Domain.ValueObjects;

namespace StopGame.Domain.Entities;

public class Room
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid HostUserId { get; set; }
    public List<Topic> Topics { get; set; } = new();
    public List<Player> Players { get; set; } = new();
    public RoomState State { get; set; } = RoomState.Waiting;
    public List<Round> Rounds { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int MaxPlayers { get; set; } = 8;
    public int RoundDurationSeconds { get; set; } = 60;
    public int VotingDurationSeconds { get; set; } = 30;
    public int MaxRounds { get; set; } = 5;
    
    public Room()
    {
        Id = Guid.NewGuid();
        Code = GenerateRoomCode();
        CreatedAt = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.AddHours(2); // Room expires in 2 hours
    }
    
    private static string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
    
    public bool CanJoin() => Players.Count < MaxPlayers && State == RoomState.Waiting;
    
    public bool IsHost(Guid userId) => HostUserId == userId;
    
    public Player? GetPlayer(Guid playerId) => Players.FirstOrDefault(p => p.Id == playerId);
    
    public Player? GetPlayerByConnectionId(string connectionId) => 
        Players.FirstOrDefault(p => p.ConnectionId == connectionId);
    
    public void AddPlayer(Player player)
    {
        if (CanJoin())
        {
            Players.Add(player);
        }
    }
    
    public void RemovePlayer(Guid playerId)
    {
        var player = GetPlayer(playerId);
        if (player != null)
        {
            Players.Remove(player);
            
            // If host leaves, assign new host or close room
            if (IsHost(playerId) && Players.Any())
            {
                HostUserId = Players.First().Id;
            }
        }
    }
    
    public Round? GetCurrentRound() => Rounds.LastOrDefault();
    
    public bool CanStartNewRound() => State == RoomState.Waiting && Players.Count >= 2;
    
    public void StartNewRound()
    {
        if (CanStartNewRound())
        {
            var round = new Round();
            Rounds.Add(round);
            State = RoomState.Playing;
        }
    }
    
    public void EndCurrentRound()
    {
        var currentRound = GetCurrentRound();
        if (currentRound != null)
        {
            currentRound.EndRound();
            State = RoomState.Voting;
        }
    }
    
    public void EndVoting()
    {
        if (Rounds.Count >= MaxRounds)
        {
            State = RoomState.Finished;
        }
        else
        {
            State = RoomState.Waiting;
        }
    }
    
    public bool IsExpired() => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value;

    public bool HasPlayersSubmittedAnswers()
    {
        return !Players.Any(p => !p.AnswerSubmitted);
    }

    public void ResetPlayerSubmissions()
    {
        foreach (var player in Players)
        {
            player.AnswerSubmitted = false;
        }
    }
}