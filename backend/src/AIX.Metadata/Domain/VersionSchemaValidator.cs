using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

public static class VersionSchemaValidator
{
    public static SchemaValidationResult Validate(
        KeywordRegistry registry,
        DocumentTypeVersion version,
        VersionMetadataPayload payload)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(version);
        ArgumentNullException.ThrowIfNull(payload);

        var composition = version.SchemaComposition;
        var keywordsById = registry.Keywords.ToDictionary(keyword => keyword.Id);
        var keywordCodeById = registry.Keywords.ToDictionary(
            keyword => keyword.Id,
            keyword => keyword.Code,
            EqualityComparer<KeywordId>.Default);

        var standaloneFieldsByKeywordCode = BuildStandaloneFieldMap(composition, keywordCodeById);
        var groupKeywordCodes = BuildGroupKeywordCodeSet(composition, keywordCodeById);
        var assignmentsByKey = BuildAssignmentMap(composition);
        var assignments = composition.GroupAssignments;

        var errors = new List<Error>();

        ValidateStandalonePayloadKeys(
            payload,
            standaloneFieldsByKeywordCode,
            groupKeywordCodes,
            errors);

        ValidateGroupPayloadStructure(payload, assignmentsByKey, keywordCodeById, errors);

        foreach (var field in composition.Fields)
        {
            if (!keywordCodeById.TryGetValue(field.KeywordId, out var keywordCode))
            {
                continue;
            }

            payload.StandaloneValues.TryGetValue(keywordCode, out var value);
            ValidateFieldCapture(
                keywordsById,
                field,
                value,
                payload.StandaloneValues.ContainsKey(keywordCode),
                errors);
        }

        foreach (var assignment in assignments)
        {
            var instanceKey = NormalizeInstanceKey(assignment.InstanceKey);
            var payloadInstance = FindGroupInstance(payload, assignment.GroupCode, instanceKey);

            if (payloadInstance is null)
            {
                if (assignment.IsRequired)
                {
                    errors.Add(SchemaValidationErrors.MissingRequiredGroupInstance);
                }

                continue;
            }

            foreach (var keywordId in assignment.KeywordIds)
            {
                if (!keywordCodeById.TryGetValue(keywordId, out var keywordCode))
                {
                    continue;
                }

                payloadInstance.Values.TryGetValue(keywordCode, out var value);
                ValidateKeywordValue(
                    keywordsById,
                    keywordId,
                    value,
                    assignment.IsRequired,
                    errors);
            }
        }

