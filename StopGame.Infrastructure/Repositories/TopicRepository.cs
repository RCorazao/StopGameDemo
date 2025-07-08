using Microsoft.EntityFrameworkCore;
using StopGame.Application.Interfaces;
using StopGame.Domain.Entities;
using StopGame.Infrastructure.Data;

namespace StopGame.Infrastructure.Repositories;

public class TopicRepository : ITopicRepository
{
    private readonly StopGameDbContext _context;

    public TopicRepository(StopGameDbContext context)
    {
        _context = context;
    }

    public async Task<List<Topic>> GetDefaultTopicsAsync()
    {
        return await _context.Topics
            .Where(t => t.IsDefault)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<List<Topic>> GetCustomTopicsByUserAsync(Guid userId)
    {
        return await _context.Topics
            .Where(t => !t.IsDefault && t.CreatedByUserId == userId)
            .OrderBy(t => t.Name)
            .ToListAsync();
    }

    public async Task<Topic> CreateCustomTopicAsync(Topic topic)
    {
        _context.Topics.Add(topic);
        await _context.SaveChangesAsync();
        return topic;
    }

    public async Task<Topic?> GetByIdAsync(Guid id)
    {
        return await _context.Topics.FindAsync(id);
    }

    public async Task<List<Topic>> GetByIdsAsync(List<Guid> ids)
    {
        return await _context.Topics
            .Where(t => ids.Contains(t.Id))
            .ToListAsync();
    }

    public async Task DeleteCustomTopicAsync(Guid id, Guid userId)
    {
        var topic = await _context.Topics
            .FirstOrDefaultAsync(t => t.Id == id && t.CreatedByUserId == userId && !t.IsDefault);
        
        if (topic != null)
        {
            _context.Topics.Remove(topic);
            await _context.SaveChangesAsync();
        }
    }
}