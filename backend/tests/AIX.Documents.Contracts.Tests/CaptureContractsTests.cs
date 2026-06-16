using AIX.Documents.Contracts;
using FluentAssertions;

namespace AIX.Documents.Contracts.Tests;

public class CaptureContractsTests
{
    [Fact]
    public void constructs_captured_metadata_payload_with_standalone_values()
    {
        var payload = new CapturedMetadataPayload(
            standaloneValues: new Dictionary<string, string?>
            {
                ["vendor_name"] = "Acme Corp",
                ["amount"] = "100.50"
            });

        payload.StandaloneValues.Should().ContainKey("vendor_name").WhoseValue.Should().Be("Acme Corp");
        payload.StandaloneValues.Should().ContainKey("amount").WhoseValue.Should().Be("100.50");
        payload.GroupInstances.Should().BeEmpty();
    }

    [Fact]
    public void constructs_captured_metadata_payload_with_group_instances()
    {
        var groupInstance = new CapturedMetadataGroupInstance(
            "line_items",
            "1",
            new Dictionary<string, string?>
            {
                ["description"] = "Widget",
                ["quantity"] = "2"
            });

        var payload = new CapturedMetadataPayload(
            groupInstances: [groupInstance]);

        payload.StandaloneValues.Should().BeEmpty();
        payload.GroupInstances.Should().ContainSingle();
        payload.GroupInstances[0].GroupCode.Should().Be("line_items");
        payload.GroupInstances[0].InstanceKey.Should().Be("1");
        payload.GroupInstances[0].Values.Should().ContainKey("description").WhoseValue.Should().Be("Widget");
        payload.GroupInstances[0].Values.Should().ContainKey("quantity").WhoseValue.Should().Be("2");
    }

    [Fact]
    public void captured_metadata_payload_is_immutable_after_construction()
    {
        var standaloneValues = new Dictionary<string, string?> { ["vendor_name"] = "Acme Corp" };
        var groupValues = new Dictionary<string, string?> { ["description"] = "Widget" };
        var groupInstances = new List<CapturedMetadataGroupInstance>
        {
            new("line_items", "1", groupValues)
        };

        var payload = new CapturedMetadataPayload(standaloneValues, groupInstances);

        standaloneValues["vendor_name"] = "Changed Corp";
        standaloneValues["new_key"] = "unexpected";
        groupValues["description"] = "Changed Widget";
        groupInstances.Add(new CapturedMetadataGroupInstance("other", null, new Dictionary<string, string?>()));

        payload.StandaloneValues.Should().ContainKey("vendor_name").WhoseValue.Should().Be("Acme Corp");
        payload.StandaloneValues.Should().NotContainKey("new_key");
        payload.GroupInstances.Should().ContainSingle();
        payload.GroupInstances[0].Values.Should().ContainKey("description").WhoseValue.Should().Be("Widget");
    }

    [Fact]
    public void capture_validation_result_success_has_no_errors()
    {
        var result = CaptureValidationResult.Success();

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void capture_validation_result_failure_carries_multiple_errors()
    {
        var errors = new List<CaptureValidationError>
        {
            new("required.missing", "Vendor name is required."),
            new("unknown.keyword", "Unknown keyword 'extra'.")
        };

        var result = CaptureValidationResult.Failure(errors);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().HaveCount(2);
        result.Errors[0].Code.Should().Be("required.missing");
        result.Errors[0].Message.Should().Be("Vendor name is required.");
        result.Errors[1].Code.Should().Be("unknown.keyword");
        result.Errors[1].Message.Should().Be("Unknown keyword 'extra'.");
    }

    [Fact]
    public void capture_validation_error_carries_deterministic_details()
    {
        var error = new CaptureValidationError("required.missing", "Vendor name is required.");

        error.Code.Should().Be("required.missing");
        error.Message.Should().Be("Vendor name is required.");
    }

    [Fact]
    public void contracts_project_has_no_forbidden_references()
    {
        var assembly = typeof(CapturedMetadataPayload).Assembly;
        var referencedNames = assembly.GetReferencedAssemblies().Select(a => a.Name).ToList();

        referencedNames.Should().NotContain("AIX.Documents");
        referencedNames.Should().NotContain("AIX.Metadata");
        referencedNames.Should().NotContain("AIX.Metadata.Contracts");
        referencedNames.Should().NotContain("AIX.SharedKernel");
    }

    [Fact]
    public void contracts_project_defines_no_behavioral_ports()
    {
        var assembly = typeof(CapturedMetadataPayload).Assembly;
        var exportedTypes = assembly.GetExportedTypes();

        exportedTypes.Should().NotContain(t => t.Name == "ICaptureMetadataValidator");
        exportedTypes
            .Where(t => t.IsInterface && t.IsPublic)
            .Select(t => t.Name)
            .Should()
            .NotContain(name => name.Contains("Validator", StringComparison.Ordinal));
    }
}
