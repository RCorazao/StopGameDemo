using StopGame.Application.DTOs;
using StopGame.Application.DTOs.Requests;
using StopGame.Application.Interfaces;
using StopGame.Domain.Entities;
using StopGame.Domain.Enums;
using StopGame.Domain.ValueObjects;

namespace StopGame.Application.Services;

public class RoomService : IRoomService
{
    private readonly IRoomRepository _roomRepository;
    private readonly ITopicRepository _topicRepository;
    private readonly IChatService _chatService;
    private readonly IAppJobScheduler _jobScheduler;

    public RoomService(
        IRoomRepository roomRepository,
        ITopicRepository topicRepository,
        IChatService chatService,
        IAppJobScheduler jobScheduler)
    {
        _roomRepository = roomRepository;
        _topicRepository = topicRepository;
        _chatService = chatService;
        _jobScheduler = jobScheduler;
    }

    public async Task<RoomDto> CreateRoomAsync(CreateRoomRequest request, string connectionId)
    {
        var room = new Room
        {
            MaxPlayers = request.MaxPlayers,
            RoundDurationSeconds = request.RoundDurationSeconds,
            VotingDurationSeconds = request.VotingDurationSeconds,
            MaxRounds = request.MaxRounds
        };

        // Add topics
        if (request.UseDefaultTopics)
        {
            var defaultTopics = await _topicRepository.GetDefaultTopicsAsync();
            room.Topics.AddRange(defaultTopics);
        }

        foreach (var customTopicName in request.CustomTopics)
        {
            room.Topics.Add(new Topic(customTopicName));
        }

        // Create host player
        var hostPlayer = new Player(request.HostName, connectionId);
        room.HostUserId = hostPlayer.Id;
        room.AddPlayer(hostPlayer);

        var createdRoom = await _roomRepository.CreateAsync(room);
        
        await _chatService.NotifyPlayerJoinedAsync(createdRoom.Code, request.HostName);
        
        return MapToDto(createdRoom);
    }

    public async Task<RoomDto> JoinRoomAsync(JoinRoomRequest request, string connectionId)
    {
        var room = await _roomRepository.GetByCodeAsync(request.RoomCode);
        if (room == null)
            throw new InvalidOperationException("Room not found");

        var exists = room.Players.Exists(p => p.ConnectionId == connectionId);

        if (exists)
        {
            await _chatService.NotifyPlayerJoinedAsync(room.Code, request.PlayerName);
            return MapToDto(room);
        }

        if (!room.CanJoin())
            throw new InvalidOperationException("Cannot join room");

        var player = new Player(request.PlayerName, connectionId);
        room.AddPlayer(player);

        var updatedRoom = await _roomRepository.UpdateAsync(room);
        
        await _chatService.NotifyPlayerJoinedAsync(room.Code, request.PlayerName);
        
        return MapToDto(updatedRoom);
    }

    public async Task<RoomDto> UpdateRoomSettings(string roomCode , UpdateRoomSettingsRequest request)
    {
        var room = await _roomRepository.GetByCodeAsync(roomCode);
        if (room == null)
            throw new InvalidOperationException("Room not found");

        room.Topics = new List<Topic>();
        foreach (var customTopicName in request.Topics)
        {
            room.Topics.Add(new Topic(customTopicName));
        }

        room.MaxPlayers = request.MaxPlayers;
        room.RoundDurationSeconds = request.RoundDurationSeconds;
        room.VotingDurationSeconds = request.VotingDurationSeconds;
        room.MaxRounds = request.MaxRounds;

        var updatedRoom = await _roomRepository.UpdateAsync(room);

        return MapToDto(updatedRoom);
    }

    public async Task<RoomDto?> GetRoomAsync(string roomCode)
    {
        var room = await _roomRepository.GetByCodeAsync(roomCode);
        return room != null ? MapToDto(room) : null;
    }

    public async Task<RoomDto?> GetRoomByConnectionIdAsync(string connectionId)
    {
        var room = await _roomRepository.GetByConnectionIdAsync(connectionId);
        return room != null ? MapToDto(room) : null;
    }

    public async Task LeaveRoomAsync(string connectionId)
    {
        var room = await _roomRepository.GetByConnectionIdAsync(connectionId);
        if (room == null) return;

        var player = room.GetPlayerByConnectionId(connectionId);
        if (player == null) return;

        var playerName = player.Name;
        room.RemovePlayer(player.Id);

        if (room.Players.Any())
        {
            await _roomRepository.UpdateAsync(room);
            await _chatService.NotifyPlayerLeftAsync(room.Code, playerName);
        }
        else
        {
            await _roomRepository.DeleteAsync(room.Code);
        }
    }

