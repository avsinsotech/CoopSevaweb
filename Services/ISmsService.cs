using System.Threading.Tasks;

namespace AVSBackend.Services
{
    public interface ISmsService
    {
        Task<bool> SendOtpAsync(string phoneNumber, string otpCode);
    }
}
