using AppReservasAPI.Context;
using AppReservasAPI.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Connection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new Exception("Connection string no configurada.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new Exception("JWT Key no configurada.");
}

if (string.IsNullOrWhiteSpace(jwtIssuer))
{
    throw new Exception("JWT Issuer no configurado.");
}

if (string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new Exception("JWT Audience no configurado.");
}

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new Exception("JWT Key debe tener al menos 32 caracteres.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "https://viajespro-frontend.vercel.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    var apiKeyHeaderName = builder.Configuration["ApiKey:HeaderName"] ?? "X-API-KEY";

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "App Reservas API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT. Ejemplo: Bearer eyJhbGciOiJIUzI1NiIs...",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Ingrese la API Key.",
        Name = apiKeyHeaderName,
        Type = SecuritySchemeType.ApiKey
    });

    options.AddSecurityRequirement(document => new()
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = [],
        [new OpenApiSecuritySchemeReference("ApiKey", document)] = []
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();

app.UseCors("FrontendPolicy");

// En Somee puede dar problemas si no tenés HTTPS bien configurado.
// app.UseHttpsRedirection();

app.UseMiddleware<ApiKeyMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
