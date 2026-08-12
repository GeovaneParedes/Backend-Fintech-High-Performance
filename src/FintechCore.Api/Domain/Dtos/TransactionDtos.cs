namespace FintechCore.Api.Domain.Dtos;

public readonly record struct ProcessTransactionRequest(
    decimal Amount,
    string CardNumberMasked,
    string CardHolder,
    string ExpirationDate,
    string Cvc
);

public readonly record struct ProcessTransactionResponse(
    Guid TransactionId,
    string Status,
    string AuthorizationCode,
    decimal Amount,
    DateTime CreatedAt
);

public readonly record struct TefAuthorizeRequest(
    Guid IdempotencyKey,
    decimal Amount,
    string CardNumberMasked
);

public readonly record struct TefAuthorizeResponse(
    string Status,
    string AuthorizationCode,
    string Message
);

public readonly record struct UserLoginRequest(
    string Email,
    string Password
);

public readonly record struct UserLoginResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresInSeconds
);
