using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using AVSBackend.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ExcelDataReader;
using System.Data;
using AVSBackend.Models;
using AVSBackend.DTOs;

namespace AVSBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        private static readonly ConcurrentDictionary<string, (int processed, int total)> _uploadProgress = new();

        [HttpGet("upload-progress/{jobId}")]
        public IActionResult GetUploadProgress(string jobId)
        {
            if (_uploadProgress.TryGetValue(jobId, out var progress))
            {
                return Ok(new { success = true, processed = progress.processed, total = progress.total });
            }
            return Ok(new { success = false, message = "Job not found or completed." });
        }

        public class AdminLoginRequest
        {
            public string UserID { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AdminLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserID) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { success = false, message = "UserID and Password are required." });

            // 1. Check System Admin Table
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.UserID == request.UserID && a.UserPassword == request.Password);
            if (admin != null)
            {
                return Ok(new { 
                    success = true, 
                    message = "Login successful", 
                    role = "Admin",
                    userId = admin.ID,
                    username = admin.UserID,
                    token = Guid.NewGuid().ToString() 
                });
            }

            // 2. Check AppUsers Table for WebAdmin role
            var webAdmin = await _context.AppUsers.FirstOrDefaultAsync(u => 
                u.Username == request.UserID && 
                u.Password == request.Password && 
                u.UserRole == "WebAdmin" && 
                u.IsActive);

            if (webAdmin != null)
            {
                return Ok(new { 
                    success = true, 
                    message = "Login successful", 
                    role = "WebAdmin",
                    userId = webAdmin.UserID,
                    username = webAdmin.Username,
                    token = Guid.NewGuid().ToString() 
                });
            }

            return Unauthorized(new { success = false, message = "Invalid credentials or account inactive." });
        }

        [HttpGet("bank-details")]
        public async Task<IActionResult> GetBankDetails()
        {
            // Assuming we take the first config or BankCode=1
            var details = await _context.BankLogoDetails.FirstOrDefaultAsync() 
                         ?? await _context.BankLogoDetails.FirstOrDefaultAsync(b => b.BankCode == 1);
            
            if (details == null)
                return NotFound(new { success = false, message = "Bank details not found in database." });

            return Ok(new { success = true, data = details });
        }

        [HttpGet("branding-info")]
        public async Task<IActionResult> GetBrandingInfo()
        {
            var details = await _context.BankLogoDetails.FirstOrDefaultAsync()
                         ?? await _context.BankLogoDetails.FirstOrDefaultAsync(b => b.BankCode == 1);

            if (details == null)
                return NotFound(new { success = false, message = "Branding details not found." });

            return Ok(new
            {
                success = true,
                bankname = details.bankname,
                logoBase64 = details.LogoBase64
            });
        }

        [HttpGet("dashboard-stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var totalCustomers = await _context.CustomerFullDetails.CountAsync();
            
            // Assuming "CreatedDate" or similar tracks when the record was made.
            // If they don't have CreatedDate, we'll try to get it. Oh, model has CreatedDate = DateTime.Now;
            var startOfToday = DateTime.Today;
            var todayRecords = await _context.CustomerFullDetails
                                             .Where(c => c.CreatedDate >= startOfToday)
                                             .CountAsync();

            return Ok(new { success = true, totalCustomers, todayRecords });
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchCustomers([FromQuery] string? query, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;

            var baseQuery = _context.CustomerFullDetails.AsQueryable();

            // 1. Date range filtering (if either date is provided)
            if (fromDate.HasValue)
            {
                var startOfFrom = fromDate.Value.Date;
                baseQuery = baseQuery.Where(c => c.CreatedDate >= startOfFrom);
            }

            if (toDate.HasValue)
            {
                var endOfTo = toDate.Value.Date.AddDays(1).AddTicks(-1);
                baseQuery = baseQuery.Where(c => c.CreatedDate <= endOfTo);
            }

            // 2. Text Search filtering (if query is provided)
            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.Trim();
                bool isNumeric = int.TryParse(query, out int referenceId);

                baseQuery = baseQuery.Where(c => 
                    (isNumeric && c.ReferenceID == referenceId) ||
                    (c.AadharNumber != null && c.AadharNumber == query) ||
                    (c.PanNumber != null && c.PanNumber == query) ||
                    (c.MobileNumber != null && c.MobileNumber == query) ||
                    (c.CIFID != null && c.CIFID == query) ||
                    (c.FullName != null && c.FullName.Contains(query))
                );
            }

            // 3. Default behavior: If no query and NO date filters, show today's records
            if (string.IsNullOrWhiteSpace(query) && !fromDate.HasValue && !toDate.HasValue)
            {
                var startOfToday = DateTime.Today;
                baseQuery = baseQuery.Where(c => c.CreatedDate >= startOfToday);
            }

            // 4. Get total count for pagination
            var totalCount = await baseQuery.CountAsync();

            var results = await baseQuery
                .OrderByDescending(c => c.CreatedDate)
                .Select(c => new {
                    referenceID = c.ReferenceID,
                    cifID = c.CIFID,
                    fullName = c.FullName,
                    mobileNumber = c.MobileNumber,
                    aadharNumber = c.AadharNumber,
                    panNumber = c.PanNumber,
                    createdDate = c.CreatedDate
                })
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new { 
                success = true, 
                data = results, 
                totalCount = totalCount,
                totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                currentPage = page
            });
        }

        [HttpPost("update-cifid")]
        public async Task<IActionResult> UpdateCIFID([FromBody] UpdateCIFIDRequest request)
        {
            if (request.ReferenceID <= 0)
                return BadRequest(new { success = false, message = "Invalid ReferenceID" });

            if (string.IsNullOrWhiteSpace(request.CIFID))
                return BadRequest(new { success = false, message = "CIF ID cannot be empty." });

            // Ensure uniqueness
            var existing = await _context.CustomerFullDetails.AnyAsync(c => c.CIFID == request.CIFID && c.ReferenceID != request.ReferenceID);
            if (existing)
                return BadRequest(new { success = false, message = $"The CIF ID '{request.CIFID}' is already assigned to another customer." });

            var customer = await _context.CustomerFullDetails.FindAsync(request.ReferenceID);
            if (customer == null)
                return NotFound(new { success = false, message = "Customer not found" });

            customer.CIFID = request.CIFID;
            
            // Track assignment if AdminID is provided
            if (request.AdminID > 0)
            {
                customer.CIFAssignedBy = request.AdminID;
                customer.CIFAssignedDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "CIFID updated successfully" });
        }

        [HttpPost("record-print")]
        public async Task<IActionResult> RecordPrintAction([FromBody] RecordPrintRequest request)
        {
            if (request.ReferenceID <= 0 || request.AdminID <= 0)
                return BadRequest(new { success = false, message = "Invalid parameters." });

            var customer = await _context.CustomerFullDetails.FindAsync(request.ReferenceID);
            if (customer == null) return NotFound(new { success = false, message = "Customer not found." });

            customer.IsPrinted = true;
            customer.PrintedBy = request.AdminID;
            customer.PrintDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Print action recorded." });
        }

        public class UpdateCIFIDRequest
        {
            public int ReferenceID { get; set; }
            public string? CIFID { get; set; }
            public int AdminID { get; set; }
        }

        public class RecordPrintRequest
        {
            public int ReferenceID { get; set; }
            public int AdminID { get; set; }
        }

        [HttpGet("ckyc-report")]
        public async Task<IActionResult> GetCKYCReport([FromQuery] string? status, [FromQuery] string? query, [FromQuery] string? branch, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] bool all = false)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;

                // ONLY show Excel uploaded data for this specific report
                var baseQuery = _context.CustomerFullDetails.Where(c => c.IsExcelUploaded == true).AsQueryable();

                // 1. Branch Filter
                if (!string.IsNullOrWhiteSpace(branch) && branch != "All")
                {
                    baseQuery = baseQuery.Where(c => c.BranchName == branch);
                }

                // 2. Search Query
                if (!string.IsNullOrWhiteSpace(query))
                {
                    query = query.Trim();
                    bool isNumeric = int.TryParse(query, out int referenceId);

                    baseQuery = baseQuery.Where(c => 
                        (isNumeric && c.ReferenceID == referenceId) ||
                        (c.CIFID != null && c.CIFID == query) ||
                        (c.AadharNumber != null && c.AadharNumber == query) ||
                        (c.PanNumber != null && c.PanNumber == query) ||
                        (c.MobileNumber != null && c.MobileNumber == query) ||
                        (c.FullName != null && c.FullName.Contains(query))
                    );
                }

                // 3. Map to projected object with calculated Status
                var reportQuery = baseQuery.Select(c => new {
                    referenceID = c.ReferenceID,
                    cifID = c.CIFID,
                    fullName = c.FullName,
                    mobileNumber = c.MobileNumber,
                    aadharNumber = c.AadharNumber,
                    panNumber = c.PanNumber,
                    branchName = c.BranchName,
                    createdDate = c.CreatedDate,
                    ckycStatus = (string.IsNullOrEmpty(c.AadharNumber) || string.IsNullOrEmpty(c.PanNumber)) ? "Pending" : "Completed"
                });

                // 4. Filter by Status (calculated field)
                if (!string.IsNullOrWhiteSpace(status) && status != "All")
                {
                    reportQuery = reportQuery.Where(r => r.ckycStatus == status);
                }

                // 5. Get total count
                var totalCount = await reportQuery.CountAsync();

                if (all)
                {
                    var allResults = await reportQuery.OrderByDescending(r => r.createdDate).ToListAsync();
                    return Ok(new { success = true, data = allResults });
                }

                var results = await reportQuery
                    .OrderByDescending(r => r.createdDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return Ok(new { 
                    success = true, 
                    data = results, 
                    totalCount = totalCount,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                    currentPage = page
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error generating report: " + ex.Message });
            }
        }

        [HttpGet("branches")]
        public async Task<IActionResult> GetBranches()
        {
            try
            {
                var branches = await _context.CustomerFullDetails
                    .Where(c => !string.IsNullOrEmpty(c.BranchName))
                    .Select(c => c.BranchName!.Trim())
                    .Distinct()
                    .OrderBy(b => b)
                    .ToListAsync();

                return Ok(new { success = true, data = branches });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "Error fetching branches: " + ex.Message });
            }
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadCustomerZip(int id)
        {
            var customer = await _context.CustomerFullDetails.FindAsync(id);
            if (customer == null)
                return NotFound(new { success = false, message = "Customer not found" });

            using (var memoryStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                {
                    // Create a folder named after the Customer
                    string folderName = GetSafeFileName(customer.FullName.Trim());

                    // Create the directory entry (with trailing slash)
                    archive.CreateEntry($"{folderName}/");
                    
                    // Add OVD Images with 1:1 mapping to Types
                    string ovd1Name = !string.IsNullOrWhiteSpace(customer.OVDType_1) ? GetSafeFileName(customer.OVDType_1) : "OVD1";
                    string ovd2Name = !string.IsNullOrWhiteSpace(customer.OVDType_2) ? GetSafeFileName(customer.OVDType_2) : "OVD2";
                    string ovd3Name = !string.IsNullOrWhiteSpace(customer.OVDType_3) ? GetSafeFileName(customer.OVDType_3) : "OVD3";
                    if (customer.OVDImg1 != null && customer.OVDImg1.Length > 0)
                        AddZipEntry(archive, $"{folderName}/{ovd1Name}.jpg", customer.OVDImg1);
                    
                    if (customer.OVDImg2 != null && customer.OVDImg2.Length > 0)
                        AddZipEntry(archive, $"{folderName}/{ovd2Name}.jpg", customer.OVDImg2);
                    
                    if (customer.OVDImg3 != null && customer.OVDImg3.Length > 0)
                        AddZipEntry(archive, $"{folderName}/{ovd3Name}.jpg", customer.OVDImg3);
                    
                    if (customer.OVDImg4 != null && customer.OVDImg4.Length > 0)
                        AddZipEntry(archive, $"{folderName}/Aadhar_Back.jpg", customer.OVDImg4);

                    if (customer.Form60_61_Img != null && customer.Form60_61_Img.Length > 0)
                        AddZipEntry(archive, $"{folderName}/Form_60_61.jpg", customer.Form60_61_Img);
                    
                    if (customer.Photo != null && customer.Photo.Length > 0)
                        AddZipEntry(archive, $"{folderName}/Photo.jpg", customer.Photo);

                    if (customer.Signature != null && customer.Signature.Length > 0)
                        AddZipEntry(archive, $"{folderName}/Signature.jpg", customer.Signature);
                }

                memoryStream.Position = 0;
                var fileName = $"{customer.ReferenceID}_{customer.CreatedDate:yyyyMMdd}.zip";
                return File(memoryStream.ToArray(), "application/zip", fileName);
            }
        }

        [HttpPost("upload-excel")]
        public async Task<IActionResult> UploadExcel(IFormFile file, [FromQuery] string jobId = null)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { success = false, message = "Please upload a valid Excel file." });

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            // Increase timeout for long-running batch operations (5 minutes)
            _context.Database.SetCommandTimeout(300);

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    IExcelDataReader reader;
                    if (file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                        reader = ExcelReaderFactory.CreateCsvReader(stream);
                    else
                        reader = ExcelReaderFactory.CreateReader(stream);

                    using (reader)
                    {
                        var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration() { UseHeaderRow = false }
                        });

                        var table = result.Tables[0];
                        var pendingCustomers = new List<CustomerFullDetails>();
                        var skippedRows = new List<object>();

                        // --- Dynamic Header Detection Logic ---
                        int headerRowIndex = -1;
                        var colMap = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

                        for (int r = 0; r < Math.Min(table.Rows.Count, 10); r++)
                        {
                            bool hasName = false, hasCif = false;
                            for (int c = 0; c < table.Columns.Count; c++)
                            {
                                string val = table.Rows[r][c]?.ToString()?.Trim() ?? "";
                                if (val.Equals("Name", StringComparison.OrdinalIgnoreCase)) hasName = true;
                                if (val.Equals("BankCustID", StringComparison.OrdinalIgnoreCase) || val.Equals("CIF ID", StringComparison.OrdinalIgnoreCase)) hasCif = true;
                            }
                            if (hasName || hasCif)
                            {
                                headerRowIndex = r;
                                for (int c = 0; c < table.Columns.Count; c++)
                                {
                                    string h = table.Rows[r][c]?.ToString()?.Trim() ?? "";
                                    if (!string.IsNullOrEmpty(h)) colMap[h] = c;
                                }
                                break;
                            }
                        }

                        if (headerRowIndex == -1)
                        {
                            return BadRequest(new { success = false, message = "Could not find headers (Name, BankCustID, etc.) in the Excel file." });
                        }

                        // Tracks duplicates WITHIN the Excel sheet to avoid processing same row twice
                        var seenCifsInSheet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        
                        int rowNumber = headerRowIndex + 1;

                        // 1. Initial Parsing Loop (Memory Efficient)
                        for (int r = headerRowIndex + 1; r < table.Rows.Count; r++)
                        {
                            rowNumber++;
                            var row = table.Rows[r];
                            string GetVal(string col) => colMap.ContainsKey(col) ? row[colMap[col]]?.ToString()?.Trim() ?? "" : "";

                            string name = !string.IsNullOrWhiteSpace(GetVal("Name")) ? GetVal("Name") : $"{GetVal("NameFirst")} {GetVal("NameLast")}".Trim();
                            string mobile = GetVal("Mobile");
                            string cif = GetVal("BankCustID");
                            string aadhar = GetVal("AADHAR NUMBER");
                            string pan = GetVal("PAN");

                            if (string.IsNullOrWhiteSpace(name))
                            {
                                skippedRows.Add(new { row = rowNumber, column = "Name", value = name, reason = "Name is required." });
                                continue;
                            }

                            if (string.IsNullOrWhiteSpace(mobile) || !System.Text.RegularExpressions.Regex.IsMatch(mobile, @"^\d{10}$"))
                            {
                                skippedRows.Add(new { row = rowNumber, column = "Mobile", value = mobile, reason = "Valid 10-digit mobile number required." });
                                continue;
                            }

                            if (!string.IsNullOrWhiteSpace(cif))
                            {
                                if (seenCifsInSheet.Contains(cif))
                                {
                                    skippedRows.Add(new { row = rowNumber, column = "BankCustID", value = cif, reason = $"Duplicate BankCustID in Excel." });
                                    continue;
                                }
                                seenCifsInSheet.Add(cif);
                            }

                            string mergedAddress = $"{GetVal("AddressLine1")} {GetVal("AddressLine2")} {GetVal("AddressLine3")}".Trim();

                            pendingCustomers.Add(new CustomerFullDetails
                            {
                                AadharNumber = aadhar,
                                PanNumber = pan,
                                FullName = name,
                                CIFID = cif,
                                BranchCode = GetVal("BranchCode"),
                                BranchName = GetVal("Branch Name"),
                                FatherOrHusbandName = GetVal("NameMiddle"),
                                NameLast = GetVal("NameLast"),
                                NamePrefix = GetVal("NameFirst"),
                                CurrentAddress = mergedAddress,
                                CurrentCity = GetVal("City"),
                                CurrentDistrict = GetVal("District"),
                                CurrentState = !string.IsNullOrWhiteSpace(GetVal("StateCode")) ? GetVal("StateCode") : "Maharashtra",
                                CurrentPinCode = GetVal("Pin"),
                                CurrentCountry = !string.IsNullOrWhiteSpace(GetVal("ISO3166CountryCode")) ? GetVal("ISO3166CountryCode") : "India",
                                IsPermanentSame = true,
                                MobileNumber = mobile,
                                Email = GetVal("Emailed"),
                                Occupation = !string.IsNullOrWhiteSpace(GetVal("OccupationType")) ? GetVal("OccupationType") : "Service",
                                CreatedDate = DateTime.Now,
                                IsExcelUploaded = true,
                                AnnualIncome = "Not Specified",
                                SourceOfFunds = "Not Specified",
                                MaritalStatus = "Single",
                                ResidentialStatus = "Resident",
                                Nationality = "Indian"
                            });
                        }

                        if (!string.IsNullOrEmpty(jobId))
                        {
                            _uploadProgress[jobId] = (0, pendingCustomers.Count);
                        }

                        // 2. Batch Processing Loop (Database Efficient)
                        int importedCount = 0;
                        int batchSize = 200;

                        for (int i = 0; i < pendingCustomers.Count; i += batchSize)
                        {
                            var batch = pendingCustomers.Skip(i).Take(batchSize).ToList();
                            
                            // Check for DB-side duplicates for this batch specifically
                            // Using a more compatible approach to avoid "Incorrect syntax near WITH"
                            var existingCifs = new List<string>();
                            foreach (var item in batch)
                            {
                                if (!string.IsNullOrEmpty(item.CIFID))
                                {
                                    if (await _context.CustomerFullDetails.AnyAsync(c => c.CIFID == item.CIFID))
                                    {
                                        existingCifs.Add(item.CIFID);
                                    }
                                }
                            }

                            var newRecords = batch.Where(c => string.IsNullOrEmpty(c.CIFID) || !existingCifs.Contains(c.CIFID)).ToList();
                            
                            int batchSkipped = batch.Count - newRecords.Count;
                            if (batchSkipped > 0)
                            {
                                // Optional: log which ones were skipped from DB check
                            }

                            if (newRecords.Any())
                            {
                                _context.CustomerFullDetails.AddRange(newRecords);
                                await _context.SaveChangesAsync();
                                importedCount += newRecords.Count;
                            }

                            // CRITICAL: Clear tracking to free memory for the next batch
                            _context.ChangeTracker.Clear();

                            if (!string.IsNullOrEmpty(jobId))
                            {
                                _uploadProgress[jobId] = (Math.Min(i + batchSize, pendingCustomers.Count), pendingCustomers.Count);
                            }
                        }

                        if (!string.IsNullOrEmpty(jobId)) _uploadProgress.TryRemove(jobId, out _);

                        return Ok(new
                        {
                            success = true,
                            imported = importedCount,
                            totalFoundInExcel = pendingCustomers.Count,
                            skipped = skippedRows.Count + (pendingCustomers.Count - importedCount),
                            skippedDetails = skippedRows
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                var inner = ex.InnerException?.Message ?? "";
                return StatusCode(500, new { success = false, message = $"Error processing Excel file: {ex.Message}. {inner}" });
            }
        }

        private string GetSafeFileName(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "Document";
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                input = input.Replace(c, '_');
            }
            return input.Replace(" ", "_");
        }

        private void AddZipEntry(ZipArchive archive, string entryName, byte[] data)
        {
            var entry = archive.CreateEntry(entryName);
            using (var entryStream = entry.Open())
            {
                entryStream.Write(data, 0, data.Length);
            }
        }

        // --- App User Management & Tracking ---

        [HttpPost("app-users")]
        public async Task<IActionResult> CreateAppUser([FromBody] AppUser request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || 
                string.IsNullOrWhiteSpace(request.Password) || 
                string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.MobileNo))
            {
                return BadRequest(new { success = false, message = "Username, Password, FullName, and Mobile Number are required." });
            }

            if (!IsStrongPassword(request.Password))
            {
                return BadRequest(new { success = false, message = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one digit, and one special character (e.g., Harshad@123)." });
            }

            if (await _context.AppUsers.AnyAsync(u => u.Username == request.Username))
                return BadRequest(new { success = false, message = "Username already exists." });

            request.CreatedDate = DateTime.Now;
            _context.AppUsers.Add(request);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "User created successfully." });
        }

        [HttpGet("app-users")]
        public async Task<IActionResult> GetAppUsers([FromQuery] string? role)
        {
            var query = _context.AppUsers.AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(role))
            {
                if (role == "Staff")
                {
                    // For 'Staff', also include records where UserRole is NULL or empty (backward compatibility)
                    query = query.Where(u => u.UserRole == role || u.UserRole == null || u.UserRole == "");
                }
                else
                {
                    query = query.Where(u => u.UserRole == role);
                }
            }

            var users = await query
                .OrderByDescending(u => u.CreatedDate)
                .Select(u => new {
                    u.UserID,
                    u.Username,
                    u.FullName,
                    u.BranchName,
                    u.MobileNo,
                    u.UserRole,
                    u.CreatedDate
                })
                .ToListAsync();

            return Ok(new { success = true, data = users });
        }

        [HttpGet("app-users/{userId}/stats")]
        public async Task<IActionResult> GetAppUserStats(int userId)
        {
            var user = await _context.AppUsers.FindAsync(userId);
            if (user == null) return NotFound(new { success = false, message = "User not found." });

            if (user.UserRole == "WebAdmin")
            {
                // Get all records where this admin either assigned the CIF OR printed the form
                var allActivity = await _context.CustomerFullDetails
                    .Where(c => c.CIFAssignedBy == userId || c.PrintedBy == userId)
                    .OrderByDescending(c => c.CIFAssignedDate ?? c.PrintDate)
                    .Select(c => new {
                        c.ReferenceID,
                        c.CIFID,
                        c.FullName,
                        // Show checkmark if THIS admin printed it
                        IsPrinted = (c.PrintedBy == userId),
                        // Show CIF if THIS admin assigned it (or if it exists)
                        IsCifAssigned = (c.CIFAssignedBy == userId),
                        ActionDate = c.CIFAssignedDate ?? c.PrintDate
                    })
                    .ToListAsync();

                var totalPrints = await _context.CustomerFullDetails.CountAsync(c => c.PrintedBy == userId);
                var totalCifs = await _context.CustomerFullDetails.CountAsync(c => c.CIFAssignedBy == userId);

                return Ok(new { 
                    success = true, 
                    data = new {
                        userId = user.UserID,
                        fullName = user.FullName,
                        role = user.UserRole,
                        totalCifsAssigned = totalCifs,
                        totalPrints = totalPrints,
                        cifAssignments = allActivity
                    }
                });
            }
            else
            {
                // Staff Stats: Profile Submissions
                var submissions = await _context.CustomerFullDetails
                    .Where(c => c.SubmittedByUserId == userId)
                    .OrderByDescending(c => c.CreatedDate)
                    .Select(c => new {
                        c.ReferenceID,
                        c.CIFID,
                        c.FullName,
                        c.CreatedDate
                    })
                    .ToListAsync();

                return Ok(new { 
                    success = true, 
                    data = new {
                        userId = user.UserID,
                        fullName = user.FullName,
                        role = user.UserRole,
                        totalSubmissions = submissions.Count,
                        submissions = submissions
                    }
                });
            }
        }

        [HttpPost("app-users/{userId}/reset-password")]
        public async Task<IActionResult> ResetAppUserPassword(int userId, [FromBody] AppUserPasswordResetRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { success = false, message = "New password cannot be empty." });
            }

            if (!IsStrongPassword(request.NewPassword))
            {
                return BadRequest(new { success = false, message = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, one digit, and one special character (e.g., Harshad@123)." });
            }

            var user = await _context.AppUsers.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { success = false, message = "User not found." });
            }

            user.Password = request.NewPassword;
            _context.Entry(user).Property(u => u.Password).IsModified = true;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Password reset successfully." });
        }

        private bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;
            // Min 8 chars, 1 Uppercase, 1 Lowercase, 1 Digit, 1 Special Character
            var regex = new System.Text.RegularExpressions.Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,}$");
            return regex.IsMatch(password);
        }
    }
}
