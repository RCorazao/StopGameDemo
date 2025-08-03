using StopGame.Application.DTOs;
using StopGame.Application.DTOs.Requests;
using StopGame.Application.Interfaces;
using StopGame.Domain.Entities;
using StopGame.Domain.ValueObjects;

namespace StopGame.Application.Services;

public class AnswerSubmissionService : IAnswerSubmissionService
{
    private readonly IRoomRepository _roomRepository;
    private readonly ISignalRService _signalRService;

    public AnswerSubmissionService(IRoomRepository roomRepository, ISignalRService signalRService)
    {
        _roomRepository = roomRepository;
        _signalRService = signalRService;
    }

    public async Task ProcessAnswersAsync(string roomCode, Guid playerId, SubmitAnswersRequest request)
    {
        var room = await _roomRepository.GetByCodeAsync(roomCode);
        if (room == null) return;

        var currentRound = room.GetCurrentRound();
        if (currentRound == null || !currentRound.IsActive) return;

        var player = room.GetPlayer(playerId);
        if (player == null) return;

        foreach (var answer in request.Answers)
        {
            var answerRecord = new Answer()
            {
                PlayerId = playerId,
                TopicId = Guid.Parse(answer.Key),
                TopicName = room.GetTopicById(Guid.Parse(answer.Key))?.Name ?? string.Empty,
                Value = answer.Value,
            };

            currentRound.AddAnswer(answerRecord);
        }

        player.MarkAnswerSubmitted();

        if (room.HasPlayersSubmittedAnswers())
        {
            room.EndCurrentRound();
            room.InitializeVotes();
            var updatedRoom = await _roomRepository.UpdateAsync(room);
            await _signalRService.SendToGroupAsync(roomCode, "RoomUpdated", updatedRoom);

            var answersData = await GetAnswersDataAsync(roomCode);
            await _signalRService.SendToGroupAsync(roomCode, "VoteStarted", answersData);
        }
        else
        {
            await _roomRepository.UpdateAsync(room);
        }
    }

    private async Task<List<VoteAnswerDto>> GetAnswersDataAsync(string roomCode)
    {
        var room = await _roomRepository.GetByCodeAsync(roomCode);
        if (room == null) return new List<VoteAnswerDto>();

        var currentRound = room.GetCurrentRound();
        if (currentRound == null) return new List<VoteAnswerDto>();

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
