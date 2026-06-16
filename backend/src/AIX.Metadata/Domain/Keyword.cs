using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

public sealed class Keyword : Entity<KeywordId>
{
    public string Code { get; private init; } = string.Empty;
    public string Name { get; private init; } = string.Empty;
    public KeywordDataType DataType { get; private init; }
    public int? MaxLength { get; private init; }
    public DateTimeOffset CreatedAt { get; private init; }

    private Keyword(
        KeywordId id,
        string code,
        string name,
        KeywordDataType dataType,
        int? maxLength,
        DateTimeOffset createdAt)
    {
        Id = id;
        Code = code;
        Name = name;
        DataType = dataType;
        MaxLength = maxLength;
        CreatedAt = createdAt;
    }

    internal static Result<Keyword> Create(
        string code,
        string name,
        KeywordDataType dataType,
        int? maxLength,
        DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return Result<Keyword>.Failure(KeywordErrors.CodeRequired);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<Keyword>.Failure(KeywordErrors.NameRequired);
        }

        if (maxLength is <= 0)
        {
            return Result<Keyword>.Failure(KeywordErrors.MaxLengthInvalid);
        }

        if (maxLength is not null && dataType != KeywordDataType.Text)
        {
            return Result<Keyword>.Failure(KeywordErrors.MaxLengthNotAllowed);
        }

        var keyword = new Keyword(
            KeywordId.New(),
            code.Trim(),
            name.Trim(),
            dataType,
            maxLength,
            createdAt);

        return Result<Keyword>.Success(keyword);
    }
}
