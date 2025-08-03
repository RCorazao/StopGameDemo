using StopGame.Application.DTOs;
using StopGame.Domain.Entities;
using StopGame.Domain.ValueObjects;
using StopGame.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StopGame.Application.Mappings
{
    public static class RoomMappings
    {
        public static RoomDto MapToDto(Room room)
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

        public static PlayerDto MapPlayerToDto(Player player, bool isHost = false)
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

        public static TopicDto MapTopicToDto(Topic topic)
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

        public static RoundDto MapRoundToDto(Room room, Round round)
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

        public static AnswerDto MapAnswerToDto(Room room, Answer answer)
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

        public static VoteDto MapVoteToDto(Room room, Vote vote)
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
            };
        }
    }
}
