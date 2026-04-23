using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AVSBackend.Models
{
    [Table("tbl_CustomerFullDetails")]
    public class CustomerFullDetails
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ReferenceID")]
        public int ReferenceID { get; set; }

        // Aadhaar & PAN
        [StringLength(500)]
        public string? AadharNumber { get; set; }
        [StringLength(500)]
        public string? PanNumber { get; set; }

        // Personal Details
        [Required]
        [StringLength(150)]
        public string FullName { get; set; } = string.Empty;
        
        [StringLength(150)]
        public string? NameInMarathi { get; set; }
        
        [Required]
        [StringLength(150)]
        public string FatherOrHusbandName { get; set; } = string.Empty;
        
        [StringLength(150)]
        public string? MotherName { get; set; }
        
        [Required]
        public DateTime DOB { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Gender { get; set; } = string.Empty;
        
        [Required]
        [StringLength(20)]
        public string MaritalStatus { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string Nationality { get; set; } = string.Empty;
        
        [StringLength(50)]
        public string? ResidentialStatus { get; set; }
        
        [StringLength(50)]
        public string? Religion { get; set; }
        
        [StringLength(20)]
        public string? Category { get; set; }

        public byte[]? Photo { get; set; }
        public byte[]? Signature { get; set; }

        // Current Address
        public string? CurrentAddress { get; set; }
        [StringLength(100)]
        public string? CurrentCity { get; set; }
        [StringLength(100)]
        public string? CurrentTaluka { get; set; }
        [StringLength(100)]
        public string? CurrentDistrict { get; set; }
        [StringLength(100)]
        public string? CurrentState { get; set; }
        [StringLength(10)]
        public string? CurrentPinCode { get; set; }
        [StringLength(50)]
        public string? CurrentCountry { get; set; } = "India";

        // Permanent Address
        public bool IsPermanentSame { get; set; }
        public string? PermanentAddress { get; set; }
        [StringLength(100)]
        public string? PermanentCity { get; set; }
        [StringLength(100)]
        public string? PermanentTaluka { get; set; }
        [StringLength(100)]
        public string? PermanentDistrict { get; set; }
        [StringLength(100)]
        public string? PermanentState { get; set; }
        [StringLength(10)]
        public string? PermanentPinCode { get; set; }
        [StringLength(50)]
        public string? PermanentCountry { get; set; } = "India";

        // KYC Details
        [StringLength(50)]
        public string? OVDType_1 { get; set; }
        [StringLength(100)]
        public string? OVDNumber_1 { get; set; }
        public DateTime? OVDExpiryDate_1 { get; set; }

        [StringLength(50)]
        public string? OVDType_2 { get; set; }
        [StringLength(100)]
        public string? OVDNumber_2 { get; set; }
        public DateTime? OVDExpiryDate_2 { get; set; }

        [StringLength(50)]
        public string? OVDType_3 { get; set; }
        [StringLength(100)]
        public string? OVDNumber_3 { get; set; }
        public DateTime? OVDExpiryDate_3 { get; set; }

        [StringLength(50)]
        public string? AddressProofType { get; set; }
        [StringLength(100)]
        public string? AddressProofNumber { get; set; }
        [StringLength(10)]
        public string? FormType { get; set; }

        // Contact
        [Required]
        [StringLength(15)]
        public string MobileNumber { get; set; } = string.Empty;
        
        [StringLength(15)]
        public string? AlternateMobile { get; set; }
        
        [StringLength(100)]
        public string? Email { get; set; }

        // Occupation & Income
        [StringLength(50)]
        public string? Occupation { get; set; }
        [StringLength(150)]
        public string? EmployerName { get; set; }
        [StringLength(100)]
        public string? Designation { get; set; }
        
        [Required]
        [StringLength(50)]
        public string AnnualIncome { get; set; } = string.Empty;
        
        [Required]
        [StringLength(50)]
        public string SourceOfFunds { get; set; } = string.Empty;
        
        public bool IsPEP { get; set; }
        public bool IsRelatedToPEP { get; set; }

        // Nominee
        [StringLength(150)]
        public string? NomineeName { get; set; }
        [StringLength(50)]
        public string? NomineeRelationship { get; set; }
        public DateTime? NomineeDOB { get; set; }
        public int? NomineeAge { get; set; }
        
        [Column(TypeName = "decimal(5, 2)")]
        public decimal? NomineeSharePercent { get; set; }
        
        public string? NomineeAddress { get; set; }
        
        [StringLength(150)]
        public string? GuardianName { get; set; }

        // New Excel Related Fields
        [StringLength(50)]
        public string? BranchCode { get; set; }
        [StringLength(50)]
        public string? KYCNumber { get; set; }
        public DateTime? DateOfApplication { get; set; }
        [StringLength(100)]
        public string? PlaceOfApplication { get; set; }
        [StringLength(20)]
        public string? NamePrefix { get; set; }
        [StringLength(100)]
        public string? NameLast { get; set; }
        [StringLength(20)]
        public string? FatherSpouseNamePrefix { get; set; }
        [StringLength(10)]
        public string? MobileISDCode { get; set; }

        public byte[]? OVDImg1 { get; set; }
        public byte[]? OVDImg2 { get; set; }
        public byte[]? OVDImg3 { get; set; }
        public byte[]? OVDImg4 { get; set; }
        public byte[]? Form60_61_Img { get; set; }

        [StringLength(50)]
        public string? CIFID { get; set; }
        [StringLength(100)]
        public string? BranchName { get; set; }

        // Nominee 1 Details
        [StringLength(150)]
        public string? NomineeName1 { get; set; }
        [StringLength(50)]
        public string? NomineeRelationship1 { get; set; }
        public DateTime? NomineeDOB1 { get; set; }
        public int? NomineeAge1 { get; set; }
        [Column(TypeName = "decimal(5, 2)")]
        public decimal? NomineeSharePercent1 { get; set; }
        public string? NomineeAddress1 { get; set; }
        [StringLength(150)]
        public string? GuardianName1 { get; set; }

        // Nominee 2 Details
        [StringLength(150)]
        public string? NomineeName2 { get; set; }
        [StringLength(50)]
        public string? NomineeRelationship2 { get; set; }
        public DateTime? NomineeDOB2 { get; set; }
        public int? NomineeAge2 { get; set; }
        [Column(TypeName = "decimal(5, 2)")]
        public decimal? NomineeSharePercent2 { get; set; }
        public string? NomineeAddress2 { get; set; }
        [StringLength(150)]
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

        public bool IsUpdated { get; set; } = false;
        public bool IsExcelUploaded { get; set; } = false;
        public int? SubmittedByUserId { get; set; }

        // Tracking for Web Admin Activity
        public int? CIFAssignedBy { get; set; }
        public DateTime? CIFAssignedDate { get; set; }
        public bool IsPrinted { get; set; } = false;
        public int? PrintedBy { get; set; }
        public DateTime? PrintDate { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
