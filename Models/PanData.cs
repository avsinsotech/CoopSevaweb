using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AVSBackend.Models
{
    [Table("tbl_PanData")]
    public class PanData
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Key]
        [Required]
        [StringLength(100)]
        public string PanNumber { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(100)]
        public string? FatherName { get; set; }

        [Column(TypeName = "date")]
        public DateTime? DOB { get; set; }

        [Required]
        [StringLength(100)]
        public string ClientId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string VerifiedBy { get; set; } = "AVS_API";

        [Column(TypeName = "date")]
        public DateTime VerifiedDate { get; set; }
    }
}
