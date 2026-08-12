using System.Data;
using Dapper;
using FintechCore.Api.Domain.Dtos;
using Microsoft.Data.Sqlite;

namespace FintechCore.Api.Infrastructure.Data;

public sealed class TransactionRepository
{
    private readonly string _connectionString;

    public TransactionRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
                            ?? "Data Source=fintech.db;Cache=Shared;";
        
        EnsureDatabaseCreated();
    }

    private IDbConnection CreateConnection() => new SqliteConnection(_connectionString);

    private void EnsureDatabaseCreated()
    {
        using var connection = CreateConnection();
        connection.Execute(@"
            PRAGMA journal_mode = WAL;
            PRAGMA synchronous = NORMAL;

            CREATE TABLE IF NOT EXISTS Transactions (
                Id TEXT PRIMARY KEY,
                IdempotencyKey TEXT NOT NULL UNIQUE,
                Amount REAL NOT NULL,
                CardNumberMasked TEXT NOT NULL,
                Status TEXT NOT NULL,
                AuthorizationCode TEXT,
                CreatedAt TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS Users (
                Id TEXT PRIMARY KEY,
                Email TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL,
                CreatedAt TEXT NOT NULL
            );
        ");
    }

    public async ValueTask SaveTransactionAsync(Guid id, Guid idempotencyKey, decimal amount, string cardNumberMasked, string status, string? authCode, DateTime createdAt, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"
            INSERT INTO Transactions (Id, IdempotencyKey, Amount, CardNumberMasked, Status, AuthorizationCode, CreatedAt)
            VALUES (@Id, @IdempotencyKey, @Amount, @CardNumberMasked, @Status, @AuthorizationCode, @CreatedAt);";

        var command = new CommandDefinition(sql, new
        {
            Id = id.ToString(),
            IdempotencyKey = idempotencyKey.ToString(),
            Amount = (double)amount,
            CardNumberMasked = cardNumberMasked,
            Status = status,
            AuthorizationCode = authCode,
            CreatedAt = createdAt.ToString("o")
        }, cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async ValueTask UpdateTransactionStatusAsync(Guid id, string status, string? authCode, CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"
            UPDATE Transactions 
            SET Status = @Status, AuthorizationCode = @AuthorizationCode 
            WHERE Id = @Id;";

        var command = new CommandDefinition(sql, new
        {
            Id = id.ToString(),
            Status = status,
            AuthorizationCode = authCode
        }, cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async ValueTask<IEnumerable<ProcessTransactionResponse>> GetPendingTransactionsAsync(CancellationToken cancellationToken = default)
    {
        using var connection = CreateConnection();
        const string sql = @"
            SELECT Id, Status, AuthorizationCode, Amount, CreatedAt 
            FROM Transactions 
            WHERE Status = 'PENDING_RETRY';";

        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
        var rows = await connection.QueryAsync(sql, command);

        return rows.Select(r => new ProcessTransactionResponse(
            Guid.Parse((string)r.Id),
            (string)r.Status,
            (string)(r.AuthorizationCode ?? string.Empty),
            Convert.ToDecimal(r.Amount),
            DateTime.Parse((string)r.CreatedAt)
        ));
    }
}
