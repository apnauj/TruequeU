using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TruequeU.Interfaces;
using TruequeU.Models.DTOs;

namespace TruequeU.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConversationsController : ControllerBase
{
    private readonly IConversationService _conversationService;
    private readonly IMessageService _messageService;

    public ConversationsController(
        IConversationService conversationService,
        IMessageService messageService)
    {
        _conversationService = conversationService;
        _messageService = messageService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConversationReadDto>>> GetMyConversations()
    {
        var userId = GetCurrentUserId();
        var conversations = await _conversationService.GetUserConversationsAsync(userId);
        return Ok(conversations);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ConversationReadDto>> GetById(Guid id)
    {
        var userId = GetCurrentUserId();
        var conversation = await _conversationService.GetConversationByIdAsync(id, userId);

        if (conversation is null)
            return NotFound();

        return Ok(conversation);
    }

    [HttpPost]
    public async Task<ActionResult<ConversationReadDto>> Create([FromBody] ConversationCreateDto dto)
    {
        var buyerId = GetCurrentUserId();

        try
        {
            var conversation = await _conversationService.CreateConversationAsync(buyerId, dto);
            return CreatedAtAction(nameof(GetById), new { id = conversation.Id }, conversation);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _conversationService.DeleteConversationAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpGet("{conversationId:guid}/messages")]
    public async Task<ActionResult<IEnumerable<MessageReadDto>>> GetMessages(Guid conversationId)
    {
        var messages = await _messageService.GetMessagesAsync(conversationId);
        return Ok(messages);
    }

    [HttpPost("{conversationId:guid}/messages")]
    public async Task<ActionResult<MessageReadDto>> SendMessage(Guid conversationId, [FromBody] MessageCreateDto dto)
    {
        var senderId = GetCurrentUserId();

        try
        {
            var message = await _messageService.SendMessageAsync(conversationId, senderId, dto);
            return CreatedAtAction(nameof(GetMessages), new { conversationId }, message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{conversationId:guid}/messages/{messageId:guid}/read")]
    public async Task<IActionResult> MarkMessageAsRead(Guid conversationId, Guid messageId)
    {
        await _messageService.MarkAsReadAsync(messageId);
        return NoContent();
    }

    [HttpPatch("{conversationId:guid}/read-all")]
    public async Task<IActionResult> MarkAllAsRead(Guid conversationId)
    {
        var userId = GetCurrentUserId();
        await _messageService.MarkAllAsReadAsync(conversationId, userId);
        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(userId!);
    }
}
