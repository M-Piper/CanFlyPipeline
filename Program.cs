using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.EntityFrameworkCore;
using CanFlyPipeline; // Add this to include the db class

var builder = WebApplication.CreateBuilder(args);

// Test database connection
db.TestDatabaseConnection(); // Call the method to test the database connection

// Add authorization policies
builder.Services.AddAuthorization();

// Add controllers and configure JSON serialization
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    options.SerializerSettings.ContractResolver = new DefaultContractResolver();

});

// Add MySQL DbContext
builder.Services.AddDbContext<CanFlyDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("CanFlyDBConn"),
    new MySqlServerVersion(new Version(8, 0, 25))));

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

// Application Insights Telemetry (if needed)
// builder.Services.AddApplicationInsightsTelemetry();

// Get the port from the environment variable set by Railway
var port = 80;

var app = builder.Build();

// Use Swagger and Swagger UI
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

// Enable CORS (for development purposes, adjust as needed)
app.UseCors(options =>
{
    options.AllowAnyHeader()
           .AllowAnyMethod()
           .AllowAnyOrigin();
});

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}

app.UseRouting();

// Map public and private routes
app.MapGet("/", () => "This is a public endpoint").AllowAnonymous();
app.MapGet("/private", () => "Private endpoint").RequireAuthorization();

// Use authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

app.Run();


