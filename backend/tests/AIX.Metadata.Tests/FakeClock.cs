using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Tests;

internal sealed class FakeClock : IClock
{
    public DateTimeOffset UtcNow { get; set; } = new(2026, 5, 17, 12, 0, 0, TimeSpan.Zero);
}
