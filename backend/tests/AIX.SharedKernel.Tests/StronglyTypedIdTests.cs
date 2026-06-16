using AIX.SharedKernel.Primitives;
using FluentAssertions;

namespace AIX.SharedKernel.Tests;

public class StronglyTypedIdTests
{
    private sealed record TestId(Guid Value) : StronglyTypedId<Guid>(Value);

    [Fact]
    public void exposes_wrapped_value()
    {
        var id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        var testId = new TestId(id);

        testId.Value.Should().Be(id);
    }

    [Fact]
    public void record_derived_ids_remain_distinct_by_value()
    {
        var id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");

        var first = new TestId(id);
        var second = new TestId(id);

        first.Should().Be(second);
        ReferenceEquals(first, second).Should().BeFalse();
    }
}
