using System.ComponentModel.DataAnnotations;

namespace AVSBackend.DTOs
{
    public class AppUserLoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AppUserPasswordResetRequest
    {
        [Required]
        public string NewPassword { get; set; } = string.Empty;
    }
}
