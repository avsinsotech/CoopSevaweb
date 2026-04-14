using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace AVSBackend.Services
{
    public class GrowwSaaS_SmsService : ISmsService
    {
        private readonly HttpClient _httpClient;

        public GrowwSaaS_SmsService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> SendOtpAsync(string phoneNumber, string otpCode)
        {
            // Verified Template Format: Online A/c Opening OTP:{otpCode} Thank you AVS
            string message = $"Online A/c Opening OTP:{otpCode} Thank you AVS";
            
            // Verified Parameters for GrowwSaaS: username, password, from, to, text, templateid, unicode
            string uid = "GYEavsin";
            string pwd = "sms2020";
            string sender = "AVSIPL";
            string templateId = "1001837171501697258";

            string url = $"https://api2.growwsaas.com/fe/api/v1/send?username={uid}&password={pwd}&unicode=false&from={sender}&to={phoneNumber}&text={Uri.EscapeDataString(message)}&templateid={templateId}";

            try
            {
                Console.WriteLine($"[SMS DEBUG] Sending to GrowwSaaS: {url}");
                var response = await _httpClient.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[SMS DEBUG] Response: {response.StatusCode} - {responseBody}");

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SMS DEBUG] Error sending SMS: {ex.Message}");
                return false;
            }
        }
    }
}
