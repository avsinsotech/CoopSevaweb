using System;

namespace AVSBackend.Models
{
    public class OtpRecord
    {
        public int Id { get; set; }
        public string MobileNumber { get; set; } = string.Empty;
        public string OTP { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; } = DateTime.Now;
        public DateTime? ExpiryTime { get; set; }
        public bool IsVerified { get; set; }
        public int AttemptCount { get; set; }
    }
}