    public async Task<RoomDto> StartRoundAsync(string roomCode, Guid hostUserId)
    {
        var room = await _roomRepository.GetByCodeAsync(roomCode);
        if (room == null)
            throw new InvalidOperationException("Room not found");

        if (!room.IsHost(hostUserId))
            throw new UnauthorizedAccessException("Only host can start rounds");

        if (!room.CanStartNewRound())
            throw new InvalidOperationException("Cannot start new round");

        room.StartNewRound();
        room.ResetPlayerSubmissions();
        var updatedRoom = await _roomRepository.UpdateAsync(room);
        
        var currentRound = room.GetCurrentRound()!;
        await _chatService.NotifyRoundStartedAsync(room.Code, currentRound.Letter, room.RoundDurationSeconds);
        
        return MapToDto(updatedRoom);
    }

    public Task SubmitAnswersAsync(string roomCode, Guid playerId, SubmitAnswersRequest request)
    {
        _jobScheduler.Enqueue<IAnswerSubmissionService>(x => x.ProcessAnswersAsync(roomCode, playerId, request));
        return Task.CompletedTask;
    }

    public async Task StopRoundAsync(string roomCode, Guid playerId)
    {
        var room = await _roomRepository.GetByCodeAsync(roomCode);
        if (room == null)
            throw new InvalidOperationException("Room not found");

        var currentRound = room.GetCurrentRound();
        if (currentRound == null || !currentRound.IsActive)
            throw new InvalidOperationException("No active round");

        var player = room.GetPlayer(playerId);
        if (player == null)
            throw new InvalidOperationException("Player not found in room");

        //room.EndCurrentRound();
        //var updatedRoom = await _roomRepository.UpdateAsync(room);
        
        await _chatService.NotifyRoundEndedAsync(room.Code);
        await _chatService.NotifyVotingStartedAsync(room.Code, room.VotingDurationSeconds);
        
        //return MapToDto(updatedRoom);
    }

    public async Task<RoomDto> VoteAsync(string roomCode, Guid voterId, VoteRequest request)
    {
        var room = await _roomRepository.GetByCodeAsync(roomCode);
        if (room == null)
            throw new InvalidOperationException("Room not found");

        if (room.State != RoomState.Voting)
            throw new InvalidOperationException("Not in voting phase");

        var currentRound = room.GetCurrentRound();
        if (currentRound == null)
            throw new InvalidOperationException("No current round");

        var voter = room.GetPlayer(voterId);
        if (voter == null)
            throw new InvalidOperationException("Voter not found in room");

        var answer = currentRound.GetAnswerById(request.AnswerId);

        if (answer == null)
            throw new InvalidOperationException("Answer not found");

        var vote = new Vote(voterId, answer.PlayerId, answer.TopicId, request.IsValid);
        answer.AddVote(vote);

        var updatedRoom = await _roomRepository.UpdateAsync(room);
        
        return MapToDto(updatedRoom);
    }

    public async Task<List<VoteAnswerDto>> GetAnswersDataAsync(string roomCode)
    {
        var room = await _roomRepository.GetByCodeAsync(roomCode);
        if (room == null)
            throw new InvalidOperationException("Room not found");

        var currentRound = room.GetCurrentRound();
        if (currentRound == null)
            throw new InvalidOperationException("No current round");
        
        List<VoteAnswerDto> answersData = currentRound.Answers
            .GroupBy(a => new { a.TopicId, a.TopicName })
            .Select(g => new VoteAnswerDto
            {
                TopicId = g.Key.TopicId,
                TopicName = g.Key.TopicName,
                Answers = g.Select(a =>
                {
                    var player = room.GetPlayer(a.PlayerId);
                    return new AnswerDto
                    {
                        Id = a.Id,
                        TopicId = a.TopicId,
                        PlayerId = a.PlayerId,
                        PlayerName = player?.Name ?? "Unknown",
                        TopicName = a.TopicName,
                        Value = a.Value,
                        CreatedAt = a.CreatedAt,
                        Votes = a.Votes.Select(v => MapVoteToDto(room, v)).ToList()
                    };
                }).ToList()
            })
            .ToList();

        return answersData;
    }

    public async Task<RoomDto> FinishVotingPhase(string roomCode, Guid playerId)
    {
        var room = await _roomRepository.GetByCodeAsync(roomCode);
        if (room == null)
            throw new InvalidOperationException("Room not found");

        if (room.State != RoomState.Voting)
            throw new InvalidOperationException("Not in voting phase");

        if (!room.IsHost(playerId))
            throw new UnauthorizedAccessException("Only host can finish voting phase");

        room.CalculateScores();
        room.EndVoting();

        var updatedRoom = await _roomRepository.UpdateAsync(room);
        await _chatService.NotifyVotingEndedAsync(room.Code);

        return MapToDto(updatedRoom);
    }

