using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AVSBackend.DTOs;

namespace AVSBackend.Services
{
    public class AadharService : IAadharService
    {
        private readonly HttpClient _httpClient;
        private static string? _lastClientId; 

        public string? LastClientId => _lastClientId;

        public AadharService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(bool Success, string Message)> SendAadharOtpAsync(string aadharNo)
        {
            string url = $"http://110.227.207.211:90/OVD_L/Frm.aspx?adharNo={aadharNo}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"[AADHAR DEBUG] Send OTP Response: {response.StatusCode} - {responseBody}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(responseBody);
                        bool status = doc.RootElement.GetProperty("status").GetBoolean();
                        // string? clientId = null; // No longer needed as we store directly in static field
                        
                        if (doc.RootElement.TryGetProperty("data", out var dataElement) && 
                            dataElement.TryGetProperty("client_id", out var clientIdElement))
                        {
                            _lastClientId = clientIdElement.GetString();
                        }

                        // Return the client_id as part of the message or handle specifically
                        return (status, _lastClientId ?? responseBody);
                    }
                    catch
                    {
                        // If not JSON or missing status, fall back to success if status code is OK
                        return (true, responseBody);
                    }
                }
                else
                {
                    return (false, $"Aadhar API returned {response.StatusCode}: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Exception in Aadhar Send OTP: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> VerifyAadharOtpAsync(AadharVerifyRequest request)
        {
            // Use internal defaults and captured clientId
            string clientId = _lastClientId ?? "";
            string clientiid = "102";
            string bankName = "pravara bank";

            string url = $"http://110.227.207.211:90/OVD_L/FrmVerifyOTP.aspx?Clientiid={clientiid}&ClientID={clientId}&OTP={request.Otp}&BankName={bankName}&adharNo={request.AadharNo}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();
                
                Console.WriteLine($"[AADHAR DEBUG] Verify OTP Response URL: {url}");
                Console.WriteLine($"[AADHAR DEBUG] Verify OTP Response: {response.StatusCode} - {responseBody}");

                if (response.IsSuccessStatusCode)
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(responseBody);
                        bool status = doc.RootElement.GetProperty("status").GetBoolean();
                        return (status, responseBody);
                    }
                    catch
                    {
                        // Fallback if parsing fails
                        return (true, responseBody);
                    }
                }
                else
                {
                    return (false, $"Aadhar Verify API returned {response.StatusCode}: {responseBody}");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Exception in Aadhar Verify OTP: {ex.Message}");
            }
        }
    }
}
