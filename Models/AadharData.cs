using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AVSBackend.Models
{
    [Table("tbl_AadharData")]
    public class AadharData
    {
        [Key]
        [Required]
        [StringLength(100)]
        public string AadharNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(100)]
        public string? AgeRange { get; set; }

        public bool? IsMobile { get; set; }

        [StringLength(100)]
        public string? MobileLastDigit { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }

        public string? Address { get; set; }

        [StringLength(100)]
        public string? District { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(100)]
        public string? Zip { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(100)]
        public string? ClientId { get; set; }

        [Required]
        [StringLength(50)]
        public string VerifiedBy { get; set; } = "AVS_API";

        public DateTime? VerifiedDate { get; set; }

        public bool IsVerified { get; set; }

        [StringLength(200)]
        public string? VerifiedByClientID { get; set; }

        [Column(TypeName = "image")]
        public byte[]? Photo_Image { get; set; }

        public string? po { get; set; }
        public string? loc { get; set; }
        public string? vtc { get; set; }
        public string? subdist { get; set; }
        public string? street { get; set; }
        public string? house { get; set; }
        public string? landmark { get; set; }
        public string? dist { get; set; }

        [StringLength(50)]
        public string? CUSTNO { get; set; }

        public DateTime? DTOPEN { get; set; }

        [StringLength(50)]
        public string? CKYCStatus { get; set; }

        public string? ADHAR_INCRIPT { get; set; }

        [StringLength(50)]
        public string? ADHARREFNO { get; set; }

        [StringLength(50)]
        public string? RBILicenseNo { get; set; }

        [StringLength(50)]
        public string? RegistrationNo { get; set; }

        [Column(TypeName = "image")]
        public byte[]? Live_Image { get; set; }

        [StringLength(50)]
        public string? Latitude { get; set; }

        [StringLength(50)]
        public string? Longitude { get; set; }

        [StringLength(200)]
        public string? LocationName { get; set; }

        public DateTime? LocationCapturedOn { get; set; }
    }
}
