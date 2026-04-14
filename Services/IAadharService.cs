using System.Threading.Tasks;
using AVSBackend.DTOs;

namespace AVSBackend.Services
{
    public interface IAadharService
    {
        string? LastClientId { get; }
        Task<(bool Success, string Message)> SendAadharOtpAsync(string aadharNo);
        Task<(bool Success, string Message)> VerifyAadharOtpAsync(AadharVerifyRequest request);
    }
}
