using System.Text.Json.Serialization;
using FintechCore.Api.Domain.Dtos;

namespace FintechCore.Api.Infrastructure.Serialization;

[JsonSourceGenerationOptions(
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ProcessTransactionRequest))]
[JsonSerializable(typeof(ProcessTransactionResponse))]
[JsonSerializable(typeof(TefAuthorizeRequest))]
[JsonSerializable(typeof(TefAuthorizeResponse))]
[JsonSerializable(typeof(UserLoginRequest))]
[JsonSerializable(typeof(UserLoginResponse))]
public partial class AppJsonSerializerContext : JsonSerializerContext;
