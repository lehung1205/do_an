using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using JobPortal.API.Models.Auth;

namespace JobPortal.API.Models;

[Table("chat_messages")]
public class ChatMessage
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("application_id")]
    public long ApplicationId { get; set; }

    [ForeignKey(nameof(ApplicationId))]
    public Application Application { get; set; } = null!;

    [Column("sender_user_id")]
    public long SenderUserId { get; set; }

    [ForeignKey(nameof(SenderUserId))]
    public User Sender { get; set; } = null!;

    [Column("content", TypeName = "text")]
    public string Content { get; set; } = null!;

    [Column("sent_at")]
    public DateTime SentAt { get; set; }

    [Column("read_at")]
    public DateTime? ReadAt { get; set; }
}