        return errors.Count == 0
            ? SchemaValidationResult.Valid()
            : SchemaValidationResult.Invalid(OrderErrorsDeterministically(errors));
    }

    private static Dictionary<string, FieldSchema> BuildStandaloneFieldMap(
        VersionSchemaComposition composition,
        IReadOnlyDictionary<KeywordId, string> keywordCodeById)
    {
        var map = new Dictionary<string, FieldSchema>(StringComparer.OrdinalIgnoreCase);
        foreach (var field in composition.Fields)
        {
            if (keywordCodeById.TryGetValue(field.KeywordId, out var keywordCode))
            {
                map[keywordCode] = field;
            }
        }

        return map;
    }

    private static HashSet<string> BuildGroupKeywordCodeSet(
        VersionSchemaComposition composition,
        IReadOnlyDictionary<KeywordId, string> keywordCodeById)
    {
        var codes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var keywordId in composition.GroupAssignments.SelectMany(assignment => assignment.KeywordIds))
        {
            if (keywordCodeById.TryGetValue(keywordId, out var keywordCode))
            {
                codes.Add(keywordCode);
            }
        }

        return codes;
    }

    private static Dictionary<GroupInstanceKey, VersionSchemaGroupAssignment> BuildAssignmentMap(
        VersionSchemaComposition composition)
    {
        var map = new Dictionary<GroupInstanceKey, VersionSchemaGroupAssignment>();
        foreach (var assignment in composition.GroupAssignments)
        {
            map[GroupInstanceKey.FromAssignment(assignment)] = assignment;
        }

        return map;
    }

    private static MetadataGroupInstancePayload? FindGroupInstance(
        VersionMetadataPayload payload,
        string groupCode,
        string instanceKey)
    {
        var expected = new GroupInstanceKey(GroupInstanceKey.NormalizeGroupCode(groupCode), instanceKey);
        return payload.GroupInstances.FirstOrDefault(instance => GroupInstanceKey.FromPayload(instance) == expected);
    }

    private static void ValidateStandalonePayloadKeys(
        VersionMetadataPayload payload,
        IReadOnlyDictionary<string, FieldSchema> standaloneFieldsByKeywordCode,
        HashSet<string> groupKeywordCodes,
        List<Error> errors)
    {
        foreach (var (keywordCode, _) in payload.StandaloneValues)
        {
            if (groupKeywordCodes.Contains(keywordCode))
            {
                errors.Add(SchemaValidationErrors.GroupKeywordMustUseGroupPayload);
                continue;
            }

            if (!standaloneFieldsByKeywordCode.ContainsKey(keywordCode))
            {
                errors.Add(SchemaValidationErrors.UnknownMetadataKey);
            }
        }
    }

    private static void ValidateGroupPayloadStructure(
        VersionMetadataPayload payload,
        IReadOnlyDictionary<GroupInstanceKey, VersionSchemaGroupAssignment> assignmentsByKey,
        IReadOnlyDictionary<KeywordId, string> keywordCodeById,
        List<Error> errors)
    {
        var seenInstances = new HashSet<GroupInstanceKey>();
        foreach (var instance in payload.GroupInstances)
        {
            var key = GroupInstanceKey.FromPayload(instance);

            if (!seenInstances.Add(key))
            {
                errors.Add(SchemaValidationErrors.DuplicateGroupInstanceInPayload);
                continue;
            }

            if (!assignmentsByKey.TryGetValue(key, out var assignment))
            {
                errors.Add(SchemaValidationErrors.UnexpectedGroupInstance);
                continue;
            }

            var allowedKeywordCodes = assignment.KeywordIds
                .Where(keywordCodeById.ContainsKey)
                .Select(keywordId => keywordCodeById[keywordId])
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var keywordCode in instance.Values.Keys)
            {
                if (!allowedKeywordCodes.Contains(keywordCode))
                {
                    errors.Add(SchemaValidationErrors.UnknownMetadataKey);
                }
            }
        }
    }

    private static void ValidateFieldCapture(
        IReadOnlyDictionary<KeywordId, Keyword> keywordsById,
        FieldSchema field,
        string? value,
        bool valueProvided,
        List<Error> errors)
    {
        if (field.IsHidden && valueProvided)
        {
            errors.Add(SchemaValidationErrors.HiddenFieldNotAllowedOnCapture);
            return;
        }

        if (field.IsDeprecated && valueProvided)
        {
            errors.Add(SchemaValidationErrors.DeprecatedFieldNotAllowedOnCapture);
            return;
        }

        if (!field.IsActive)
        {
            return;
        }

        ValidateKeywordValue(keywordsById, field.KeywordId, value, field.IsRequired, errors);
    }

    private static void ValidateKeywordValue(
        IReadOnlyDictionary<KeywordId, Keyword> keywordsById,
        KeywordId keywordId,
        string? value,
        bool isRequired,
        List<Error> errors)
    {
        if (!keywordsById.TryGetValue(keywordId, out var keyword))
        {
            return;
        }

        var validation = KeywordValidator.ValidateValue(keyword, value, isRequired);
        if (!validation.IsValid)
        {
            errors.AddRange(validation.Errors);
        }
    }

    private static string NormalizeInstanceKey(string? instanceKey) =>
        GroupInstanceKey.NormalizeInstanceKey(instanceKey);

    private static IReadOnlyList<Error> OrderErrorsDeterministically(IReadOnlyList<Error> errors) =>
        errors
            .OrderBy(error => error.Code, StringComparer.Ordinal)
            .ThenBy(error => error.Message, StringComparer.Ordinal)
            .ToList();

    private readonly record struct GroupInstanceKey(string GroupCode, string InstanceKey)
    {
        public static GroupInstanceKey FromAssignment(VersionSchemaGroupAssignment assignment) =>
            new(NormalizeGroupCode(assignment.GroupCode), NormalizeInstanceKey(assignment.InstanceKey));

        public static GroupInstanceKey FromPayload(MetadataGroupInstancePayload instance) =>
            new(NormalizeGroupCode(instance.GroupCode), NormalizeInstanceKey(instance.InstanceKey));

        public static string NormalizeGroupCode(string groupCode) =>
            groupCode.Trim().ToUpperInvariant();

        public static string NormalizeInstanceKey(string? instanceKey) =>
            string.IsNullOrWhiteSpace(instanceKey) ? string.Empty : instanceKey.Trim();
    }
}
