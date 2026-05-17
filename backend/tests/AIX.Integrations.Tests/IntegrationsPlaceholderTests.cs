using AIX.Integrations;
using FluentAssertions;

namespace AIX.Integrations.Tests;

public class IntegrationsPlaceholderTests
{
    [Fact]
    public void Module_assembly_is_available()
    {
        typeof(AssemblyReference).Assembly.GetName().Name.Should().Be("AIX.Integrations");
    }
}
