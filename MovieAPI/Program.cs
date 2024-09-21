using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MovieAPI.Data;
using MovieAPI.Services;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;
using System.Net;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 21))));

var cognitoAuthority = $"https://cognito-idp.{builder.Configuration["AWS:Region"]}.amazonaws.com/{builder.Configuration["AWS:UserPoolId"]}";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
        {
            // Fetch JWKS from AWS Cognito
            var json = new WebClient().DownloadString(validationParameters.ValidIssuer + "/.well-known/jwks.json");
            var jwks = new JsonWebKeySet(json);
            return jwks.GetSigningKeys();
        },
        ValidIssuer = cognitoAuthority, // Ensure the issuer matches your Cognito pool
        ValidateIssuer = true,          // Validate the issuer
        ValidateIssuerSigningKey = true, // Validate the signing key
        ValidateLifetime = true,        // Ensure the token hasn't expired
        ValidateAudience = false,       // AWS Access Token does not have 'aud'
    };

    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            // Log authentication failure
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // Customize the "Access Denied" response
            context.HandleResponse();
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                var result = JsonSerializer.Serialize(new { message = "Access Denied: Please log in first." });
                return context.Response.WriteAsync(result);
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MovieApp API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Enter your JWT token obtained from AWS Cognito.\nExample: 'Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

builder.Services.AddScoped<ICognitoService, CognitoService>();
builder.Services.AddScoped<FavouriteService>();

var redisConnectionString = builder.Configuration["Redis:ConnectionString"];
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
builder.Services.AddSingleton<RedisCacheService>();

builder.Services.AddHttpClient<MovieService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

builder.Services.AddControllers();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MovieApp API v1");
    c.RoutePrefix = "swagger";
});

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
