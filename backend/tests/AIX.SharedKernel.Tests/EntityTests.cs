using AIX.SharedKernel.Primitives;
using FluentAssertions;

namespace AIX.SharedKernel.Tests;

public class EntityTests
{
    private sealed class TestEntity : Entity<Guid>
    {
        public TestEntity(Guid id) => Id = id;
    }

    [Fact]
    public void exposes_identity()
    {
        var id = Guid.NewGuid();

        var entity = new TestEntity(id);

        entity.Id.Should().Be(id);
    }
}
