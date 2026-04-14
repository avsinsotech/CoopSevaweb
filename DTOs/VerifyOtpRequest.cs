using System.ComponentModel.DataAnnotations;

namespace AVSBackend.DTOs
{
    public class VerifyOtpRequest
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        public string OtpCode { get; set; } = string.Empty;
    }
}
