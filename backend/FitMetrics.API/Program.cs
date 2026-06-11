using System.Text;
using System.Text.Json.Serialization;
using FitMetrics.API.Filters;
using FitMetrics.API.Middleware;
using FitMetrics.Application;
using FitMetrics.Application.Common.Settings;
using FitMetrics.Infrastructure;
using FitMetrics.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---- Katman servisleri ----
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ---- JWT ----
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

// ---- Controllers + JSON (enum'lar string olarak) + global validation ----
builder.Services
    .AddControllers(options => options.Filters.Add<ValidationFilter>())
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

// ---- CORS ----
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? ["http://localhost:5173"];
builder.Services.AddCors(options =>
{
    options.AddPolicy("FitMetricsCors", policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod());
});

// ---- Swagger (JWT destekli) ----
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FitMetrics API",
        Version = "v1",
        Description = "Yapay zekâ destekli beslenme, antrenman ve sağlık takip platformu."
    });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT token'ı 'Bearer {token}' formatında girin.",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };

    c.AddSecurityDefinition("Bearer", jwtScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { [jwtScheme] = Array.Empty<string>() });
});

var app = builder.Build();

// ---- Başlangıçta migration'ları uygula (DB + seed hazır gelir) ----
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    // Test modu: kullanıcı yoksa otomatik test kullanıcısı oluştur
    if (!db.Users.Any())
    {
        var hasher = scope.ServiceProvider.GetRequiredService<FitMetrics.Application.Common.Interfaces.IPasswordHasher>();
        var tdee = FitMetrics.Application.Common.Helpers.HealthCalculator.CalculateTdee(
            FitMetrics.Domain.Enums.Gender.Male, 75, 175, 25, FitMetrics.Domain.Enums.ActivityLevel.Moderate);
        var testUser = new FitMetrics.Domain.Entities.User
        {
            FullName = "Test Kullanıcı",
            Email = "test@fitmetrics.com",
            PasswordHash = hasher.Hash("test123"),
            Age = 25,
            Gender = FitMetrics.Domain.Enums.Gender.Male,
            HeightCm = 175,
            CurrentWeightKg = 75,
            ActivityLevel = FitMetrics.Domain.Enums.ActivityLevel.Moderate,
            GoalType = FitMetrics.Domain.Enums.GoalType.MaintainWeight,
            DailyCalorieGoal = FitMetrics.Application.Common.Helpers.HealthCalculator.CalculateCalorieGoal(
                FitMetrics.Domain.Enums.GoalType.MaintainWeight, tdee),
            DailyProteinGoal = FitMetrics.Application.Common.Helpers.HealthCalculator.CalculateProteinGoal(
                FitMetrics.Domain.Enums.GoalType.MaintainWeight, 75),
        };
        db.Users.Add(testUser);
        db.SaveChanges();
        db.WeightEntries.Add(new FitMetrics.Domain.Entities.WeightEntry
        {
            UserId = testUser.Id,
            WeightKg = 75,
            RecordedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("FitMetricsCors");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
