using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FintechCore.Api.Domain.Dtos;
using FintechCore.Api.Infrastructure.Data;
using FintechCore.Api.Infrastructure.Resilience;
using FintechCore.Api.Infrastructure.Serialization;
using FintechCore.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar Serialização de JSON High-Performance via STJ Source Generators
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

// 2. Autenticação JWT
var jwtKey = Encoding.UTF8.GetBytes("SuperSecretFintechKeyHighPerformanceResilientKey2026!");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

// 3. Registrar Dapper Repository & Services
builder.Services.AddSingleton<TransactionRepository>();
builder.Services.AddHostedService<TransactionReprocessingBackgroundService>();

// 4. Configurar HttpClient do Fake TEF com políticas Polly (Retry + Circuit Breaker)
builder.Services.AddHttpClient<TefIntegrationService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["TefService:BaseUrl"] ?? "http://localhost:5001");
    client.Timeout = TimeSpan.FromSeconds(5);
})
.AddPolicyHandler(ResiliencePolicies.GetRetryPolicy())
.AddPolicyHandler(ResiliencePolicies.GetCircuitBreakerPolicy());

// 5. Configurar Swagger / OpenAPI com suporte ao botão Authorize Bearer JWT
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Fintech Core API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Insira o token JWT gerado na rota /api/v1/login neste formato: Bearer {seu_token}",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseAuthentication();
app.UseAuthorization();

// 5. Minimal APIs (Sem Controllers convencionais)
app.MapPost("/api/v1/login", (UserLoginRequest request) =>
{
    if (request.Email == "admin@fintech.com" && request.Password == "admin123")
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([new Claim(ClaimTypes.Name, request.Email)]),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(jwtKey), SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwtString = tokenHandler.WriteToken(token);

        return Results.Ok(new UserLoginResponse(jwtString, Guid.NewGuid().ToString(), 900));
    }
    return Results.Unauthorized();
});

app.MapPost("/api/v1/transactions/process", async (ProcessTransactionRequest request, TefIntegrationService tefService, CancellationToken ct) =>
{
    if (request.Amount <= 0)
        return Results.BadRequest(new { error = "O valor da transação deve ser positivo." });

    var response = await tefService.ProcessTransactionAsync(request, ct);
    return Results.Ok(response);
}).RequireAuthorization();

app.Run();
