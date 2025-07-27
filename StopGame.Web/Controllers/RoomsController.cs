using Microsoft.AspNetCore.Mvc;
using StopGame.Application.DTOs.Requests;
using StopGame.Application.Interfaces;

namespace StopGame.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoomsController : ControllerBase
{
    private readonly IRoomService _roomService;

    public RoomsController(IRoomService roomService)
    {
        _roomService = roomService;
    }

    [HttpGet]
    public async Task<IActionResult> GetActiveRooms()
    {
        try
        {
            var rooms = await _roomService.GetActiveRoomsAsync();
            return Ok(rooms);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{roomCode}")]
    public async Task<IActionResult> GetRoom(string roomCode)
    {
        try
        {
            var room = await _roomService.GetRoomAsync(roomCode);
            if (room == null)
            {
                return NotFound(new { error = "Room not found" });
            }
            return Ok(room);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var room = await _roomService.CreateRoomAsync(request, null);
            return CreatedAtAction(nameof(GetRoom), new { roomCode = room.Code }, room);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{roomCode}/join")]
    public async Task<IActionResult> JoinRoom(string roomCode, [FromBody] JoinRoomRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            request.RoomCode = roomCode;
            var room = await _roomService.JoinRoomAsync(request, null);
            return Ok(room);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{roomCode}/start-round")]
    public async Task<IActionResult> StartRound(string roomCode, [FromBody] StartRoundRequest request)
    {
        try
        {
            var room = await _roomService.StartRoundAsync(roomCode, request.PlayerId);
            return Ok(room);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{roomCode}/submit-answers")]
    public async Task<IActionResult> SubmitAnswers(string roomCode, [FromBody] SubmitAnswersWithPlayerRequest request)
    {
        try
        {
            var room = await _roomService.SubmitAnswersAsync(roomCode, request.PlayerId, request.SubmitAnswersRequest);
            return Ok(room);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{roomCode}/stop")]
    public async Task<IActionResult> StopRound(string roomCode, [FromBody] StopRoundRequest request)
    {
        try
        {
            await _roomService.StopRoundAsync(roomCode, request.PlayerId);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{roomCode}/vote")]
    public async Task<IActionResult> Vote(string roomCode, [FromBody] VoteWithPlayerRequest request)
    {
        try
        {
            var room = await _roomService.VoteAsync(roomCode, request.PlayerId, request.VoteRequest);
            return Ok(room);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{roomCode}/leave")]
    public async Task<IActionResult> LeaveRoom(string roomCode, [FromQuery] string connectionId)
    {
        try
        {
            await _roomService.LeaveRoomAsync(connectionId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}

// Additional request DTOs for API endpoints
public class StartRoundRequest
{
    public Guid PlayerId { get; set; }
}

public class StopRoundRequest
{
    public Guid PlayerId { get; set; }
}

public class SubmitAnswersWithPlayerRequest
{
    public Guid PlayerId { get; set; }
    public SubmitAnswersRequest SubmitAnswersRequest { get; set; } = new();
}

public class VoteWithPlayerRequest
{
    public Guid PlayerId { get; set; }
    public VoteRequest VoteRequest { get; set; } = new();
}