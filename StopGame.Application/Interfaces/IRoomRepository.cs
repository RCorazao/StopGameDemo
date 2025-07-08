using StopGame.Domain.Entities;

namespace StopGame.Application.Interfaces;

public interface IRoomRepository
{
    Task<Room?> GetByCodeAsync(string code);
    Task<Room?> GetByConnectionIdAsync(string connectionId);
    Task<Room> CreateAsync(Room room);
    Task<Room> UpdateAsync(Room room);
    Task DeleteAsync(string code);
    Task<List<Room>> GetActiveRoomsAsync();
    Task<List<Room>> GetExpiredRoomsAsync();
    Task<bool> ExistsAsync(string code);
}