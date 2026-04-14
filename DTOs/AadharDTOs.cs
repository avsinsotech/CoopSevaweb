using System.ComponentModel.DataAnnotations;

namespace AVSBackend.DTOs
{
    public class AadharSendOtpRequest
    {
        [Required]
        [StringLength(12, MinimumLength = 12, ErrorMessage = "Aadhaar number must be 12 digits.")]
        public string AadharNo { get; set; } = string.Empty;
    }

    public class AadharVerifyRequest
    {
        [Required]
        public string AadharNo { get; set; } = string.Empty;

        [Required]
        public string Otp { get; set; } = string.Empty;
    }
}
