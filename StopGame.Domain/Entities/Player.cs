namespace StopGame.Domain.Entities;

public class Player
{
    public Guid Id { get; set; }
    public string ConnectionId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
    public bool IsConnected { get; set; } = true;
    public DateTime JoinedAt { get; set; }
    public bool AnswerSubmitted { get; set; } = false;

    public Player()
    {
        Id = Guid.NewGuid();
        JoinedAt = DateTime.UtcNow;
    }
    
    public Player(string name, string connectionId) : this()
    {
        Name = name;
        ConnectionId = connectionId;
    }
    
    public void AddScore(int points)
    {
        Score += points;
    }
    
    public void UpdateConnectionId(string connectionId)
    {
        ConnectionId = connectionId;
        IsConnected = true;
    }

    public void MarkAnswerSubmitted()
    {
        AnswerSubmitted = true;
    }

    public void Disconnect()
    {
        IsConnected = false;
    }
}