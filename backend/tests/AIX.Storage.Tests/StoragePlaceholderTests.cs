using AIX.Storage;
using FluentAssertions;

namespace AIX.Storage.Tests;

public class StoragePlaceholderTests
{
    [Fact]
    public void Module_assembly_is_available()
    {
        typeof(AssemblyReference).Assembly.GetName().Name.Should().Be("AIX.Storage");
    }
}
