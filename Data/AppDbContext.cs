using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using AVSBackend.Models;
using AVSBackend.Helpers;

namespace AVSBackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<OtpRecord> OtpRecords { get; set; }
        public DbSet<AadharData> AadharDatas { get; set; }
        public DbSet<PanData> PanDatas { get; set; }
        public DbSet<CustomerFullDetails> CustomerFullDetails { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<BankLogoDetails> BankLogoDetails { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // -------------------------------------------------------
            // AES-256 ValueConverter: encrypts on save, decrypts on load
            // -------------------------------------------------------
            var encryptConverter = new ValueConverter<string, string>(
                v => EncryptionHelper.Encrypt(v) ?? v,      // To Database: encrypt
                v => EncryptionHelper.Decrypt(v) ?? v       // From Database: decrypt
            );

            // Nullable variant for optional fields (AadharNumber, PanNumber in CustomerFullDetails)
            var encryptConverterNullable = new ValueConverter<string?, string?>(
                v => v == null ? null : EncryptionHelper.Encrypt(v),
                v => v == null ? null : EncryptionHelper.Decrypt(v)
            );

            // -------------------------------------------------------
            // Table and Key mappings
            // -------------------------------------------------------
            modelBuilder.Entity<OtpRecord>().ToTable("tbl_MobileOTPLogin");

            // AadharData: encrypt AadharNumber column
            modelBuilder.Entity<AadharData>()
                .ToTable("tbl_AadharData")
                .HasKey(a => a.AadharNumber);
            modelBuilder.Entity<AadharData>()
                .Property(a => a.AadharNumber)
                .HasConversion(encryptConverter);

            // PanData: encrypt PanNumber column
            modelBuilder.Entity<PanData>()
                .ToTable("tbl_PanData")
                .HasKey(p => p.PanNumber);
            modelBuilder.Entity<PanData>()
                .Property(p => p.PanNumber)
                .HasConversion(encryptConverter);

            // CustomerFullDetails: encrypt AadharNumber and PanNumber columns
            modelBuilder.Entity<CustomerFullDetails>()
                .ToTable("tbl_CustomerFullDetails")
                .HasKey(c => c.ReferenceID);

            modelBuilder.Entity<CustomerFullDetails>()
                .Property(c => c.ReferenceID)
                .HasColumnName("ReferenceID");

            modelBuilder.Entity<CustomerFullDetails>()
                .Property(c => c.AadharNumber)
                .HasConversion(encryptConverterNullable);

            modelBuilder.Entity<CustomerFullDetails>()
                .Property(c => c.PanNumber)
                .HasConversion(encryptConverterNullable);
        
            modelBuilder.Entity<CustomerFullDetails>()
                .Property(c => c.OVDNumber_1)
                .HasConversion(encryptConverterNullable);

            modelBuilder.Entity<CustomerFullDetails>()
                .Property(c => c.OVDNumber_2)
                .HasConversion(encryptConverterNullable);

            modelBuilder.Entity<CustomerFullDetails>()
                .Property(c => c.OVDNumber_3)
                .HasConversion(encryptConverterNullable);

            // BankLogoDetails
            modelBuilder.Entity<BankLogoDetails>()
                .ToTable("BanklogoDetails1")
                .HasKey(b => b.BankCode);
        }
    }
}
