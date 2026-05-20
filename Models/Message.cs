using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TruequeU.Models;

[Index(nameof(ConversationId), nameof(SentAt))]
public class Message
{
    [Key]
    public Guid Id { get; private set; }

    [Required]
    public Guid ConversationId { get; set; }

    [ForeignKey(nameof(ConversationId))]
    public Conversation Conversation { get; set; } = null!;

    [Required]
    public Guid SenderId { get; set; } 

    [ForeignKey(nameof(SenderId))]
    public User Sender { get; set; } = null!; 

    [Required]
    [MaxLength(2000)] 
    public string Content { get; set; }

    public DateTime SentAt { get; private set; }

    public bool IsRead { get; set; }
    
    private Message() { }
    
    public Message(Guid conversationId, Guid senderId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("El contenido del mensaje no puede estar vacío.");

        Id = Guid.NewGuid();
        ConversationId = conversationId;
        SenderId = senderId;
        Content = content;
        
        SentAt = DateTime.UtcNow;
        IsRead = false;
    }
}