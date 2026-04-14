using System;

namespace AVSBackend.DTOs
{
    public class AccountOpeningRequest
    {
        // Aadhaar & PAN
        public string? AadharNumber { get; set; }
        public string? PanNumber { get; set; }

        // Personal Details
        public string FullName { get; set; } = string.Empty;
        public string? NameInMarathi { get; set; }
        public string FatherOrHusbandName { get; set; } = string.Empty;
        public string? MotherName { get; set; }
        public DateTime DOB { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string MaritalStatus { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public string? ResidentialStatus { get; set; }
        public string? Religion { get; set; }
        public string? Category { get; set; }

        public string? PhotoBase64 { get; set; }
        public string? SignatureBase64 { get; set; }
        public string? OVDImg1Base64 { get; set; }
        public string? OVDImg2Base64 { get; set; }
        public string? OVDImg3Base64 { get; set; }
        public string? OVDImg4Base64 { get; set; }
        public string? Form60_61_ImgBase64 { get; set; }

        // Current Address
        public string? CurrentAddress { get; set; }
        public string? CurrentCity { get; set; }
        public string? CurrentTaluka { get; set; }
        public string? CurrentDistrict { get; set; }
        public string? CurrentState { get; set; }
        public string? CurrentPinCode { get; set; }
        public string? CurrentCountry { get; set; } = "India";

        // Permanent Address
        public bool IsPermanentSame { get; set; }
        public string? PermanentAddress { get; set; }
        public string? PermanentCity { get; set; }
        public string? PermanentTaluka { get; set; }
        public string? PermanentDistrict { get; set; }
        public string? PermanentState { get; set; }
        public string? PermanentPinCode { get; set; }
        public string? PermanentCountry { get; set; } = "India";

        // KYC Details
        public string? OVDType_1 { get; set; }
        public string? OVDNumber_1 { get; set; }
        public DateTime? OVDExpiryDate_1 { get; set; }

        public string? OVDType_2 { get; set; }
        public string? OVDNumber_2 { get; set; }
        public DateTime? OVDExpiryDate_2 { get; set; }

        public string? OVDType_3 { get; set; }
        public string? OVDNumber_3 { get; set; }
        public DateTime? OVDExpiryDate_3 { get; set; }

        public string? AddressProofType { get; set; }
        public string? AddressProofNumber { get; set; }
        public string? FormType { get; set; }

        // Contact
        public string MobileNumber { get; set; } = string.Empty;
        public string? AlternateMobile { get; set; }
        public string? Email { get; set; }

        // Occupation & Income
        public string? Occupation { get; set; }
        public string? EmployerName { get; set; }
        public string? Designation { get; set; }
        public string AnnualIncome { get; set; } = string.Empty;
        public string SourceOfFunds { get; set; } = string.Empty;
        public bool IsPEP { get; set; }
        public bool IsRelatedToPEP { get; set; }

        // Nominee
        public string? NomineeName { get; set; }
        public string? NomineeRelationship { get; set; }
        public DateTime? NomineeDOB { get; set; }
        public int? NomineeAge { get; set; }
        public decimal? NomineeSharePercent { get; set; }
        public string? NomineeAddress { get; set; }
        public string? GuardianName { get; set; }

        public string? BranchCode { get; set; }
        public string? KYCNumber { get; set; }
        public DateTime? DateOfApplication { get; set; }
        public string? PlaceOfApplication { get; set; }
        public string? NamePrefix { get; set; }
        public string? NameLast { get; set; }
        public string? FatherSpouseNamePrefix { get; set; }
        public string? MobileISDCode { get; set; }

        public string? CIFID { get; set; }
        public string? BranchName { get; set; }

        // Nominee 1 Details
        public string? NomineeName1 { get; set; }
        public string? NomineeRelationship1 { get; set; }
        public DateTime? NomineeDOB1 { get; set; }
        public int? NomineeAge1 { get; set; }
        public decimal? NomineeSharePercent1 { get; set; }
        public string? NomineeAddress1 { get; set; }
        public string? GuardianName1 { get; set; }

        // Nominee 2 Details
        public string? NomineeName2 { get; set; }
        public string? NomineeRelationship2 { get; set; }
        public DateTime? NomineeDOB2 { get; set; }
        public int? NomineeAge2 { get; set; }
        public decimal? NomineeSharePercent2 { get; set; }
        public string? NomineeAddress2 { get; set; }
        public string? GuardianName2 { get; set; }

        // OVD Capture Metadata
        public DateTime? OVD1_CaptureDate { get; set; }
        public string? OVD1_Latitude { get; set; }
        public string? OVD1_Longitude { get; set; }
        public string? OVD1_Location { get; set; }

        public DateTime? OVD2_CaptureDate { get; set; }
        public string? OVD2_Latitude { get; set; }
        public string? OVD2_Longitude { get; set; }
        public string? OVD2_Location { get; set; }

        public DateTime? OVD3_CaptureDate { get; set; }
        public string? OVD3_Latitude { get; set; }
        public string? OVD3_Longitude { get; set; }
        public string? OVD3_Location { get; set; }

        public DateTime? OVD4_CaptureDate { get; set; }
        public string? OVD4_Latitude { get; set; }
        public string? OVD4_Longitude { get; set; }
        public string? OVD4_Location { get; set; }

        public int? SubmittedByUserId { get; set; }
    }

    public class CustomerProfileResponse : AccountOpeningRequest
    {
        public int ReferenceID { get; set; }
        public bool IsUpdated { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
