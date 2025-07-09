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

    public RoomService(
        IRoomRepository roomRepository,
        ITopicRepository topicRepository,
        IChatService chatService)
    {
        _roomRepository = roomRepository;
        _topicRepository = topicRepository;
        _chatService = chatService;
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
        var updatedRoom = await _roomRepository.UpdateAsync(room);
        
        var currentRound = room.GetCurrentRound()!;
        await _chatService.NotifyRoundStartedAsync(room.Code, currentRound.Letter, room.RoundDurationSeconds);
        
        return MapToDto(updatedRoom);
    }

    public async Task<RoomDto> SubmitAnswersAsync(string roomCode, Guid playerId, SubmitAnswersRequest request)
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

        foreach (var answer in request.Answers)
        {
            var submission = new Submission(
                playerId,
                answer.Key,
                new Answer(answer.Value, answer.Key)
            );
            currentRound.AddSubmission(submission);
        }

        var updatedRoom = await _roomRepository.UpdateAsync(room);
        return MapToDto(updatedRoom);
    }

    public async Task<RoomDto> StopRoundAsync(string roomCode, Guid playerId)
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

        room.EndCurrentRound();
        var updatedRoom = await _roomRepository.UpdateAsync(room);
        
        await _chatService.NotifyRoundEndedAsync(room.Code);
        await _chatService.NotifyVotingStartedAsync(room.Code, room.VotingDurationSeconds);
        
        return MapToDto(updatedRoom);
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

        foreach (var voteItem in request.Votes)
        {
            var vote = new Vote(voterId, voteItem.AnswerOwnerId, voteItem.TopicName, voteItem.IsValid);
            currentRound.AddVote(vote);
        }

        var updatedRoom = await _roomRepository.UpdateAsync(room);
        
        // Check if all players have voted
        var totalVotesNeeded = room.Players.Count * room.Topics.Count * (room.Players.Count - 1);
        if (currentRound.Votes.Count >= totalVotesNeeded)
        {
            // Calculate scores and end voting
            var scores = currentRound.CalculateScores(room.Topics.Select(t => t.Name).ToList());
            foreach (var score in scores)
            {
                var player = room.GetPlayer(score.Key);
                player?.AddScore(score.Value);
            }
            
            room.EndVoting();
            updatedRoom = await _roomRepository.UpdateAsync(room);
            
            await _chatService.NotifyVotingEndedAsync(room.Code);
            
            if (room.State == RoomState.Finished)
            {
                var finalScores = room.Players.OrderByDescending(p => p.Score).ToList();
                await _chatService.NotifyGameFinishedAsync(room.Code, finalScores.Select(p => MapPlayerToDto(p)).ToList());
            }
        }
        
        return MapToDto(updatedRoom);
    }

    public async Task<Dictionary<string, List<SubmissionDto>>> GetVotingDataAsync(string roomCode)
    {
        var room = await _roomRepository.GetByCodeAsync(roomCode);
        if (room == null)
            throw new InvalidOperationException("Room not found");

        var currentRound = room.GetCurrentRound();
        if (currentRound == null)
            throw new InvalidOperationException("No current round");

        var votingData = new Dictionary<string, List<SubmissionDto>>();
        
        foreach (var topic in room.Topics)
        {
            var topicSubmissions = currentRound.GetSubmissionsForTopic(topic.Name)
                .Select(s => 
                {
                    var player = room.GetPlayer(s.PlayerId);
                    var submission = MapSubmissionToDto(s, player?.Name ?? "");
                    
                    // Add vote counts
                    var votes = currentRound.Votes.Where(v => 
                        v.AnswerOwnerId == s.PlayerId && v.TopicName == topic.Name).ToList();
                    submission.VotesValid = votes.Count(v => v.IsValid);
                    submission.VotesInvalid = votes.Count(v => !v.IsValid);
                    submission.IsValid = submission.VotesValid >= submission.VotesInvalid;
                    
                    return submission;
                })
                .ToList();
            
            votingData[topic.Name] = topicSubmissions;
        }
        
        return votingData;
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
            Rounds = room.Rounds.Select(r => MapRoundToDto(r, room)).ToList(),
            CreatedAt = room.CreatedAt,
            ExpiresAt = room.ExpiresAt,
            MaxPlayers = room.MaxPlayers,
            RoundDurationSeconds = room.RoundDurationSeconds,
            VotingDurationSeconds = room.VotingDurationSeconds,
            MaxRounds = room.MaxRounds,
            CurrentRound = room.GetCurrentRound() != null ? MapRoundToDto(room.GetCurrentRound()!, room) : null
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

    private static RoundDto MapRoundToDto(Round round, Room? room = null)
    {
        var submissions = round.Submissions.Select(s => 
        {
            var playerName = room?.GetPlayer(s.PlayerId)?.Name ?? "";
            return MapSubmissionToDto(s, playerName);
        }).ToList();

        return new RoundDto
        {
            Id = round.Id,
            Letter = round.Letter,
            StartedAt = round.StartedAt,
            EndedAt = round.EndedAt,
            Submissions = submissions,
            Votes = round.Votes.Select(MapVoteToDto).ToList(),
            IsActive = round.IsActive,
            TimeRemainingSeconds = round.IsActive ? 
                Math.Max(0, 60 - (int)(DateTime.UtcNow - round.StartedAt).TotalSeconds) : 0
        };
    }

    private static SubmissionDto MapSubmissionToDto(Submission submission, string playerName = "")
    {
        return new SubmissionDto
        {
            PlayerId = submission.PlayerId,
            PlayerName = playerName,
            TopicName = submission.TopicName,
            Answer = new AnswerDto
            {
                Word = submission.Answer.Word,
                TopicName = submission.Answer.TopicName
            },
            SubmittedAt = submission.SubmittedAt
        };
    }

    private static VoteDto MapVoteToDto(Vote vote)
    {
        return new VoteDto
        {
            VoterId = vote.VoterId,
            AnswerOwnerId = vote.AnswerOwnerId,
            TopicName = vote.TopicName,
            IsValid = vote.IsValid,
            CreatedAt = vote.CreatedAt
        };
    }
}