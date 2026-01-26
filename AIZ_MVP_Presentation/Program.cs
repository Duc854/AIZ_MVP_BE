using AIZ_MVP_Presentation.Configurations;
using AIZ_MVP_Presentation.Extensions;
using Shared.Models;

namespace AIZ_MVP_Presentation
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddCustomAuthentication(builder.Configuration);
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddAuthorization();
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy.WithOrigins("http://localhost:3000")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });

                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });
            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection("Jwt")
            );

            var app = builder.Build();

            // Log database connection info at startup (Development only)
            if (app.Environment.IsDevelopment())
            {
                var connectionString = builder.Configuration.GetConnectionString("SQLSeverConnection");
                if (!string.IsNullOrEmpty(connectionString))
                {
                    // Redact password if present
                    var redactedConnection = connectionString;
                    var passwordIndex = connectionString.IndexOf("Password=", StringComparison.OrdinalIgnoreCase);
                    if (passwordIndex >= 0)
                    {
                        var passwordEnd = connectionString.IndexOf(";", passwordIndex);
                        if (passwordEnd < 0) passwordEnd = connectionString.Length;
                        redactedConnection = connectionString.Substring(0, passwordIndex + 9) + "***REDACTED***" + 
                                          (passwordEnd < connectionString.Length ? connectionString.Substring(passwordEnd) : "");
                    }
                    
                    // Extract database name
                    var dbNameMatch = System.Text.RegularExpressions.Regex.Match(connectionString, @"Database=([^;]+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    var dbName = dbNameMatch.Success ? dbNameMatch.Groups[1].Value : "Unknown";
                    
                    var logger = app.Services.GetRequiredService<ILogger<Program>>();
                    logger.LogInformation("=== DATABASE CONNECTION INFO ===");
                    logger.LogInformation("Database Name: {DatabaseName}", dbName);
                    logger.LogInformation("Connection String (redacted): {ConnectionString}", redactedConnection);
                    logger.LogInformation("=================================");
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
