using AIX.Governance;
using FluentAssertions;

namespace AIX.Governance.Tests;

public class GovernancePlaceholderTests
{
    [Fact]
    public void Module_assembly_is_available()
    {
        typeof(AssemblyReference).Assembly.GetName().Name.Should().Be("AIX.Governance");
    }
}
