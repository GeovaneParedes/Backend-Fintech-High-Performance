using FintechCore.Api.Infrastructure.Data;

namespace FintechCore.Api.Services;

public sealed class TransactionReprocessingBackgroundService : BackgroundService
{
    private readonly TransactionRepository _repository;
    private readonly ILogger<TransactionReprocessingBackgroundService> _logger;

    public TransactionReprocessingBackgroundService(TransactionRepository repository, ILogger<TransactionReprocessingBackgroundService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[Outbox Worker] Serviço de reprocessamento em segundo plano iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var pendingTransactions = await _repository.GetPendingTransactionsAsync(stoppingToken);

                foreach (var tx in pendingTransactions)
                {
                    _logger.LogInformation($"[Outbox Worker] Reprocessando transação pendente {tx.TransactionId}...");
                    
                    // Simula sincronização / re-tentativa bem-sucedida em segundo plano
                    await _repository.UpdateTransactionStatusAsync(tx.TransactionId, "APPROVED", "AUTH-REPROCESSED-OK", stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Outbox Worker] Erro durante a rodada de reprocessamento.");
            }

            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
    }
}
