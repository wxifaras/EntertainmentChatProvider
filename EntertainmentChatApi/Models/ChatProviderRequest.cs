using System.ComponentModel.DataAnnotations;

namespace EntertainmentChatApi.Models
{
    public class ChatProviderRequest
    {
        public string? SessionId { get; set; }

        public string? UserId { get; set; }
        [Required]
        public required string Prompt { get; set; }
    }
}
