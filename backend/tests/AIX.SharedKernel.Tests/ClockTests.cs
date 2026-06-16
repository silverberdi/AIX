using AIX.SharedKernel.Primitives;
using FluentAssertions;

namespace AIX.SharedKernel.Tests;

public class ClockTests
{
    [Fact]
    public void system_clock_returns_utc_now_near_current_time()
    {
        var before = DateTimeOffset.UtcNow;
        var clock = new SystemClock();
        var after = DateTimeOffset.UtcNow;

        clock.UtcNow.Should().BeCloseTo(before, TimeSpan.FromSeconds(1));
        clock.UtcNow.Should().BeCloseTo(after, TimeSpan.FromSeconds(1));
    }
}
