using AIX.Security.Domain;
using FluentAssertions;

namespace AIX.Security.Domain.Tests;

public class SecurityDomainPlaceholderTests
{
    [Fact]
    public void Security_domain_assembly_is_available()
    {
        typeof(AssemblyReference).Assembly.GetName().Name
            .Should()
            .Be("AIX.Security.Domain");
    }
}
