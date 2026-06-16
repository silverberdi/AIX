using AIX.SharedKernel.Primitives;
using FluentAssertions;

namespace AIX.SharedKernel.Tests;

public class ValueObjectTests
{
    private sealed class Money : ValueObject
    {
        public int Amount { get; }
        public string Currency { get; }

        public Money(int amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }
    }

    [Fact]
    public void equal_components_imply_equality()
    {
        var left = new Money(10, "USD");
        var right = new Money(10, "USD");

        left.Equals(right).Should().BeTrue();
        left.GetHashCode().Should().Be(right.GetHashCode());
    }

    [Fact]
    public void different_components_are_not_equal()
    {
        var left = new Money(10, "USD");
        var right = new Money(20, "USD");

        left.Equals(right).Should().BeFalse();
    }
}
