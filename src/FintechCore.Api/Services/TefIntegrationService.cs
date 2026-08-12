using System.Net.Http.Json;
using System.Net.Sockets;
using FintechCore.Api.Domain.Dtos;
using FintechCore.Api.Infrastructure.Data;
using FintechCore.Api.Infrastructure.Serialization;
using Polly.CircuitBreaker;

namespace FintechCore.Api.Services;

public sealed class TefIntegrationService
{
    private readonly HttpClient _httpClient;
    private readonly TransactionRepository _repository;
    private readonly ILogger<TefIntegrationService> _logger;

    public TefIntegrationService(HttpClient httpClient, TransactionRepository repository, ILogger<TefIntegrationService> logger)
    {
        _httpClient = httpClient;
        _repository = repository;
        _logger = logger;
    }

    public async ValueTask<ProcessTransactionResponse> ProcessTransactionAsync(ProcessTransactionRequest request, CancellationToken cancellationToken = default)
    {
        var transactionId = Guid.NewGuid();
        var idempotencyKey = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var tefRequest = new TefAuthorizeRequest(idempotencyKey, request.Amount, request.CardNumberMasked);

        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/tef/authorize", tefRequest, AppJsonSerializerContext.Default.TefAuthorizeRequest, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var tefResult = await response.Content.ReadFromJsonAsync(AppJsonSerializerContext.Default.TefAuthorizeResponse, cancellationToken);

                var status = tefResult.Status switch
                {
                    "Approved" => "APPROVED",
                    "InsufficientFunds" => "DECLINED_INSUFFICIENT_FUNDS",
                    _ => "DECLINED_UNKNOWN"
                };

                await _repository.SaveTransactionAsync(transactionId, idempotencyKey, request.Amount, request.CardNumberMasked, status, tefResult.AuthorizationCode, createdAt, cancellationToken);

                return new ProcessTransactionResponse(transactionId, status, tefResult.AuthorizationCode ?? string.Empty, request.Amount, createdAt);
            }
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is SocketException || ex is TimeoutException || ex is BrokenCircuitException)
        {
            _logger.LogWarning(ex, "[TEF Falha Conexão/Circuito] Transação salva com status PENDING_RETRY para reprocessamento assíncrono.");

            // Fallback: Salva no banco local como PENDING_RETRY para o BackgroundService processar depois
            await _repository.SaveTransactionAsync(transactionId, idempotencyKey, request.Amount, request.CardNumberMasked, "PENDING_RETRY", null, createdAt, cancellationToken);

            return new ProcessTransactionResponse(transactionId, "PENDING_RETRY", string.Empty, request.Amount, createdAt);
        }

        await _repository.SaveTransactionAsync(transactionId, idempotencyKey, request.Amount, request.CardNumberMasked, "FAILED", null, createdAt, cancellationToken);
        return new ProcessTransactionResponse(transactionId, "FAILED", string.Empty, request.Amount, createdAt);
    }
}
