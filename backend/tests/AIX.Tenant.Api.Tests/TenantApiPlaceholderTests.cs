using FluentAssertions;

namespace AIX.Tenant.Api.Tests;

public class TenantApiPlaceholderTests
{
    [Fact]
    public void Tenant_api_assembly_is_available()
    {
        typeof(AIX.Tenant.Api.WeatherForecast).Assembly.GetName().Name
            .Should()
            .Be("AIX.Tenant.Api");
    }
}
