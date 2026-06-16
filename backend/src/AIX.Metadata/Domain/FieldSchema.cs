using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

public sealed class FieldSchema
{
    public FieldSchemaId Id { get; private init; }
    public KeywordId KeywordId { get; private init; }
    public FieldCatalogType CatalogType { get; private init; }
    public string? LabelOverride { get; private init; }
    public string? HelpText { get; private init; }
    public int? OrderHint { get; private init; }
    public bool IsDeprecated { get; private init; }
    public bool IsHidden { get; private init; }
    public bool IsRequired { get; private init; }
    public bool IsActive => !IsDeprecated && !IsHidden;

    private FieldSchema(
        FieldSchemaId id,
        KeywordId keywordId,
        FieldCatalogType catalogType,
        string? labelOverride,
        string? helpText,
        int? orderHint,
        bool isDeprecated,
        bool isHidden,
        bool isRequired)
    {
        Id = id;
        KeywordId = keywordId;
        CatalogType = catalogType;
        LabelOverride = labelOverride;
        HelpText = helpText;
        OrderHint = orderHint;
        IsDeprecated = isDeprecated;
        IsHidden = isHidden;
        IsRequired = isRequired;
    }

    public static Result<FieldSchema> Create(
        KeywordId keywordId,
        FieldCatalogType catalogType,
        KeywordRegistry registry,
        string? labelOverride = null,
        string? helpText = null,
        int? orderHint = null,
        bool isDeprecated = false,
        bool isHidden = false,
        bool isRequired = false)
    {
        ArgumentNullException.ThrowIfNull(registry);

        if (orderHint is < 0)
        {
            return Result<FieldSchema>.Failure(FieldSchemaErrors.OrderHintInvalid);
        }

        var keyword = registry.Keywords.FirstOrDefault(existing => existing.Id == keywordId);
        if (keyword is null)
        {
            return Result<FieldSchema>.Failure(FieldSchemaErrors.KeywordNotRegistered);
        }

        if (!FieldCatalogTypeCompatibility.IsCompatible(keyword.DataType, catalogType))
        {
            return Result<FieldSchema>.Failure(FieldSchemaErrors.IncompatibleCatalogType);
        }

        var field = new FieldSchema(
            FieldSchemaId.New(),
            keywordId,
            catalogType,
            NormalizeOptionalText(labelOverride),
            NormalizeOptionalText(helpText),
            orderHint,
            isDeprecated,
            isHidden,
            isRequired);

        return Result<FieldSchema>.Success(field);
    }

    public static IReadOnlyList<FieldSchema> SelectActive(IEnumerable<FieldSchema> fields) =>
        fields.Where(field => field.IsActive).ToList();

    private static string? NormalizeOptionalText(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
