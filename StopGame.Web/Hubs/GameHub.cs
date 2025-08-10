using Microsoft.AspNetCore.SignalR;
using StopGame.Application.DTOs.Requests;
using StopGame.Application.Interfaces;

namespace StopGame.Web.Hubs;

public class GameHub : Hub
{
    private readonly IRoomService _roomService;
    private readonly IChatService _chatService;

    public GameHub(IRoomService roomService, IChatService chatService)
    {
        _roomService = roomService;
        _chatService = chatService;
    }

    public async Task CreateRoom(CreateRoomRequest request)
    {
        try
        {
            var room = await _roomService.CreateRoomAsync(request, Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, room.Code);
            var currentPlayer = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            await Clients.Caller.SendAsync("RoomCreated", room, currentPlayer);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public async Task JoinRoom(JoinRoomRequest request)
    {
        try
        {
            var room = await _roomService.JoinRoomAsync(request, Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, room.Code);
            var currentPlayer = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            await Clients.Caller.SendAsync("RoomJoined", room, currentPlayer);
            await Clients.Group(room.Code).SendAsync("RoomUpdated", room);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public async Task ReconnectRoom(ReconnectRoomRequest request)
    {
        try
        {
            var room = await _roomService.ReconnectRoom(request, Context.ConnectionId);
            await Groups.AddToGroupAsync(Context.ConnectionId, room.Code);
            var currentPlayer = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            await Clients.Caller.SendAsync("RoomJoined", room, currentPlayer);
            await Clients.Group(room.Code).SendAsync("RoomUpdated", room);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public async Task GetCurrentRoom()
    {
        try
        {
            var room = await _roomService.GetRoomByConnectionIdAsync(Context.ConnectionId);
            if (room == null)
            {
                await Clients.Caller.SendAsync("Error", "Room not found");
                return;
            }
            var currentPlayer = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            await Clients.Caller.SendAsync("RoomJoined", room, currentPlayer);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public async Task UpdateRoomSettings(string roomCode, UpdateRoomSettingsRequest request)
    {
        try
        {
            var room = await _roomService.UpdateRoomSettings(roomCode, request);
            await Clients.Group(room.Code).SendAsync("RoomUpdated", room);
        } 
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public async Task SendChat(string message)
    {
        try
        {
            var room = await _roomService.GetRoomByConnectionIdAsync(Context.ConnectionId);
            if (room == null)
            {
                await Clients.Caller.SendAsync("Error", "Room not found");
                return;
            }

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null)
            {
                await Clients.Caller.SendAsync("Error", "Player not found");
                return;
            }

            await _chatService.SendMessageToRoomAsync(room.Code, player, message);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public async Task StartRound()
    {
        try
        {
            var room = await _roomService.GetRoomByConnectionIdAsync(Context.ConnectionId);
            if (room == null)
            {
                await Clients.Caller.SendAsync("Error", "Room not found");
                return;
            }

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null || !player.IsHost)
            {
                await Clients.Caller.SendAsync("Error", "Only host can start rounds");
                return;
            }

            var updatedRoom = await _roomService.StartRoundAsync(room.Code, player.Id);
            await Clients.Group(room.Code).SendAsync("RoundStarted", updatedRoom);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public async Task SubmitAnswers(SubmitAnswersRequest request)
    {
        try
        {
            var room = await _roomService.GetRoomByConnectionIdAsync(Context.ConnectionId);
            if (room == null)
            {
                await Clients.Caller.SendAsync("Error", "Room not found");
                return;
            }

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null)
            {
                await Clients.Caller.SendAsync("Error", "Player not found");
                return;
            }

            await _roomService.SubmitAnswersAsync(room.Code, player.Id, request);
            // await Clients.Group(room.Code).SendAsync("AnswersSubmitted", new { PlayerId = player.Id, PlayerName = player.Name });
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public async Task Stop()
    {
        try
        {
            var room = await _roomService.GetRoomByConnectionIdAsync(Context.ConnectionId);
            if (room == null)
            {
                await Clients.Caller.SendAsync("Error", "Room not found");
                return;
            }

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null)
            {
                await Clients.Caller.SendAsync("Error", "Player not found");
                return;
            }

            await _roomService.StopRoundAsync(room.Code, player.Id);
            //var votingData = await _roomService.GetVotingDataAsync(room.Code);
            await Clients.Group(room.Code).SendAsync("RoundStopped");
            //await Clients.Group(room.Code).SendAsync("VotingStarted", votingData);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public async Task Vote(VoteRequest request)
    {
        try
        {
            var room = await _roomService.GetRoomByConnectionIdAsync(Context.ConnectionId);
            if (room == null)
            {
                await Clients.Caller.SendAsync("Error", "Room not found");
                return;
            }

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null)
            {
                await Clients.Caller.SendAsync("Error", "Player not found");
                return;
            }

            var updatedRoom = await _roomService.VoteAsync(room.Code, player.Id, request);
            var answersData = await _roomService.GetAnswersDataAsync(room.Code);
            await Clients.Group(room.Code).SendAsync("VoteUpdate", answersData);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public async Task FinishVotingPhase()
    {
        try
        {
            var room = await _roomService.GetRoomByConnectionIdAsync(Context.ConnectionId);
            if (room == null)
            {
                await Clients.Caller.SendAsync("Error", "Room not found");
                return;
            }

            var player = room.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player == null)
            {
                await Clients.Caller.SendAsync("Error", "Player not found");
                return;
            }

            var updatedRoom = await _roomService.FinishVotingPhase(room.Code, player.Id);

            await Clients.Group(room.Code).SendAsync("RoomUpdated", updatedRoom);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public async Task GetVoteData()
    {
        try
        {
            var room = await _roomService.GetRoomByConnectionIdAsync(Context.ConnectionId);
            if (room == null)
            {
                await Clients.Caller.SendAsync("Error", "Room not found");
                return;
            }

            if (room.State != Domain.Enums.RoomState.Voting)
            {
                await Clients.Caller.SendAsync("Error", "Not in voting phase");
                return;
            }

            while (!room!.HasPlayersSubmittedAnswers)
            {
                await Task.Delay(500); // Wait for answers to be submitted
                room = await _roomService.GetRoomAsync(room.Code);
            }

            var votingData = await _roomService.GetAnswersDataAsync(room.Code);
            await Clients.Caller.SendAsync("VoteStarted", votingData);
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public async Task LeaveRoom()
    {
        try
        {
            var room = await _roomService.GetRoomByConnectionIdAsync(Context.ConnectionId);
            if (room != null)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, room.Code);
                await _roomService.LeaveRoomAsync(Context.ConnectionId);
                
                // Notify remaining players
                var updatedRoom = await _roomService.GetRoomAsync(room.Code);
                if (updatedRoom != null)
                {
                    await Clients.Group(room.Code).SendAsync("RoomUpdated", updatedRoom);
                }
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.SendAsync("Error", ex.Message);
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            await LeaveRoom();
        }
        catch
        {
            // Ignore errors during disconnect
        }
        
        await base.OnDisconnectedAsync(exception);
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        await base.OnConnectedAsync();
    }
}