    public async Task<List<RoomDto>> GetActiveRoomsAsync()
    {
        var rooms = await _roomRepository.GetActiveRoomsAsync();
        return rooms.Select(MapToDto).ToList();
    }

    public async Task CleanupExpiredRoomsAsync()
    {
        var expiredRooms = await _roomRepository.GetExpiredRoomsAsync();
        foreach (var room in expiredRooms)
        {
            await _roomRepository.DeleteAsync(room.Code);
        }
    }

    public async Task UpdatePlayerConnectionAsync(string oldConnectionId, string newConnectionId)
    {
        var room = await _roomRepository.GetByConnectionIdAsync(oldConnectionId);
        if (room == null) return;

        var player = room.GetPlayerByConnectionId(oldConnectionId);
        if (player != null)
        {
            player.UpdateConnectionId(newConnectionId);
            await _roomRepository.UpdateAsync(room);
        }
    }

    private static RoomDto MapToDto(Room room)
    {
        return new RoomDto
        {
            Id = room.Id,
            Code = room.Code,
            HostUserId = room.HostUserId,
            Topics = room.Topics.Select(MapTopicToDto).ToList(),
            Players = room.Players.Select(p => MapPlayerToDto(p, room.IsHost(p.Id))).ToList(),
            State = room.State,
            Rounds = room.Rounds.Select(r => MapRoundToDto(room, r)).ToList(),
            CreatedAt = room.CreatedAt,
            ExpiresAt = room.ExpiresAt,
            MaxPlayers = room.MaxPlayers,
            RoundDurationSeconds = room.RoundDurationSeconds,
            VotingDurationSeconds = room.VotingDurationSeconds,
            MaxRounds = room.MaxRounds,
            CurrentRound = room.GetCurrentRound() != null ? MapRoundToDto(room, room.GetCurrentRound()!) : null,
            HasPlayersSubmittedAnswers = room.HasPlayersSubmittedAnswers()
        };
    }

    private static PlayerDto MapPlayerToDto(Player player, bool isHost = false)
    {
        return new PlayerDto
        {
            Id = player.Id,
            ConnectionId = player.ConnectionId,
            Name = player.Name,
            Score = player.Score,
            IsConnected = player.IsConnected,
            JoinedAt = player.JoinedAt,
            IsHost = isHost
        };
    }

    private static TopicDto MapTopicToDto(Topic topic)
    {
        return new TopicDto
        {
            Id = topic.Id,
            Name = topic.Name,
            IsDefault = topic.IsDefault,
            CreatedByUserId = topic.CreatedByUserId,
            CreatedAt = topic.CreatedAt
        };
    }

    private static RoundDto MapRoundToDto(Room room, Round round)
    {
        return new RoundDto
        {
            Id = round.Id,
            Letter = round.Letter,
            StartedAt = round.StartedAt,
            EndedAt = round.EndedAt,
            Answers = round.Answers.Select(a => MapAnswerToDto(room, a)).ToList(),
            IsActive = round.IsActive,
            TimeRemainingSeconds = round.IsActive ? 
                Math.Max(0, 60 - (int)(DateTime.UtcNow - round.StartedAt).TotalSeconds) : 0
        };
    }

    private static AnswerDto MapAnswerToDto(Room room, Answer answer)
    {
        return new AnswerDto
        {
            Id = answer.Id,
            TopicId = answer.TopicId,
            PlayerId = answer.PlayerId,
            TopicName = answer.TopicName,
            Value = answer.Value,
            CreatedAt = answer.CreatedAt,
            Votes = answer.Votes.Select(v => MapVoteToDto(room, v)).ToList()
        };
    }

    private static VoteDto MapVoteToDto(Room room, Vote vote)
    {
        return new VoteDto
        {
            VoterId = vote.VoterId,
            VoterName = room.GetPlayer(vote.VoterId)?.Name ?? "Unknown",
            AnswerOwnerId = vote.AnswerOwnerId,
            AnswerOwnerName = room.GetPlayer(vote.AnswerOwnerId)?.Name ?? "Unknown",
            TopicId = vote.TopicId,
            TopicName = room.GetTopicById(vote.TopicId)?.Name ?? "Unknown",
            IsValid = vote.IsValid,
            CreatedAt = vote.CreatedAt
        };
    }
}