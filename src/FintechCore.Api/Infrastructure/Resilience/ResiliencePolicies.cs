using Polly;
using Polly.Extensions.Http;

namespace FintechCore.Api.Infrastructure.Resilience;

public static class ResiliencePolicies
{
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        var random = new Random();

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromMilliseconds(Math.Pow(2, retryAttempt) * 100) +
                    TimeSpan.FromMilliseconds(random.Next(0, 100)), // Jitter
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    Console.WriteLine($"[Polly Retry] Tentativa {retryAttempt} após {timespan.TotalMilliseconds}ms. Erro: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                });
    }

    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(15),
                onBreak: (outcome, breakDuration) =>
                {
                    Console.WriteLine($"[Polly CircuitBreaker] Circuito ABERTO por {breakDuration.TotalSeconds}s devido a falhas consecutivas. Erro: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                },
                onReset: () =>
                {
                    Console.WriteLine("[Polly CircuitBreaker] Circuito FECHADO. Operação normalizada.");
                });
    }
}
