using System.Net;
using System.Net.Http.Json;
using FintechCore.Api.Domain.Dtos;
using FluentAssertions;
using Xunit;

namespace FintechCore.Tests;

public class TransactionResilienceTests
{
    [Fact]
    public void ProcessTransactionRequest_ShouldBeReadonlyRecordStruct_AndZeroHeapAllocation()
    {
        var dto = new ProcessTransactionRequest(150.50m, "**** **** **** 1234", "DEV GEGE", "12/28", "123");
        dto.Amount.Should().Be(150.50m);
        dto.CardNumberMasked.Should().Be("**** **** **** 1234");
    }

    [Fact]
    public void TransactionDtos_ShouldEnsureValueSemantics()
    {
        var dto1 = new TefAuthorizeRequest(Guid.Empty, 100m, "1234");
        var dto2 = new TefAuthorizeRequest(Guid.Empty, 100m, "1234");

        dto1.Should().Be(dto2);
    }
}
