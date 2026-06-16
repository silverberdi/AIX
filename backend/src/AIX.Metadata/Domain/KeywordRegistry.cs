using AIX.Metadata.Events;
using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

public sealed class KeywordRegistry : Entity<KeywordRegistryId>
{
    private readonly List<Keyword> _keywords = [];
    private readonly List<DomainEvent> _domainEvents = [];

    public IReadOnlyList<Keyword> Keywords => _keywords;
    public IReadOnlyList<DomainEvent> DomainEvents => _domainEvents;

    private KeywordRegistry(KeywordRegistryId id)
    {
        Id = id;
    }

    public static Result<KeywordRegistry> Create(IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        return Result<KeywordRegistry>.Success(new KeywordRegistry(KeywordRegistryId.New()));
    }

    public Result<Keyword> Register(
        string code,
        string name,
        KeywordDataType dataType,
        int? maxLength,
        IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);

        var keywordResult = Keyword.Create(code, name, dataType, maxLength, clock.UtcNow);
        if (!keywordResult.IsSuccess)
        {
            return keywordResult;
        }

        var keyword = keywordResult.Value!;

        if (_keywords.Any(existing =>
                string.Equals(existing.Code, keyword.Code, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<Keyword>.Failure(KeywordErrors.DuplicateCode);
        }

        if (_keywords.Any(existing =>
                string.Equals(existing.Name, keyword.Name, StringComparison.OrdinalIgnoreCase)))
        {
            return Result<Keyword>.Failure(KeywordErrors.DuplicateName);
        }

        _keywords.Add(keyword);

        _domainEvents.Add(new KeywordRegistered(
            Guid.NewGuid(),
            clock.UtcNow,
            CorrelationId.New(),
            Id,
            keyword.Id,
            keyword.Code,
            keyword.Name,
            keyword.DataType,
            keyword.MaxLength));

        return Result<Keyword>.Success(keyword);
    }

    public Result ValidateKeywordValue(KeywordId keywordId, string? value, bool isRequired)
    {
        var keyword = _keywords.FirstOrDefault(existing => existing.Id == keywordId);
        if (keyword is null)
        {
            return Result.Failure(KeywordValidationErrors.KeywordNotFound);
        }

        var validation = KeywordValidator.ValidateValue(keyword, value, isRequired);
        if (validation.IsValid)
        {
            return Result.Success();
        }

        return Result.Failure(validation.Errors[0]);
    }
}
