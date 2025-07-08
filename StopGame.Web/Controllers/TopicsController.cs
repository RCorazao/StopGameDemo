using Microsoft.AspNetCore.Mvc;
using StopGame.Application.Interfaces;
using StopGame.Domain.Entities;

namespace StopGame.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TopicsController : ControllerBase
{
    private readonly ITopicRepository _topicRepository;

    public TopicsController(ITopicRepository topicRepository)
    {
        _topicRepository = topicRepository;
    }

    [HttpGet("default")]
    public async Task<IActionResult> GetDefaultTopics()
    {
        try
        {
            var topics = await _topicRepository.GetDefaultTopicsAsync();
            return Ok(topics.Select(t => new
            {
                t.Id,
                t.Name,
                t.IsDefault,
                t.CreatedAt
            }));
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("custom/{userId:guid}")]
    public async Task<IActionResult> GetCustomTopics(Guid userId)
    {
        try
        {
            var topics = await _topicRepository.GetCustomTopicsByUserAsync(userId);
            return Ok(topics.Select(t => new
            {
                t.Id,
                t.Name,
                t.IsDefault,
                t.CreatedByUserId,
                t.CreatedAt
            }));
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("custom")]
    public async Task<IActionResult> CreateCustomTopic([FromBody] CreateTopicRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var topic = new Topic(request.Name, false, request.UserId);
            var createdTopic = await _topicRepository.CreateCustomTopicAsync(topic);
            
            return CreatedAtAction(nameof(GetTopicById), new { id = createdTopic.Id }, new
            {
                createdTopic.Id,
                createdTopic.Name,
                createdTopic.IsDefault,
                createdTopic.CreatedByUserId,
                createdTopic.CreatedAt
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetTopicById(Guid id)
    {
        try
        {
            var topic = await _topicRepository.GetByIdAsync(id);
            if (topic == null)
            {
                return NotFound(new { error = "Topic not found" });
            }

            return Ok(new
            {
                topic.Id,
                topic.Name,
                topic.IsDefault,
                topic.CreatedByUserId,
                topic.CreatedAt
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCustomTopic(Guid id, [FromQuery] Guid userId)
    {
        try
        {
            await _topicRepository.DeleteCustomTopicAsync(id, userId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

public class CreateTopicRequest
{
    public string Name { get; set; } = string.Empty;
    public Guid UserId { get; set; }
}