using FluentAssertions;

namespace AIX.Platform.Api.Tests;

public class PlatformApiPlaceholderTests
{
    [Fact]
    public void Platform_api_assembly_is_available()
    {
        typeof(AIX.Platform.Api.WeatherForecast).Assembly.GetName().Name
            .Should()
            .Be("AIX.Platform.Api");
    }
}
