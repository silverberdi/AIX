using AIX.Metadata;
using FluentAssertions;

namespace AIX.Metadata.Tests;

public class MetadataPlaceholderTests
{
    [Fact]
    public void Module_assembly_is_available()
    {
        typeof(AssemblyReference).Assembly.GetName().Name.Should().Be("AIX.Metadata");
    }
}
