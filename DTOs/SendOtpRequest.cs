using System.ComponentModel.DataAnnotations;

namespace AVSBackend.DTOs
{
    public class SendOtpRequest
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
    }
}
