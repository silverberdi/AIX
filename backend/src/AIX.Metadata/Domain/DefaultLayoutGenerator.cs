using AIX.SharedKernel.Primitives;

namespace AIX.Metadata.Domain;

/// <summary>
/// Deterministic default layout from version composition using section heuristics:
/// standalone fields in a General section; each keyword group in its own section.
/// </summary>
public static class DefaultLayoutGenerator
{
    public const string GeneralSectionTitle = "General";

    public static Result<LayoutSchema> Generate(VersionSchemaComposition composition)
    {
        ArgumentNullException.ThrowIfNull(composition);

        if (composition.Fields.Count == 0 && composition.GroupAssignments.Count == 0)
        {
            return Result<LayoutSchema>.Success(LayoutSchema.Empty);
        }

        var sections = new List<LayoutSection>();
        var sectionOrder = 1;

        if (composition.Fields.Count > 0)
        {
            var fieldPlacements = new List<LayoutPlacement>();
            var placementOrder = 1;

            foreach (var field in composition.Fields
                         .OrderBy(field => field.OrderHint ?? int.MaxValue)
                         .ThenBy(field => field.KeywordId.Value))
            {
                fieldPlacements.Add(new LayoutFieldPlacement(field.Id, placementOrder++));
            }

            sections.Add(new LayoutSection(
                LayoutSectionId.New(),
                GeneralSectionTitle,
                sectionOrder++,
                fieldPlacements));
        }

        foreach (var group in composition.GroupAssignments
                     .GroupBy(assignment => assignment.KeywordGroupId)
                     .OrderBy(group => group.Min(assignment => assignment.Order))
                     .ThenBy(group => group.Key.Value))
        {
            var placements = new List<LayoutPlacement>();
            var placementOrder = 1;

            foreach (var assignment in group.OrderBy(assignment => assignment.Order)
                         .ThenBy(assignment => assignment.InstanceKey, StringComparer.Ordinal))
            {
                placements.Add(new LayoutGroupPlacement(
                    assignment.KeywordGroupId,
                    assignment.InstanceKey,
                    placementOrder++));
            }

            sections.Add(new LayoutSection(
                LayoutSectionId.New(),
                HumanizeGroupCode(group.First().GroupCode),
                sectionOrder++,
                placements));
        }

        return Result<LayoutSchema>.Success(LayoutSchema.FromSections(sections));
    }

    internal static string HumanizeGroupCode(string groupCode)
    {
        if (string.IsNullOrWhiteSpace(groupCode))
        {
            return "Group";
        }

        var words = groupCode.Trim()
            .Replace('_', ' ')
            .Replace('-', ' ')
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return string.Join(' ', words.Select(word =>
            word.Length == 0
                ? word
                : char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant()));
    }
}
