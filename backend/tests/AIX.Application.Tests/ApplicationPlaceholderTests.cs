using AIX.Application;
using FluentAssertions;
using NSubstitute;

namespace AIX.Application.Tests;

public class ApplicationPlaceholderTests
{
    [Fact]
    public void Application_assembly_is_available()
    {
        typeof(AssemblyReference).Assembly.GetName().Name.Should().Be("AIX.Application");
    }

    [Fact]
    public void NSubstitute_can_create_substitutes()
    {
        var substitute = Substitute.For<IDisposable>();
        substitute.Should().NotBeNull();
    }
}
