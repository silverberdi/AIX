using AIX.SharedKernel.Primitives;
using FluentAssertions;

namespace AIX.SharedKernel.Tests;

public class ResultTests
{
    [Fact]
    public void success_result_has_no_error()
    {
        var result = Result.Success();

        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void failure_result_carries_error()
    {
        var error = new Error("test.code", "Test message");

        var result = Result.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void generic_success_result_exposes_value()
    {
        var result = Result<int>.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void generic_failure_result_has_no_value()
    {
        var error = new Error("test.code", "Test message");

        var result = Result<string>.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.Value.Should().BeNull();
        result.Error.Should().Be(error);
    }
}
