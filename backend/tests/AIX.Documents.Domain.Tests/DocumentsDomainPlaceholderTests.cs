using AIX.Documents.Domain;
using FluentAssertions;

namespace AIX.Documents.Domain.Tests;

public class DocumentsDomainPlaceholderTests
{
    [Fact]
    public void Documents_domain_assembly_is_available()
    {
        typeof(AssemblyReference).Assembly.GetName().Name
            .Should()
            .Be("AIX.Documents.Domain");
    }
}
