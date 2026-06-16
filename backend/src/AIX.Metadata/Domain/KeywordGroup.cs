using AIX.Metadata.Events;
using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

public sealed class KeywordGroup : Entity<KeywordGroupId>
{
    private readonly List<KeywordId> _keywordIds = [];
    private readonly List<DomainEvent> _domainEvents = [];

    public string Code { get; private init; } = string.Empty;
    public string Name { get; private init; } = string.Empty;
    public bool IsRepeatable { get; private init; }
    public bool IsRequired { get; private init; }
    public DateTimeOffset CreatedAt { get; private init; }
    public IReadOnlyList<KeywordId> KeywordIds => _keywordIds;
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents;

    private KeywordGroup(
        KeywordGroupId id,
        string code,
        string name,
        bool isRepeatable,
        bool isRequired,
        IReadOnlyList<KeywordId> keywordIds,
        DateTimeOffset createdAt)
    {
        Id = id;
        Code = code;
        Name = name;
        IsRepeatable = isRepeatable;
        IsRequired = isRequired;
        CreatedAt = createdAt;
        _keywordIds.AddRange(keywordIds);
    }

    public static Result<KeywordGroup> Create(
        string code,
        string name,
        bool isRepeatable,
        bool isRequired,
        IReadOnlyList<KeywordId> keywordIds,
        KeywordRegistry registry,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(clock);

        if (string.IsNullOrWhiteSpace(code))
        {
            return Result<KeywordGroup>.Failure(KeywordGroupErrors.CodeRequired);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<KeywordGroup>.Failure(KeywordGroupErrors.NameRequired);
        }

        if (keywordIds is null || keywordIds.Count == 0)
        {
            return Result<KeywordGroup>.Failure(KeywordGroupErrors.KeywordsRequired);
        }

        if (keywordIds.GroupBy(id => id.Value).Any(group => group.Count() > 1))
        {
            return Result<KeywordGroup>.Failure(KeywordGroupErrors.DuplicateKeyword);
        }

        var registeredIds = registry.Keywords.Select(keyword => keyword.Id).ToHashSet();
        if (keywordIds.Any(id => !registeredIds.Contains(id)))
        {
            return Result<KeywordGroup>.Failure(KeywordGroupErrors.KeywordNotRegistered);
        }

        var createdAt = clock.UtcNow;
        var groupId = KeywordGroupId.New();
        var normalizedCode = code.Trim();
        var normalizedName = name.Trim();
        var group = new KeywordGroup(
            groupId,
            normalizedCode,
            normalizedName,
            isRepeatable,
            isRequired,
            keywordIds,
            createdAt);

        group._domainEvents.Add(new KeywordGroupCreated(
            Guid.NewGuid(),
            createdAt,
            CorrelationId.New(),
            groupId,
            normalizedCode,
            normalizedName,
            isRepeatable,
            isRequired,
            keywordIds));

        return Result<KeywordGroup>.Success(group);
    }
}
