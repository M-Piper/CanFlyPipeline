using Newtonsoft.Json.Serialization;
using CanFlyPipeline.JwtAuthentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

//variable for jwtOptions
var jwtOptions = builder.Configuration
    .GetSection("JwtOptions")
    .Get<JwtOptions>();

//add services to container
builder.Services.AddSingleton(jwtOptions);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuring the Authentication Service
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        //convert the string signing key to byte array
        byte[] signingKeyBytes = Encoding.UTF8
            .GetBytes(jwtOptions.SigningKey);

        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes)
        };
    });

// Configuring the Authorization Service
builder.Services.AddAuthorization();

//JSON Serializer
builder.Services.AddControllers().AddNewtonsoftJson(options =>
options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore).AddNewtonsoftJson
(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());
builder.Services.AddApplicationInsightsTelemetry();

var app = builder.Build();

// add Authentication Middleware
app.UseAuthentication();
// add Authorization Middleware
app.UseAuthorization();

// routes and public routes can make anonymous requests
app.MapGet("/", () => "Hello World!");
app.MapGet("/public", () => "Public Hello World!")
    .AllowAnonymous();

// routes for private require authorized request
app.MapGet("/private", () => "Private Hello World!")
    .RequireAuthorization();

// handles the request token endpoint
app.MapPost("/tokens/connect", (HttpContext ctx, JwtOptions jwtOptions)
    => CanFlyPipeline.JwtAuthentication.Endpoints.TokenEndpoint.Connect(ctx, jwtOptions));

//read the jwt token from header
app.MapGet("/jwt-token/headers", (HttpContext ctx) =>
{
    if (ctx.Request.Headers.TryGetValue("Authorization", out var headerAuth))
    {
        var jwtToken = headerAuth.First().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[1];
        return Task.FromResult(TypedResults.Ok(new { token = jwtToken }));
    }
    return Task.FromResult(TypedResults.NotFound(new { message = "jwt not found" }));
});

//read the jwt token from authentication context
app.MapGet("/jwt-token/context", async (HttpContext ctx) =>
{
    var token = await ctx.GetTokenAsync("access_token");

    return TypedResults.Ok(new { token = token });
});

// Configuring the Authorization Service
builder.Services.AddAuthorization();

//Enable CORS
app.UseCors(c => c.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
