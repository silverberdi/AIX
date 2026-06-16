using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

public sealed class VersionSchemaComposition
{
    public IReadOnlyList<FieldSchema> Fields { get; }
    public IReadOnlyList<VersionSchemaGroupAssignment> GroupAssignments { get; }
    internal IReadOnlyList<VersionSchemaFieldSnapshot> FieldSnapshots { get; }
    internal IReadOnlyList<VersionSchemaGroupSnapshot> GroupSnapshots { get; }

    private VersionSchemaComposition(
        IReadOnlyList<FieldSchema> fields,
        IReadOnlyList<VersionSchemaFieldSnapshot> fieldSnapshots,
        IReadOnlyList<VersionSchemaGroupAssignment> groupAssignments,
        IReadOnlyList<VersionSchemaGroupSnapshot> groupSnapshots)
    {
        Fields = fields;
        FieldSnapshots = fieldSnapshots;
        GroupAssignments = groupAssignments;
        GroupSnapshots = groupSnapshots;
    }

    public static VersionSchemaComposition Empty { get; } = new([], [], [], []);

    public static Result<VersionSchemaComposition> FromDefinitions(
        KeywordRegistry registry,
        IReadOnlyList<VersionSchemaFieldDefinition> definitions) =>
        FromDefinitions(registry, keywordGroups: [], definitions, groupDefinitions: []);

    public static Result<VersionSchemaComposition> FromDefinitions(
        KeywordRegistry registry,
        IReadOnlyList<KeywordGroup> keywordGroups,
        IReadOnlyList<VersionSchemaFieldDefinition> definitions,
        IReadOnlyList<VersionSchemaGroupAssignmentDefinition> groupDefinitions)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(keywordGroups);
        ArgumentNullException.ThrowIfNull(definitions);
        ArgumentNullException.ThrowIfNull(groupDefinitions);

        var fieldsResult = BuildFields(registry, definitions);
        if (!fieldsResult.IsSuccess)
        {
            return Result<VersionSchemaComposition>.Failure(fieldsResult.Error.Value);
        }

        var groupsResult = BuildGroupAssignments(registry, keywordGroups, groupDefinitions);
        if (!groupsResult.IsSuccess)
        {
            return Result<VersionSchemaComposition>.Failure(groupsResult.Error.Value);
        }

        var (fields, fieldSnapshots) = fieldsResult.Value!;
        var (groupAssignments, groupSnapshots) = groupsResult.Value!;

        if (fields.Count == 0 && groupAssignments.Count == 0)
        {
            return Result<VersionSchemaComposition>.Success(Empty);
        }

        var uniquenessResult = ValidateUniqueFieldRules(fields);
        if (!uniquenessResult.IsSuccess)
        {
            return Result<VersionSchemaComposition>.Failure(uniquenessResult.Error.Value);
        }

        var overlapResult = ValidateNoKeywordOverlap(fields, groupAssignments);
        if (!overlapResult.IsSuccess)
        {
            return Result<VersionSchemaComposition>.Failure(overlapResult.Error.Value);
        }

