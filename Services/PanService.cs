using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AVSBackend.DTOs;

namespace AVSBackend.Services
{
    public class PanService : IPanService
    {
        private readonly HttpClient _httpClient;

        public PanService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(bool Success, string Message, string FullResponse)> VerifyPanAsync(PanVerifyRequest request)
        {
            string clientId = "101";
            string bankName = "pravara bank";

            string url = $"http://110.227.207.211:90/ovd/FrmPAN.aspx?PAN={request.PanNumber}&BankName={bankName}&ClientId={clientId}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"[PAN DEBUG] Response: {response.StatusCode} - {responseBody}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(responseBody);
                        bool status = doc.RootElement.GetProperty("status").GetBoolean();
                        string msg = doc.RootElement.GetProperty("message").GetString() ?? "PAN Verification results";
                        return (status, msg, responseBody);
                    }
                    catch
                    {
                        // Fallback if not JSON or parsing fails
                        return (true, "PAN verified (status code 200)", responseBody);
                    }
                }
                else
                {
                    return (false, $"PAN API returned {response.StatusCode}", responseBody);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Exception in PAN verification: {ex.Message}", string.Empty);
            }
        }
    }
}
