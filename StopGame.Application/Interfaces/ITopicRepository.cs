using StopGame.Domain.Entities;

namespace StopGame.Application.Interfaces;

public interface ITopicRepository
{
    Task<List<Topic>> GetDefaultTopicsAsync();
    Task<List<Topic>> GetCustomTopicsByUserAsync(Guid userId);
    Task<Topic> CreateCustomTopicAsync(Topic topic);
    Task<Topic?> GetByIdAsync(Guid id);
    Task<List<Topic>> GetByIdsAsync(List<Guid> ids);
    Task DeleteCustomTopicAsync(Guid id, Guid userId);
}