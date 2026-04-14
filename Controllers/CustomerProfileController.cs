using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AVSBackend.Data;
using AVSBackend.Models;
using AVSBackend.DTOs;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace AVSBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerProfileController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomerProfileController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("submit-profile")]
        public async Task<IActionResult> SubmitProfile([FromBody] AccountOpeningRequest request)
        {
            if (request == null)
                return BadRequest("Request cannot be null.");

            try
            {
                var customer = new CustomerFullDetails
                {
                    // Aadhaar & PAN
                    AadharNumber = request.AadharNumber,
                    PanNumber = request.PanNumber,

                    // Personal Details
                    FullName = request.FullName,
                    NameInMarathi = request.NameInMarathi,
                    FatherOrHusbandName = request.FatherOrHusbandName,
                    MotherName = request.MotherName,
                    DOB = request.DOB,
                    Gender = request.Gender,
                    MaritalStatus = request.MaritalStatus,
                    Nationality = request.Nationality,
                    ResidentialStatus = request.ResidentialStatus,
                    Religion = request.Religion,
                    Category = request.Category,

                    // Current Address
                    CurrentAddress = request.CurrentAddress,
                    CurrentCity = request.CurrentCity,
                    CurrentTaluka = request.CurrentTaluka,
                    CurrentDistrict = request.CurrentDistrict,
                    CurrentState = request.CurrentState,
                    CurrentPinCode = request.CurrentPinCode,
                    CurrentCountry = request.CurrentCountry ?? "India",

                    // Permanent Address
                    IsPermanentSame = request.IsPermanentSame,
                    PermanentAddress = request.IsPermanentSame ? request.CurrentAddress : request.PermanentAddress,
                    PermanentCity = request.IsPermanentSame ? request.CurrentCity : request.PermanentCity,
                    PermanentTaluka = request.IsPermanentSame ? request.CurrentTaluka : request.PermanentTaluka,
                    PermanentDistrict = request.IsPermanentSame ? request.CurrentDistrict : request.PermanentDistrict,
                    PermanentState = request.IsPermanentSame ? request.CurrentState : request.PermanentState,
                    PermanentPinCode = request.IsPermanentSame ? request.CurrentPinCode : request.PermanentPinCode,
                    PermanentCountry = request.IsPermanentSame ? request.CurrentCountry : (request.PermanentCountry ?? "India"),

                    // KYC Details
                    OVDType_1 = request.OVDType_1,
                    OVDNumber_1 = request.OVDNumber_1,
                    OVDExpiryDate_1 = request.OVDExpiryDate_1,

                    OVDType_2 = request.OVDType_2,
                    OVDNumber_2 = request.OVDNumber_2,
                    OVDExpiryDate_2 = request.OVDExpiryDate_2,

                    OVDType_3 = request.OVDType_3,
                    OVDNumber_3 = request.OVDNumber_3,
                    OVDExpiryDate_3 = request.OVDExpiryDate_3,

                    AddressProofType = request.AddressProofType,
                    AddressProofNumber = request.AddressProofNumber,
                    FormType = request.FormType,

                    // Contact
                    MobileNumber = request.MobileNumber,
                    AlternateMobile = request.AlternateMobile,
                    Email = request.Email,

                    // Occupation & Income
                    Occupation = request.Occupation,
                    EmployerName = request.EmployerName,
                    Designation = request.Designation,
                    AnnualIncome = request.AnnualIncome,
                    SourceOfFunds = request.SourceOfFunds,
                    IsPEP = request.IsPEP,
                    IsRelatedToPEP = request.IsRelatedToPEP,

                    // Nominee Details are mapped below under specific rows

                    // New Fields Mapping
                    BranchCode = request.BranchCode,
                    KYCNumber = request.KYCNumber,
                    DateOfApplication = request.DateOfApplication,
                    PlaceOfApplication = request.PlaceOfApplication,
                    NamePrefix = request.NamePrefix,
                    NameLast = request.NameLast,
                    FatherSpouseNamePrefix = request.FatherSpouseNamePrefix,
                    MobileISDCode = request.MobileISDCode,
                    CIFID = request.CIFID,
                    BranchName = request.BranchName,

                    // Nominee 1 (Database Base Fields) -> Row 1
                    NomineeName = request.NomineeName,
                    NomineeRelationship = request.NomineeRelationship,
                    NomineeDOB = request.NomineeDOB,
                    NomineeAge = request.NomineeAge,
                    NomineeSharePercent = request.NomineeSharePercent,
                    NomineeAddress = request.NomineeAddress,
                    GuardianName = request.GuardianName,

                    // Nominee 2 (Database '1' Fields) -> Row 2
                    NomineeName1 = request.NomineeName1,
                    NomineeRelationship1 = request.NomineeRelationship1,
                    NomineeDOB1 = request.NomineeDOB1,
                    NomineeAge1 = request.NomineeAge1,
                    NomineeSharePercent1 = request.NomineeSharePercent1,
                    NomineeAddress1 = request.NomineeAddress1,
                    GuardianName1 = request.GuardianName1,

                    // Nominee 3 (Database '2' Fields) -> Row 3
                    NomineeName2 = request.NomineeName2,
                    NomineeRelationship2 = request.NomineeRelationship2,
                    NomineeDOB2 = request.NomineeDOB2,
                    NomineeAge2 = request.NomineeAge2,
                    NomineeSharePercent2 = request.NomineeSharePercent2,
                    NomineeAddress2 = request.NomineeAddress2,
                    GuardianName2 = request.GuardianName2,

                    // OVD Capture Data
                    OVD1_CaptureDate = request.OVD1_CaptureDate,
                    OVD1_Latitude = request.OVD1_Latitude,
                    OVD1_Longitude = request.OVD1_Longitude,
                    OVD1_Location = request.OVD1_Location,

                    OVD2_CaptureDate = request.OVD2_CaptureDate,
                    OVD2_Latitude = request.OVD2_Latitude,
                    OVD2_Longitude = request.OVD2_Longitude,
                    OVD2_Location = request.OVD2_Location,

                    SubmittedByUserId = request.SubmittedByUserId,

                    OVD3_CaptureDate = request.OVD3_CaptureDate,
                    OVD3_Latitude = request.OVD3_Latitude,
                    OVD3_Longitude = request.OVD3_Longitude,
                    OVD3_Location = request.OVD3_Location,

                    OVD4_CaptureDate = request.OVD4_CaptureDate,
                    OVD4_Latitude = request.OVD4_Latitude,
                    OVD4_Longitude = request.OVD4_Longitude,
                    OVD4_Location = request.OVD4_Location,

                    IsUpdated = false,
                    CreatedDate = DateTime.Now
                };

                try
                {
                    customer.Photo = ConvertBase64(request.PhotoBase64, "Photo");
                    customer.Signature = ConvertBase64(request.SignatureBase64, "Signature");
                    customer.OVDImg1 = ConvertBase64(request.OVDImg1Base64, "OVDImg1");
                    customer.OVDImg2 = ConvertBase64(request.OVDImg2Base64, "OVDImg2");
                    customer.OVDImg3 = ConvertBase64(request.OVDImg3Base64, "OVDImg3");
                    customer.OVDImg4 = ConvertBase64(request.OVDImg4Base64, "OVDImg4");
                    customer.Form60_61_Img = ConvertBase64(request.Form60_61_ImgBase64, "Form60_61_Img");
                }
                catch (ArgumentException argEx)
                {
                    return BadRequest(argEx.Message);
                }

                _context.CustomerFullDetails.Add(customer);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Customer details submitted successfully.", ReferenceID = customer.ReferenceID });
            }
            catch (Exception ex)
            {
                // Detailed error logging would happen here
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("check-existence")]
        public async Task<IActionResult> CheckExistence([FromQuery] int? referenceId, [FromQuery] string? aadharNumber, [FromQuery] string? panNumber, [FromQuery] string? cifId)
        {
            var query = _context.CustomerFullDetails.AsNoTracking();

            if (referenceId.HasValue)
            {
                query = query.Where(c => c.ReferenceID == referenceId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(aadharNumber))
            {
                query = query.Where(c => c.AadharNumber == aadharNumber);
            }
            else if (!string.IsNullOrWhiteSpace(panNumber))
            {
                query = query.Where(c => c.PanNumber == panNumber);
            }
            else if (!string.IsNullOrWhiteSpace(cifId))
            {
                query = query.Where(c => c.CIFID == cifId);
            }
            else
            {
                return BadRequest("Please provide at least one search parameter (referenceId, aadharNumber, panNumber, or cifId).");
            }

            var customer = await query.FirstOrDefaultAsync();

            if (customer != null)
            {
                return Ok(MapToResponse(customer));
            }

            return Ok(new { exists = false });
        }

        [HttpGet("check-aadhar/{aadharNumber}")]
        public async Task<IActionResult> CheckAadharExistence(string aadharNumber)
        {
            if (string.IsNullOrWhiteSpace(aadharNumber))
                return BadRequest(new { success = false, message = "Aadhaar number is required." });

            var customer = await _context.CustomerFullDetails
                .AsNoTracking()
                .Where(c => c.AadharNumber == aadharNumber)
                .Select(c => new { c.ReferenceID, c.FullName })
                .FirstOrDefaultAsync();

            if (customer != null)
            {
                return Ok(new { exists = true, referenceId = customer.ReferenceID, fullName = customer.FullName });
            }

            return Ok(new { exists = false });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(int id)
        {
            var customer = await _context.CustomerFullDetails.FindAsync(id);

            if (customer == null)
                return NotFound();

            return Ok(MapToResponse(customer));
        }

        [HttpPut("update-profile/{referenceId}")]
        public async Task<IActionResult> UpdateProfile(int referenceId, [FromBody] AccountOpeningRequest request)
        {
            if (request == null)
                return BadRequest("Request cannot be null.");

            try
            {
                var customer = await _context.CustomerFullDetails.FindAsync(referenceId);
                if (customer == null)
                    return NotFound($"Customer with ReferenceID {referenceId} not found.");

                // Update Fields (Aadhar & PAN)
                customer.AadharNumber = request.AadharNumber;
                customer.PanNumber = request.PanNumber;

                // Personal Details
                customer.FullName = request.FullName;
                customer.NameInMarathi = request.NameInMarathi;
                customer.FatherOrHusbandName = request.FatherOrHusbandName;
                customer.MotherName = request.MotherName;
                customer.DOB = request.DOB;
                customer.Gender = request.Gender;
                customer.MaritalStatus = request.MaritalStatus;
                customer.Nationality = request.Nationality;
                customer.ResidentialStatus = request.ResidentialStatus;
                customer.Religion = request.Religion;
                customer.Category = request.Category;

                // Current Address
                customer.CurrentAddress = request.CurrentAddress;
                customer.CurrentCity = request.CurrentCity;
                customer.CurrentTaluka = request.CurrentTaluka;
                customer.CurrentDistrict = request.CurrentDistrict;
                customer.CurrentState = request.CurrentState;
                customer.CurrentPinCode = request.CurrentPinCode;
                customer.CurrentCountry = request.CurrentCountry ?? "India";

                // Permanent Address
                customer.IsPermanentSame = request.IsPermanentSame;
                customer.PermanentAddress = request.IsPermanentSame ? request.CurrentAddress : request.PermanentAddress;
                customer.PermanentCity = request.IsPermanentSame ? request.CurrentCity : request.PermanentCity;
                customer.PermanentTaluka = request.IsPermanentSame ? request.CurrentTaluka : request.PermanentTaluka;
                customer.PermanentDistrict = request.IsPermanentSame ? request.CurrentDistrict : request.PermanentDistrict;
                customer.PermanentState = request.IsPermanentSame ? request.CurrentState : request.PermanentState;
                customer.PermanentPinCode = request.IsPermanentSame ? request.CurrentPinCode : request.PermanentPinCode;
                customer.PermanentCountry = request.IsPermanentSame ? request.CurrentCountry : (request.PermanentCountry ?? "India");

                // KYC Details
                customer.OVDType_1 = request.OVDType_1;
                customer.OVDNumber_1 = request.OVDNumber_1;
                customer.OVDExpiryDate_1 = request.OVDExpiryDate_1;
                customer.OVDType_2 = request.OVDType_2;
                customer.OVDNumber_2 = request.OVDNumber_2;
                customer.OVDExpiryDate_2 = request.OVDExpiryDate_2;
                customer.OVDType_3 = request.OVDType_3;
                customer.OVDNumber_3 = request.OVDNumber_3;
                customer.OVDExpiryDate_3 = request.OVDExpiryDate_3;
                customer.AddressProofType = request.AddressProofType;
                customer.AddressProofNumber = request.AddressProofNumber;
                customer.FormType = request.FormType;

                // Contact
                customer.MobileNumber = request.MobileNumber;
                customer.AlternateMobile = request.AlternateMobile;
                customer.Email = request.Email;

                // Occupation & Income
                customer.Occupation = request.Occupation;
                customer.EmployerName = request.EmployerName;
                customer.Designation = request.Designation;
                customer.AnnualIncome = request.AnnualIncome;
                customer.SourceOfFunds = request.SourceOfFunds;
                customer.IsPEP = request.IsPEP;
                customer.IsRelatedToPEP = request.IsRelatedToPEP;

                // Additional Fields Mapping (Restored)
                customer.BranchCode = request.BranchCode;
                customer.BranchName = request.BranchName;
                customer.KYCNumber = request.KYCNumber;
                customer.DateOfApplication = request.DateOfApplication;
                customer.PlaceOfApplication = request.PlaceOfApplication;
                customer.NamePrefix = request.NamePrefix;
                customer.NameLast = request.NameLast;
                customer.FatherSpouseNamePrefix = request.FatherSpouseNamePrefix;
                customer.MobileISDCode = request.MobileISDCode;

                // Nominee 1 (Database Base Fields) -> Row 1
                if (request.NomineeName != null) customer.NomineeName = request.NomineeName;
                if (request.NomineeRelationship != null) customer.NomineeRelationship = request.NomineeRelationship;
                if (request.NomineeDOB != null) customer.NomineeDOB = request.NomineeDOB;
                if (request.NomineeAge != null) customer.NomineeAge = request.NomineeAge;
                if (request.NomineeSharePercent != null) customer.NomineeSharePercent = request.NomineeSharePercent;
                if (request.NomineeAddress != null) customer.NomineeAddress = request.NomineeAddress;
                if (request.GuardianName != null) customer.GuardianName = request.GuardianName;

                // Nominee 2 (Database '1' Fields) -> Row 2
                if (request.NomineeName1 != null) customer.NomineeName1 = request.NomineeName1;
                if (request.NomineeRelationship1 != null) customer.NomineeRelationship1 = request.NomineeRelationship1;
                if (request.NomineeDOB1 != null) customer.NomineeDOB1 = request.NomineeDOB1;
                if (request.NomineeAge1 != null) customer.NomineeAge1 = request.NomineeAge1;
                if (request.NomineeSharePercent1 != null) customer.NomineeSharePercent1 = request.NomineeSharePercent1;
                if (request.NomineeAddress1 != null) customer.NomineeAddress1 = request.NomineeAddress1;
                if (request.GuardianName1 != null) customer.GuardianName1 = request.GuardianName1;

                // Nominee 3 (Database '2' Fields) -> Row 3
                if (request.NomineeName2 != null) customer.NomineeName2 = request.NomineeName2;
                if (request.NomineeRelationship2 != null) customer.NomineeRelationship2 = request.NomineeRelationship2;
                if (request.NomineeDOB2 != null) customer.NomineeDOB2 = request.NomineeDOB2;
                if (request.NomineeAge2 != null) customer.NomineeAge2 = request.NomineeAge2;
                if (request.NomineeSharePercent2 != null) customer.NomineeSharePercent2 = request.NomineeSharePercent2;
                if (request.NomineeAddress2 != null) customer.NomineeAddress2 = request.NomineeAddress2;
                if (request.GuardianName2 != null) customer.GuardianName2 = request.GuardianName2;

                // OVD Capture Data
                customer.OVD1_CaptureDate = request.OVD1_CaptureDate;
                customer.OVD1_Latitude = request.OVD1_Latitude;
                customer.OVD1_Longitude = request.OVD1_Longitude;
                customer.OVD1_Location = request.OVD1_Location;
                customer.OVD2_CaptureDate = request.OVD2_CaptureDate;
                customer.OVD2_Latitude = request.OVD2_Latitude;
                customer.OVD2_Longitude = request.OVD2_Longitude;
                customer.OVD2_Location = request.OVD2_Location;
                customer.OVD3_CaptureDate = request.OVD3_CaptureDate;
                customer.OVD3_Latitude = request.OVD3_Latitude;
                customer.OVD3_Longitude = request.OVD3_Longitude;
                customer.OVD3_Location = request.OVD3_Location;
                customer.OVD4_CaptureDate = request.OVD4_CaptureDate;
                customer.OVD4_Latitude = request.OVD4_Latitude;
                customer.OVD4_Longitude = request.OVD4_Longitude;
                customer.OVD4_Location = request.OVD4_Location;

                // Update Images only if new Base64 provided
                if (!string.IsNullOrWhiteSpace(request.PhotoBase64)) customer.Photo = ConvertBase64(request.PhotoBase64, "Photo");
                if (!string.IsNullOrWhiteSpace(request.SignatureBase64)) customer.Signature = ConvertBase64(request.SignatureBase64, "Signature");
                if (!string.IsNullOrWhiteSpace(request.OVDImg1Base64)) customer.OVDImg1 = ConvertBase64(request.OVDImg1Base64, "OVDImg1");
                if (!string.IsNullOrWhiteSpace(request.OVDImg2Base64)) customer.OVDImg2 = ConvertBase64(request.OVDImg2Base64, "OVDImg2");
                if (!string.IsNullOrWhiteSpace(request.OVDImg3Base64)) customer.OVDImg3 = ConvertBase64(request.OVDImg3Base64, "OVDImg3");
                if (!string.IsNullOrWhiteSpace(request.OVDImg4Base64)) customer.OVDImg4 = ConvertBase64(request.OVDImg4Base64, "OVDImg4");
                if (!string.IsNullOrWhiteSpace(request.Form60_61_ImgBase64)) customer.Form60_61_Img = ConvertBase64(request.Form60_61_ImgBase64, "Form60_61_Img");
                customer.IsUpdated = true;
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Customer profile updated successfully.", ReferenceID = customer.ReferenceID });
            }
            catch (ArgumentException argEx)
            {
                return BadRequest(argEx.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        private byte[]? ConvertBase64(string? base64, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(base64)) return null;
            try
            {
                string pureBase64 = base64.Contains(",") ? base64.Split(',')[1] : base64;
                return Convert.FromBase64String(pureBase64.Trim());
            }
            catch
            {
                throw new ArgumentException($"Invalid Base64 string provided for {fieldName}.");
            }
        }

        private CustomerProfileResponse MapToResponse(CustomerFullDetails customer)
        {
            string? ToBase64(byte[]? data) => data != null ? "data:image/png;base64," + Convert.ToBase64String(data) : null;

            return new CustomerProfileResponse
            {
                ReferenceID = customer.ReferenceID,
                IsUpdated = customer.IsUpdated,
                CreatedDate = customer.CreatedDate,
                
                AadharNumber = customer.AadharNumber,
                PanNumber = customer.PanNumber,
                FullName = customer.FullName,
                NameInMarathi = customer.NameInMarathi,
                FatherOrHusbandName = customer.FatherOrHusbandName,
                MotherName = customer.MotherName,
                DOB = customer.DOB,
                Gender = customer.Gender,
                MaritalStatus = customer.MaritalStatus,
                Nationality = customer.Nationality,
                ResidentialStatus = customer.ResidentialStatus,
                Religion = customer.Religion,
                Category = customer.Category,

                PhotoBase64 = ToBase64(customer.Photo),
                SignatureBase64 = ToBase64(customer.Signature),
                OVDImg1Base64 = ToBase64(customer.OVDImg1),
                OVDImg2Base64 = ToBase64(customer.OVDImg2),
                OVDImg3Base64 = ToBase64(customer.OVDImg3),
                OVDImg4Base64 = ToBase64(customer.OVDImg4),
                Form60_61_ImgBase64 = ToBase64(customer.Form60_61_Img),

                CurrentAddress = customer.CurrentAddress,
                CurrentCity = customer.CurrentCity,
                CurrentTaluka = customer.CurrentTaluka,
                CurrentDistrict = customer.CurrentDistrict,
                CurrentState = customer.CurrentState,
                CurrentPinCode = customer.CurrentPinCode,
                CurrentCountry = customer.CurrentCountry,

                IsPermanentSame = customer.IsPermanentSame,
                PermanentAddress = customer.PermanentAddress,
                PermanentCity = customer.PermanentCity,
                PermanentTaluka = customer.PermanentTaluka,
                PermanentDistrict = customer.PermanentDistrict,
                PermanentState = customer.PermanentState,
                PermanentPinCode = customer.PermanentPinCode,
                PermanentCountry = customer.PermanentCountry,

                OVDType_1 = customer.OVDType_1,
                OVDNumber_1 = customer.OVDNumber_1,
                OVDExpiryDate_1 = customer.OVDExpiryDate_1,

                OVDType_2 = customer.OVDType_2,
                OVDNumber_2 = customer.OVDNumber_2,
                OVDExpiryDate_2 = customer.OVDExpiryDate_2,

                OVDType_3 = customer.OVDType_3,
                OVDNumber_3 = customer.OVDNumber_3,
                OVDExpiryDate_3 = customer.OVDExpiryDate_3,

                AddressProofType = customer.AddressProofType,
                AddressProofNumber = customer.AddressProofNumber,
                FormType = customer.FormType,

                MobileNumber = customer.MobileNumber,
                AlternateMobile = customer.AlternateMobile,
                Email = customer.Email,

                Occupation = customer.Occupation,
                EmployerName = customer.EmployerName,
                Designation = customer.Designation,
                AnnualIncome = customer.AnnualIncome,
                SourceOfFunds = customer.SourceOfFunds,
                IsPEP = customer.IsPEP,
                IsRelatedToPEP = customer.IsRelatedToPEP,

                // New Fields Mapping
                BranchCode = customer.BranchCode,
                KYCNumber = customer.KYCNumber,
                DateOfApplication = customer.DateOfApplication,
                PlaceOfApplication = customer.PlaceOfApplication,
                NamePrefix = customer.NamePrefix,
                NameLast = customer.NameLast,
                FatherSpouseNamePrefix = customer.FatherSpouseNamePrefix,
                MobileISDCode = customer.MobileISDCode,
                CIFID = customer.CIFID,
                BranchName = customer.BranchName,

                // Nominee 1 (Database Base Fields) -> Row 1
                NomineeName = customer.NomineeName,
                NomineeRelationship = customer.NomineeRelationship,
                NomineeDOB = customer.NomineeDOB,
                NomineeAge = customer.NomineeAge,
                NomineeSharePercent = (decimal?)customer.NomineeSharePercent,
                NomineeAddress = customer.NomineeAddress,
                GuardianName = customer.GuardianName,

                // Nominee 2 (Database '1' Fields) -> Row 2
                NomineeName1 = customer.NomineeName1,
                NomineeRelationship1 = customer.NomineeRelationship1,
                NomineeDOB1 = customer.NomineeDOB1,
                NomineeAge1 = customer.NomineeAge1,
                NomineeSharePercent1 = customer.NomineeSharePercent1,
                NomineeAddress1 = customer.NomineeAddress1,
                GuardianName1 = customer.GuardianName1,

                // Nominee 3 (Database '2' Fields) -> Row 3
                NomineeName2 = customer.NomineeName2,
                NomineeRelationship2 = customer.NomineeRelationship2,
                NomineeDOB2 = customer.NomineeDOB2,
                NomineeAge2 = customer.NomineeAge2,
                NomineeSharePercent2 = customer.NomineeSharePercent2,
                NomineeAddress2 = customer.NomineeAddress2,
                GuardianName2 = customer.GuardianName2,

                // OVD Capture Data
                OVD1_CaptureDate = customer.OVD1_CaptureDate,
                OVD1_Latitude = customer.OVD1_Latitude,
                OVD1_Longitude = customer.OVD1_Longitude,
                OVD1_Location = customer.OVD1_Location,

                OVD2_CaptureDate = customer.OVD2_CaptureDate,
                OVD2_Latitude = customer.OVD2_Latitude,
                OVD2_Longitude = customer.OVD2_Longitude,
                OVD2_Location = customer.OVD2_Location,

                OVD3_CaptureDate = customer.OVD3_CaptureDate,
                OVD3_Latitude = customer.OVD3_Latitude,
                OVD3_Longitude = customer.OVD3_Longitude,
                OVD3_Location = customer.OVD3_Location,

                OVD4_CaptureDate = customer.OVD4_CaptureDate,
                OVD4_Latitude = customer.OVD4_Latitude,
                OVD4_Longitude = customer.OVD4_Longitude,
                OVD4_Location = customer.OVD4_Location,
                SubmittedByUserId = customer.SubmittedByUserId
            };
        }
    }
}
