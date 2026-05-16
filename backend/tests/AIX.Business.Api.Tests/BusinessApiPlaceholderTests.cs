using FluentAssertions;

namespace AIX.Business.Api.Tests;

public class BusinessApiPlaceholderTests
{
    [Fact]
    public void Business_api_assembly_is_available()
    {
        typeof(AIX.Business.Api.WeatherForecast).Assembly.GetName().Name
            .Should()
            .Be("AIX.Business.Api");
    }
}
