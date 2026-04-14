using System.ComponentModel.DataAnnotations;

namespace AVSBackend.DTOs
{
    public class PanVerifyRequest
    {
        [Required]
        [RegularExpression(@"[A-Z]{5}[0-9]{4}[A-Z]{1}", ErrorMessage = "Invalid PAN number format.")]
        public string PanNumber { get; set; } = string.Empty;
    }
}
