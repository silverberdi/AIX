using AIX.Search;
using FluentAssertions;

namespace AIX.Search.Tests;

public class SearchPlaceholderTests
{
    [Fact]
    public void Module_assembly_is_available()
    {
        typeof(AssemblyReference).Assembly.GetName().Name.Should().Be("AIX.Search");
    }
}
