using AIX.Platform;
using FluentAssertions;

namespace AIX.Platform.Tests;

public class PlatformPlaceholderTests
{
    [Fact]
    public void Module_assembly_is_available()
    {
        typeof(AssemblyReference).Assembly.GetName().Name.Should().Be("AIX.Platform");
    }
}
