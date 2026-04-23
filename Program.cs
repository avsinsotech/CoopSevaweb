using AVSBackend.Data;
using AVSBackend.Helpers;
using AVSBackend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Initialize encryption key from appsettings.json at startup
EncryptionHelper.EncryptionKey = builder.Configuration["EncryptionSettings:EncryptionKey"]
    ?? throw new InvalidOperationException("EncryptionKey is not configured in appsettings.json.");

// Add services to the container.
builder.Services.AddControllers();

// Configure Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));

// Add injected services
builder.Services.AddHttpClient<ISmsService, GrowwSaaS_SmsService>();
builder.Services.AddHttpClient<IAadharService, AadharService>();
builder.Services.AddHttpClient<IPanService, PanService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseDeveloperExceptionPage(); // Enable detailed errors everywhere for debugging
app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    // Development-specific logic
}

// app.UseHttpsRedirection(); // Commented out for easier HTTP deployment on local networks

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
