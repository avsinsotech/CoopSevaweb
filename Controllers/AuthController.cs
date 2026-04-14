using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AVSBackend.Data;
using AVSBackend.DTOs;
using AVSBackend.Models;
using AVSBackend.Services;

namespace AVSBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISmsService _smsService;
        private readonly IAadharService _aadharService;
        private readonly IPanService _panService;

        public AuthController(AppDbContext context, ISmsService smsService, IAadharService aadharService, IPanService panService)
        {
            _context = context;
            _smsService = smsService;
            _aadharService = aadharService;
            _panService = panService;
        }

        [HttpPost("verify-pan")]
        public async Task<IActionResult> VerifyPan([FromBody] PanVerifyRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, message, fullResponse) = await _panService.VerifyPanAsync(request);
            if (!success)
            {
                return BadRequest(new { Message = "PAN verification failed.", Details = message, ProviderResponse = fullResponse });
            }

            // Persistence logic
            try
            {
                using var doc = JsonDocument.Parse(fullResponse);
                if (doc.RootElement.TryGetProperty("data", out var data))
                {
                    var panData = new PanData
                    {
                        PanNumber = request.PanNumber,
                        ClientId = "101",
                        VerifiedBy = "AVS_API",
                        VerifiedDate = DateTime.Today,
                        
                        Name = GetStringProperty(data, "full_name") ?? GetStringProperty(data, "name"),
                        FatherName = GetStringProperty(data, "father_name"),
                        DOB = DateTime.TryParse(GetStringProperty(data, "dob"), out var dob) ? dob : (DateTime?)null
                    };

                    var existing = await _context.PanDatas.FindAsync(request.PanNumber);
                    if (existing != null)
                    {
                        _context.Entry(existing).CurrentValues.SetValues(panData);
                    }
                    else
                    {
                        await _context.PanDatas.AddAsync(panData);
                    }
                    
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"[PAN SUCCESS] Saved record for {request.PanNumber}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PAN PERSIST ERROR]: {ex.Message}");
            }

            return Ok(new { Message = "PAN verified successfully.", Details = message, ProviderResponse = fullResponse });
        }

        [HttpPost("send-aadhar-otp")]
        public async Task<IActionResult> SendAadharOtp([FromBody] AadharSendOtpRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, result) = await _aadharService.SendAadharOtpAsync(request.AadharNo);
            if (!success)
            {
                return StatusCode(500, new { Message = "Failed to send Aadhaar OTP.", Details = result });
            }

            return Ok(new { Message = "Aadhaar OTP sent successfully.", ClientId = result });
        }

        [HttpPost("verify-aadhar-otp")]
        public async Task<IActionResult> VerifyAadharOtp([FromBody] AadharVerifyRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (success, result) = await _aadharService.VerifyAadharOtpAsync(request);
            if (!success)
            {
                return BadRequest(new { Message = "Aadhaar OTP verification failed.", Details = result });
            }

            // Persistence logic
            try
            {
                using var doc = JsonDocument.Parse(result);
                if (doc.RootElement.TryGetProperty("data", out var data))
                {
                    Console.WriteLine($"[AADHAR DEBUG] Success Data: {data.GetRawText()}");

                    var aadharData = new AadharData
                    {
                        AadharNumber = request.AadharNo,
                        IsVerified = true,
                        VerifiedDate = DateTime.Now,
                        VerifiedBy = "AVS_API",
                        VerifiedByClientID = _aadharService.LastClientId,
                        ClientId = "102", 
                        
                        Name = GetStringProperty(data, "full_name") ?? GetStringProperty(data, "name"),
                        Gender = GetStringProperty(data, "gender"),
                        AgeRange = GetStringProperty(data, "age_range"),
                        Address = GetStringProperty(data, "address"),
                        District = GetStringProperty(data, "district"),
                        State = GetStringProperty(data, "state"),
                        Zip = GetStringProperty(data, "zip"),
                        Country = GetStringProperty(data, "country") ?? "India",
                        MobileLastDigit = GetStringProperty(data, "mobile_hash"), 
                        IsMobile = data.TryGetProperty("has_mobile", out var pm) ? pm.GetBoolean() : (bool?)null
                    };

                    var existing = await _context.AadharDatas.FindAsync(request.AadharNo);
                    if (existing != null)
                    {
                        _context.Entry(existing).CurrentValues.SetValues(aadharData);
                    }
                    else
                    {
                        await _context.AadharDatas.AddAsync(aadharData);
                    }
                    
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"[AADHAR SUCCESS] Saved record for {request.AadharNo}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AADHAR PERSIST ERROR]: {ex.Message}");
                Console.WriteLine($"[AADHAR PERSIST TRACE]: {ex.StackTrace}");
            }

            return Ok(new { Message = "Aadhaar verified successfully.", ProviderResponse = result });
        }

        private string? GetStringProperty(JsonElement element, string propName)
        {
            if (element.TryGetProperty(propName, out var prop))
            {
                if (prop.ValueKind == JsonValueKind.String)
                    return prop.GetString();
                else if (prop.ValueKind == JsonValueKind.Object || prop.ValueKind == JsonValueKind.Array)
                    return prop.GetRawText();
                else if (prop.ValueKind == JsonValueKind.Null)
                    return null;
                else
                    return prop.ToString();
            }
            return null;
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            string otp = new Random().Next(100000, 999999).ToString();

            var otpRecord = new OtpRecord
            {
                MobileNumber = request.PhoneNumber,
                OTP = otp,
                ExpiryTime = DateTime.UtcNow.AddMinutes(5),
                IsVerified = false,
                CreatedTime = DateTime.UtcNow,
                AttemptCount = 0
            };

            _context.OtpRecords.Add(otpRecord);
            await _context.SaveChangesAsync();

            bool smsSent = await _smsService.SendOtpAsync(request.PhoneNumber, otp);
            if (!smsSent)
                return StatusCode(500, "Failed to send SMS.");

            return Ok(new { Message = "OTP sent successfully." });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var record = await _context.OtpRecords
                .Where(r => r.MobileNumber == request.PhoneNumber && r.OTP == request.OtpCode && !r.IsVerified)
                .OrderByDescending(r => r.CreatedTime)
                .FirstOrDefaultAsync();

            if (record == null || record.ExpiryTime < DateTime.UtcNow)
                return BadRequest("Invalid or expired OTP.");

            record.IsVerified = true;
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Login successful." });
        }

        [HttpPost("app-login")]
        public async Task<IActionResult> AppUserLogin([FromBody] AppUserLoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Using case-insensitive check by default in EF Core mapping (unless collation is set to case-sensitive)
            var user = await _context.AppUsers
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Password == request.Password && u.IsActive);

            if (user == null)
            {
                return Unauthorized(new { Success = false, Message = "Invalid username or password, or user is inactive." });
            }

            return Ok(new 
            { 
                Success = true, 
                Message = "Login successfully",
                Data = new 
                {
                    UserID = user.UserID,
                    Username = user.Username,
                    FullName = user.FullName,
                    BranchName = user.BranchName
                }
            });
        }
    }
}
