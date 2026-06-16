using AIX.Metadata.Contracts;
using AIX.Metadata.Domain;
using AIX.SharedKernel.Primitives;

namespace AIX.Metadata;

public static class DocumentSchemaProjector
{
    public static Result<DocumentSchemaContract> ToContract(
        DocumentType documentType,
        DocumentTypeVersionId versionId)
    {
        ArgumentNullException.ThrowIfNull(documentType);

        var version = documentType.Versions.FirstOrDefault(existing => existing.Id == versionId);
        if (version is null)
        {
            return Result<DocumentSchemaContract>.Failure(DocumentSchemaProjectorErrors.VersionNotFound);
        }

        return Result<DocumentSchemaContract>.Success(ToContract(documentType, version));
    }

    public static DocumentSchemaContract ToContract(DocumentType documentType, DocumentTypeVersion version)
    {
        ArgumentNullException.ThrowIfNull(documentType);
        ArgumentNullException.ThrowIfNull(version);

        var composition = version.SchemaComposition;
        var keywordCodesById = composition.FieldSnapshots.ToDictionary(
            snapshot => snapshot.KeywordId,
            snapshot => snapshot.KeywordCode);

        var fields = composition.Fields
            .OrderBy(field => field.OrderHint ?? int.MaxValue)
            .ThenBy(field => field.Id.Value)
            .Select(field => new FieldSchemaContract(
                field.Id.Value,
                field.KeywordId.Value,
                keywordCodesById[field.KeywordId],
                MapCatalogType(field.CatalogType),
                field.LabelOverride,
                field.HelpText,
                field.OrderHint,
                field.IsDeprecated,
                field.IsHidden,
                DatasetId: null))
            .ToList();

        var groups = composition.GroupAssignments
            .OrderBy(assignment => assignment.Order)
            .ThenBy(assignment => assignment.KeywordGroupId.Value)
            .ThenBy(assignment => assignment.InstanceKey, StringComparer.Ordinal)
            .Select(assignment => new GroupAssignmentContract(
                assignment.KeywordGroupId.Value,
                assignment.GroupCode,
                assignment.Order,
                assignment.InstanceKey,
                assignment.IsRepeatable,
                assignment.IsRequired,
                assignment.KeywordIds.Select(keywordId => keywordId.Value).ToList()))
            .ToList();

        var layout = MapLayout(version.LayoutSchema, composition);

        return new DocumentSchemaContract(
            documentType.Id.Value,
            documentType.Code,
            documentType.Name,
            version.Id.Value,
            version.VersionNumber,
            BuildSchemaBindingKey(documentType.Code, version.VersionNumber),
            fields,
            groups,
            layout);
    }

    private static string BuildSchemaBindingKey(string documentTypeCode, int versionNumber) =>
        $"{documentTypeCode.Trim()}/v{versionNumber}";

    private static LayoutSchemaContract MapLayout(LayoutSchema layoutSchema, VersionSchemaComposition composition)
    {
        var fieldById = composition.Fields.ToDictionary(field => field.Id);
        var groupByKey = composition.GroupAssignments.ToDictionary(
            assignment => (assignment.KeywordGroupId, assignment.InstanceKey));

        var sections = layoutSchema.Sections
            .OrderBy(section => section.Order)
            .ThenBy(section => section.Id.Value)
            .Select(section =>
            {
                var fieldPlacements = new List<LayoutFieldPlacementContract>();
                var groupPlacements = new List<LayoutGroupPlacementContract>();

                foreach (var placement in section.Placements.OrderBy(placement => placement.Order))
                {
                    switch (placement)
                    {
                        case LayoutFieldPlacement fieldPlacement when fieldById.ContainsKey(fieldPlacement.FieldSchemaId):
                            fieldPlacements.Add(new LayoutFieldPlacementContract(
                                fieldPlacement.FieldSchemaId.Value,
                                fieldPlacement.Order));
                            break;
                        case LayoutGroupPlacement groupPlacement
                            when groupByKey.ContainsKey((groupPlacement.KeywordGroupId, groupPlacement.InstanceKey)):
                            groupPlacements.Add(new LayoutGroupPlacementContract(
                                groupPlacement.KeywordGroupId.Value,
                                groupPlacement.InstanceKey,
                                groupPlacement.Order));
                            break;
                    }
                }

                return new LayoutSectionContract(
                    section.Id.Value,
                    section.Title,
                    section.Order,
                    fieldPlacements,
                    groupPlacements);
            })
            .ToList();

        return new LayoutSchemaContract(sections);
    }

    private static RendererFieldCatalogType MapCatalogType(FieldCatalogType catalogType) =>
        catalogType switch
        {
            FieldCatalogType.Text => RendererFieldCatalogType.Text,
            FieldCatalogType.TextArea => RendererFieldCatalogType.TextArea,
            FieldCatalogType.Number => RendererFieldCatalogType.Number,
            FieldCatalogType.Decimal => RendererFieldCatalogType.Decimal,
            FieldCatalogType.Date => RendererFieldCatalogType.Date,
            FieldCatalogType.DateTime => RendererFieldCatalogType.DateTime,
            FieldCatalogType.Boolean => RendererFieldCatalogType.Boolean,
            _ => throw new ArgumentOutOfRangeException(nameof(catalogType), catalogType, "Unsupported field catalog type.")
        };
}
