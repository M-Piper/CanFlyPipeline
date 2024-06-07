using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System.Text;
using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
builder.Services.AddAuthorization();

// Add services to container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

// JSON Serializer
builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
});

// Application Insights Telemetry
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

//added for swagger in production as well as development
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    // Specify the base URL of your API in Production
    var swaggerJsonEndpoint = app.Environment.IsDevelopment()
        ? "/swagger/v1/swagger.json"
        : "https://canfly-backend.azurewebsites.net/swagger/v1/swagger.json";

    c.SwaggerEndpoint(swaggerJsonEndpoint, "My API V1");
});

app.UseRouting();

// Enable CORS
app.UseCors(c => c.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });
}

//app.UseRouting();

// Routes and public routes can make anonymous requests
//app.MapGet("/", () => "Hello World!");
//app.MapGet("/public", () => "Public Hello World!").AllowAnonymous();

// Routes for private require authorized request
//app.MapGet("/private", () => "Private Hello World!").RequireAuthorization();

app.UseAuthorization();

app.MapControllers();

app.Run();
