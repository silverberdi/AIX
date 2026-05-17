using AIX.Workflow;
using FluentAssertions;

namespace AIX.Workflow.Tests;

public class WorkflowPlaceholderTests
{
    [Fact]
    public void Module_assembly_is_available()
    {
        typeof(AssemblyReference).Assembly.GetName().Name.Should().Be("AIX.Workflow");
    }
}
