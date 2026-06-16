using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

public sealed class LayoutSchema
{
    public IReadOnlyList<LayoutSection> Sections { get; }
    internal IReadOnlyList<LayoutSectionSnapshot> SectionSnapshots { get; }

    private LayoutSchema(
        IReadOnlyList<LayoutSection> sections,
        IReadOnlyList<LayoutSectionSnapshot> sectionSnapshots)
    {
        Sections = sections;
        SectionSnapshots = sectionSnapshots;
    }

    public static LayoutSchema Empty { get; } = new([], []);

    internal static LayoutSchema FromSections(IReadOnlyList<LayoutSection> sections)
    {
        var snapshots = sections.Select(BuildSectionSnapshot).ToList();
        return new LayoutSchema(sections.ToList(), snapshots);
    }

    public static Result<LayoutSchema> CreateDefault(VersionSchemaComposition composition) =>
        DefaultLayoutGenerator.Generate(composition);

    public static Result<LayoutSchema> FromDefinitions(
        VersionSchemaComposition composition,
        IReadOnlyList<LayoutSectionDefinition> sectionDefinitions)
    {
        ArgumentNullException.ThrowIfNull(composition);
        ArgumentNullException.ThrowIfNull(sectionDefinitions);

        if (sectionDefinitions.Count == 0)
        {
            return CreateDefault(composition);
        }

        var fieldsByKeywordId = composition.Fields.ToDictionary(field => field.KeywordId);
        var groupsByKey = composition.GroupAssignments.ToDictionary(
            assignment => (assignment.KeywordGroupId, assignment.InstanceKey));

        var placedFieldIds = new HashSet<FieldSchemaId>();
        var placedGroupKeys = new HashSet<(KeywordGroupId GroupId, string InstanceKey)>();
        var sectionIds = new HashSet<LayoutSectionId>();
        var sections = new List<LayoutSection>(sectionDefinitions.Count);

        foreach (var definition in sectionDefinitions.OrderBy(section => section.Order))
        {
            if (string.IsNullOrWhiteSpace(definition.Title))
            {
                return Result<LayoutSchema>.Failure(LayoutSchemaErrors.SectionTitleRequired);
            }

            var sectionId = definition.Id ?? LayoutSectionId.New();
            if (!sectionIds.Add(sectionId))
            {
                return Result<LayoutSchema>.Failure(LayoutSchemaErrors.DuplicateSectionId);
            }

            var placements = new List<LayoutPlacement>(definition.Placements.Count);

            foreach (var placementDefinition in definition.Placements.OrderBy(placement => placement.Order))
            {
                switch (placementDefinition)
                {
                    case LayoutFieldPlacementDefinition fieldDefinition:
                        if (!fieldsByKeywordId.TryGetValue(fieldDefinition.KeywordId, out var field))
                        {
                            return Result<LayoutSchema>.Failure(LayoutSchemaErrors.UnknownFieldReference);
                        }

                        if (!placedFieldIds.Add(field.Id))
                        {
                            return Result<LayoutSchema>.Failure(LayoutSchemaErrors.DuplicateFieldPlacement);
                        }

                        placements.Add(new LayoutFieldPlacement(field.Id, fieldDefinition.Order));
                        break;

                    case LayoutGroupPlacementDefinition groupDefinition:
                        var instanceKey = NormalizeInstanceKey(groupDefinition.InstanceKey);
                        if (!groupsByKey.TryGetValue((groupDefinition.KeywordGroupId, instanceKey), out _))
                        {
                            return Result<LayoutSchema>.Failure(LayoutSchemaErrors.UnknownGroupReference);
                        }

                        var groupKey = (groupDefinition.KeywordGroupId, instanceKey);
                        if (!placedGroupKeys.Add(groupKey))
                        {
                            return Result<LayoutSchema>.Failure(LayoutSchemaErrors.DuplicateGroupPlacement);
                        }

                        placements.Add(new LayoutGroupPlacement(
                            groupDefinition.KeywordGroupId,
                            instanceKey,
                            groupDefinition.Order));
                        break;

                    default:
                        throw new InvalidOperationException("Unsupported layout placement definition type.");
                }
            }

            sections.Add(new LayoutSection(
                sectionId,
                definition.Title.Trim(),
                definition.Order,
                placements));
        }

        sections.Sort((left, right) => left.Order.CompareTo(right.Order));

        var snapshots = sections.Select(BuildSectionSnapshot).ToList();
        return Result<LayoutSchema>.Success(new LayoutSchema(sections, snapshots));
    }

    internal static LayoutSectionSnapshot BuildSectionSnapshot(LayoutSection section)
    {
        var fieldPlacements = new List<LayoutFieldPlacementSnapshot>();
        var groupPlacements = new List<LayoutGroupPlacementSnapshot>();

        foreach (var placement in section.Placements.OrderBy(placement => placement.Order))
        {
            switch (placement)
            {
                case LayoutFieldPlacement fieldPlacement:
                    fieldPlacements.Add(new LayoutFieldPlacementSnapshot(
                        fieldPlacement.FieldSchemaId,
                        fieldPlacement.Order));
                    break;
                case LayoutGroupPlacement groupPlacement:
                    groupPlacements.Add(new LayoutGroupPlacementSnapshot(
                        groupPlacement.KeywordGroupId,
                        groupPlacement.InstanceKey,
                        groupPlacement.Order));
                    break;
            }
        }

        return new LayoutSectionSnapshot(
            section.Id,
            section.Title,
            section.Order,
            fieldPlacements,
            groupPlacements);
    }

    private static string NormalizeInstanceKey(string? instanceKey) =>
        string.IsNullOrWhiteSpace(instanceKey) ? string.Empty : instanceKey.Trim();
}
