using AIX.Security;
using FluentAssertions;

namespace AIX.Security.Tests;

public class SecurityPlaceholderTests
{
    [Fact]
    public void Module_assembly_is_available()
    {
        typeof(AssemblyReference).Assembly.GetName().Name.Should().Be("AIX.Security");
    }
}
