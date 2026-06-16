using AIX.SharedKernel.Primitives;
using FluentAssertions;

namespace AIX.SharedKernel.Tests;

public class DomainEventTests
{
    private sealed record TestEvent(
        Guid EventId,
        DateTimeOffset OccurredOn,
        CorrelationId CorrelationId,
        string Payload) : DomainEvent(EventId, OccurredOn, CorrelationId);

    [Fact]
    public void correlation_id_new_generates_non_empty_guid()
    {
        var correlationId = CorrelationId.New();

        correlationId.Value.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void domain_event_carries_metadata()
    {
        var eventId = Guid.NewGuid();
        var occurredOn = new DateTimeOffset(2026, 5, 16, 12, 0, 0, TimeSpan.Zero);
        var correlationId = CorrelationId.New();

        var domainEvent = new TestEvent(eventId, occurredOn, correlationId, "payload");

        domainEvent.EventId.Should().Be(eventId);
        domainEvent.OccurredOn.Should().Be(occurredOn);
        domainEvent.CorrelationId.Should().Be(correlationId);
        domainEvent.Payload.Should().Be("payload");
    }
}
