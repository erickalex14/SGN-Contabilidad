using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NovitecContabilidad.Data;
using NovitecContabilidad.Services;
using NovitecContabilidad.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure DB connection (MySQL)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "server=YOUR_SERVER_IP;port=27639;database=novitecdb_pruebas;user=root;password=YOUR_DB_PASSWORD;";

builder.Services.AddDbContext<AccountingDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// 2. Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "YOUR_JWT_SECRET";
var key = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero
    };
});

// 3. Register Repositories and Services
builder.Services.AddScoped<ICajaChicaRepository, CajaChicaRepository>();
builder.Services.AddScoped<ICajaChicaService, CajaChicaService>();

// 4. Register Excel Export Service
string templatePath = Path.Combine(builder.Environment.ContentRootPath, "PLANTILLA_CAJA_CHICA_NOVICOMPU.xlsx");
builder.Services.AddSingleton(new ExcelExportService(templatePath));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 4. Configure Swagger with JWT Support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Novitec Contabilidad API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Encabezado de autorización JWT. Ejemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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
                }
            },
            Array.Empty<string>()
        }
    });
});

// 5. Enable CORS for local/prod SGN domain
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSGN", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowSGN");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
