using AIX.ReferenceData;
using FluentAssertions;

namespace AIX.ReferenceData.Tests;

public class ReferenceDataPlaceholderTests
{
    [Fact]
    public void Module_assembly_is_available()
    {
        typeof(AssemblyReference).Assembly.GetName().Name.Should().Be("AIX.ReferenceData");
    }
}
