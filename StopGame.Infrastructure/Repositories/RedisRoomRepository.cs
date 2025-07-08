using StackExchange.Redis;
using System.Text.Json;
using StopGame.Application.Interfaces;
using StopGame.Domain.Entities;

namespace StopGame.Infrastructure.Repositories;

public class RedisRoomRepository : IRoomRepository
{
    private readonly IDatabase _database;
    private const string RoomKeyPrefix = "room:";
    private const string ConnectionRoomKeyPrefix = "connection:";
    private const string ActiveRoomsKey = "active_rooms";

    public RedisRoomRepository(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task<Room?> GetByCodeAsync(string code)
    {
        var roomData = await _database.StringGetAsync(GetRoomKey(code));
        if (!roomData.HasValue)
            return null;

        return JsonSerializer.Deserialize<Room>(roomData!);
    }

    public async Task<Room?> GetByConnectionIdAsync(string connectionId)
    {
        var roomCode = await _database.StringGetAsync(GetConnectionKey(connectionId));
        if (!roomCode.HasValue)
            return null;

        return await GetByCodeAsync(roomCode!);
    }

    public async Task<Room> CreateAsync(Room room)
    {
        var roomKey = GetRoomKey(room.Code);
        var roomData = JsonSerializer.Serialize(room);
        
        await _database.StringSetAsync(roomKey, roomData, TimeSpan.FromHours(2));
        await _database.SetAddAsync(ActiveRoomsKey, room.Code);
        
        // Map all player connections to this room
        foreach (var player in room.Players)
        {
            await _database.StringSetAsync(GetConnectionKey(player.ConnectionId), room.Code, TimeSpan.FromHours(2));
        }
        
        return room;
    }

    public async Task<Room> UpdateAsync(Room room)
    {
        var roomKey = GetRoomKey(room.Code);
        var roomData = JsonSerializer.Serialize(room);
        
        await _database.StringSetAsync(roomKey, roomData, TimeSpan.FromHours(2));
        
        // Update connection mappings
        foreach (var player in room.Players)
        {
            await _database.StringSetAsync(GetConnectionKey(player.ConnectionId), room.Code, TimeSpan.FromHours(2));
        }
        
        return room;
    }

    public async Task DeleteAsync(string code)
    {
        var room = await GetByCodeAsync(code);
        if (room != null)
        {
            // Remove connection mappings
            foreach (var player in room.Players)
            {
                await _database.KeyDeleteAsync(GetConnectionKey(player.ConnectionId));
            }
        }
        
        await _database.KeyDeleteAsync(GetRoomKey(code));
        await _database.SetRemoveAsync(ActiveRoomsKey, code);
    }

    public async Task<List<Room>> GetActiveRoomsAsync()
    {
        var roomCodes = await _database.SetMembersAsync(ActiveRoomsKey);
        var rooms = new List<Room>();
        
        foreach (var roomCode in roomCodes)
        {
            var room = await GetByCodeAsync(roomCode!);
            if (room != null)
            {
                rooms.Add(room);
            }
        }
        
        return rooms;
    }

    public async Task<List<Room>> GetExpiredRoomsAsync()
    {
        var activeRooms = await GetActiveRoomsAsync();
        return activeRooms.Where(r => r.IsExpired()).ToList();
    }

    public async Task<bool> ExistsAsync(string code)
    {
        return await _database.KeyExistsAsync(GetRoomKey(code));
    }

    private static string GetRoomKey(string code) => $"{RoomKeyPrefix}{code}";
    private static string GetConnectionKey(string connectionId) => $"{ConnectionRoomKeyPrefix}{connectionId}";
}