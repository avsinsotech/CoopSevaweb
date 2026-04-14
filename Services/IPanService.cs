using System.Threading.Tasks;
using AVSBackend.DTOs;

namespace AVSBackend.Services
{
    public interface IPanService
    {
        Task<(bool Success, string Message, string FullResponse)> VerifyPanAsync(PanVerifyRequest request);
    }
}
