using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Simulação de Idempotência em memória do TEF
var processedIdempotencyKeys = new ConcurrentDictionary<string, (string Status, string AuthCode)>();
var random = new Random();

app.MapPost("/api/v1/tef/authorize", async (HttpContext context) =>
{
    var request = await context.Request.ReadFromJsonAsync<TefAuthorizeRequest>();

    // 1. Verificação de Idempotência
    var idempotencyKey = request.IdempotencyKey.ToString();
    if (processedIdempotencyKeys.TryGetValue(idempotencyKey, out var existingResult))
    {
        return Results.Ok(new TefAuthorizeResponse(existingResult.Status, existingResult.AuthCode, "Transação reconhecida por Idempotência."));
    }

    // 2. Simulação de Latência Aleatória de Rede (50ms a 300ms)
    await Task.Delay(random.Next(50, 300));

    // 3. Simulação de Caos / Falha Aleatória (10% de chance de erro de comunicação)
    if (random.Next(1, 100) <= 10)
    {
        return Results.Problem("Simulação TEF: Erro temporário de comunicação.", statusCode: 503);
    }

    // 4. Lógica Comercial da Maquininha
    string status = request.Amount > 5000 ? "InsufficientFunds" : "Approved";
    string authCode = status == "Approved" ? $"AUTH-{random.Next(100000, 999999)}" : string.Empty;

    processedIdempotencyKeys.TryAdd(idempotencyKey, (status, authCode));

    return Results.Ok(new TefAuthorizeResponse(status, authCode, "Processado pelo TEF."));
});

app.Run("http://*:5001");

public readonly record struct TefAuthorizeRequest(Guid IdempotencyKey, decimal Amount, string CardNumberMasked);
public readonly record struct TefAuthorizeResponse(string Status, string AuthorizationCode, string Message);