        return Result<VersionSchemaComposition>.Success(
            new VersionSchemaComposition(fields, fieldSnapshots, groupAssignments, groupSnapshots));
    }

    public static Result<VersionSchemaComposition> FromFields(
        KeywordRegistry registry,
        IReadOnlyList<FieldSchema> fields)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(fields);

        if (fields.Count == 0)
        {
            return Result<VersionSchemaComposition>.Success(Empty);
        }

        var snapshots = new List<VersionSchemaFieldSnapshot>(fields.Count);
        foreach (var field in fields)
        {
            var keyword = registry.Keywords.FirstOrDefault(existing => existing.Id == field.KeywordId);
            if (keyword is null)
            {
                return Result<VersionSchemaComposition>.Failure(FieldSchemaErrors.KeywordNotRegistered);
            }

            snapshots.Add(new VersionSchemaFieldSnapshot(
                field.Id,
                field.KeywordId,
                keyword.Code,
                field.CatalogType));
        }

        var uniquenessResult = ValidateUniqueFieldRules(fields);
        if (!uniquenessResult.IsSuccess)
        {
            return Result<VersionSchemaComposition>.Failure(uniquenessResult.Error.Value);
        }

        return Result<VersionSchemaComposition>.Success(
            new VersionSchemaComposition(fields, snapshots, [], []));
    }

    private static Result<(List<FieldSchema> Fields, List<VersionSchemaFieldSnapshot> Snapshots)> BuildFields(
        KeywordRegistry registry,
        IReadOnlyList<VersionSchemaFieldDefinition> definitions)
    {
        if (definitions.Count == 0)
        {
            return Result<(List<FieldSchema>, List<VersionSchemaFieldSnapshot>)>.Success(([], []));
        }

        var fields = new List<FieldSchema>(definitions.Count);
        var snapshots = new List<VersionSchemaFieldSnapshot>(definitions.Count);

        foreach (var definition in definitions)
        {
            var fieldResult = FieldSchema.Create(
                definition.KeywordId,
                definition.CatalogType,
                registry,
                definition.LabelOverride,
                definition.HelpText,
                definition.OrderHint,
                definition.IsDeprecated,
                definition.IsHidden,
                definition.IsRequired);

            if (!fieldResult.IsSuccess)
            {
                return Result<(List<FieldSchema>, List<VersionSchemaFieldSnapshot>)>.Failure(fieldResult.Error.Value);
            }

            var field = fieldResult.Value!;
            var keyword = registry.Keywords.Single(existing => existing.Id == definition.KeywordId);
            fields.Add(field);
            snapshots.Add(new VersionSchemaFieldSnapshot(
                field.Id,
                field.KeywordId,
                keyword.Code,
                field.CatalogType));
        }

        return Result<(List<FieldSchema>, List<VersionSchemaFieldSnapshot>)>.Success((fields, snapshots));
    }

    private static Result<(List<VersionSchemaGroupAssignment> Assignments, List<VersionSchemaGroupSnapshot> Snapshots)>
        BuildGroupAssignments(
            KeywordRegistry registry,
            IReadOnlyList<KeywordGroup> keywordGroups,
            IReadOnlyList<VersionSchemaGroupAssignmentDefinition> groupDefinitions)
    {
        if (groupDefinitions.Count == 0)
        {
            return Result<(List<VersionSchemaGroupAssignment>, List<VersionSchemaGroupSnapshot>)>.Success(([], []));
        }

        var groupsById = keywordGroups.ToDictionary(group => group.Id);
        var registeredKeywordIds = registry.Keywords.Select(keyword => keyword.Id).ToHashSet();
        var assignments = new List<VersionSchemaGroupAssignment>(groupDefinitions.Count);
        var snapshots = new List<VersionSchemaGroupSnapshot>(groupDefinitions.Count);

        foreach (var definition in groupDefinitions)
        {
            if (!groupsById.TryGetValue(definition.KeywordGroupId, out var group))
            {
                return Result<(List<VersionSchemaGroupAssignment>, List<VersionSchemaGroupSnapshot>)>.Failure(
                    VersionSchemaCompositionErrors.KeywordGroupNotFound);
            }

            if (group.KeywordIds.Any(keywordId => !registeredKeywordIds.Contains(keywordId)))
            {
                return Result<(List<VersionSchemaGroupAssignment>, List<VersionSchemaGroupSnapshot>)>.Failure(
                    KeywordGroupErrors.KeywordNotRegistered);
            }

            var instanceKey = NormalizeInstanceKey(definition.InstanceKey);
            var assignment = new VersionSchemaGroupAssignment(
                group.Id,
                group.Code,
                definition.Order,
                instanceKey,
                group.IsRepeatable,
                group.IsRequired,
                group.KeywordIds.ToList());

            assignments.Add(assignment);
            snapshots.Add(new VersionSchemaGroupSnapshot(
                assignment.KeywordGroupId,
                assignment.GroupCode,
                assignment.Order,
                assignment.InstanceKey,
                assignment.IsRepeatable,
                assignment.IsRequired,
                assignment.KeywordIds));
        }

        var placementResult = ValidateGroupPlacements(assignments);
        if (!placementResult.IsSuccess)
        {
            return Result<(List<VersionSchemaGroupAssignment>, List<VersionSchemaGroupSnapshot>)>.Failure(
                placementResult.Error.Value);
        }

        return Result<(List<VersionSchemaGroupAssignment>, List<VersionSchemaGroupSnapshot>)>.Success(
            (assignments, snapshots));
    }

    private static Result ValidateGroupPlacements(IReadOnlyList<VersionSchemaGroupAssignment> assignments)
    {
        foreach (var group in assignments.GroupBy(assignment => assignment.KeywordGroupId))
        {
            var first = group.First();
            if (!first.IsRepeatable && group.Count() > 1)
            {
                return Result.Failure(VersionSchemaCompositionErrors.DuplicateGroupPlacement);
            }

            if (group.Select(assignment => assignment.InstanceKey).Distinct().Count() != group.Count())
            {
                return Result.Failure(VersionSchemaCompositionErrors.DuplicateGroupInstanceKey);
            }
        }

        return Result.Success();
    }

    private static Result ValidateNoKeywordOverlap(
        IReadOnlyList<FieldSchema> fields,
        IReadOnlyList<VersionSchemaGroupAssignment> groupAssignments)
    {
        if (fields.Count == 0 || groupAssignments.Count == 0)
        {
            return Result.Success();
        }

        var standaloneKeywordIds = fields.Select(field => field.KeywordId).ToHashSet();
        var groupKeywordIds = groupAssignments.SelectMany(assignment => assignment.KeywordIds).ToHashSet();

        if (standaloneKeywordIds.Overlaps(groupKeywordIds))
        {
            return Result.Failure(VersionSchemaCompositionErrors.KeywordInGroupAndStandaloneField);
        }

        return Result.Success();
    }

    private static string NormalizeInstanceKey(string? instanceKey) =>
        string.IsNullOrWhiteSpace(instanceKey) ? string.Empty : instanceKey.Trim();

    private static Result ValidateUniqueFieldRules(IReadOnlyList<FieldSchema> fields)
    {
        if (fields.Select(field => field.Id).Distinct().Count() != fields.Count)
        {
            return Result.Failure(VersionSchemaCompositionErrors.DuplicateFieldSchemaId);
        }

        if (fields.Select(field => field.KeywordId).Distinct().Count() != fields.Count)
        {
            return Result.Failure(VersionSchemaCompositionErrors.DuplicateKeyword);
        }

        return Result.Success();
    }
}
