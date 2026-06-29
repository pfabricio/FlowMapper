using Microsoft.CodeAnalysis;

namespace FlowMapper.SourceGenerator.Pipeline.Validator;

public static class FlowDiagnostics
{
    public static readonly DiagnosticDescriptor MissingDestinationProperty = new(
        id: "FM0001",
        title: "Property not mapped",
        messageFormat: "Destination property '{0}' is not mapped",
        category: "FlowMapper",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TypeMismatch = new(
        id: "FM0002",
        title: "Type mismatch",
        messageFormat: "Cannot map '{0}' to '{1}'",
        category: "FlowMapper",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidMapper = new(
        id: "FM0003",
        title: "Invalid mapper",
        messageFormat: "Mapper '{0}' is invalid or incomplete",
        category: "FlowMapper",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor IncompleteMapping = new(
        id: "FM0004",
        title: "Incomplete mapping",
        messageFormat: "Source property '{0}' has no matching destination",
        category: "FlowMapper",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MalformedMapAttribute = new(
        id: "FM0005",
        title: "Malformed Map attribute",
        messageFormat: "MapAttribute is malformed on '{0}'",
        category: "FlowMapper",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor CyclicReference = new(
        id: "FM0006",
        title: "Cyclic reference detected",
        messageFormat: "Cycle detected in mapping path: {0}",
        category: "FlowMapper",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor ConstructorMismatch = new(
        id: "FM0007",
        title: "Constructor mismatch",
        messageFormat: "No suitable constructor found for type '{0}'",
        category: "FlowMapper",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MissingConstructorBinding = new(
        id: "FM0008",
        title: "Missing constructor binding",
        messageFormat: "Required constructor parameter '{0}' not mapped",
        category: "FlowMapper",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor AmbiguousFlattenPath = new(
        id: "FM0009",
        title: "Ambiguous Flatten Path",
        messageFormat: "Multiple paths found for property '{0}'",
        category: "FlowMapper",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor FlattenPathNotFound = new(
        id: "FM0010",
        title: "Flatten Path Not Found",
        messageFormat: "No valid path found for '{0}'",
        category: "FlowMapper",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidFlattenDepth = new(
        id: "FM0011",
        title: "Invalid Flatten Depth",
        messageFormat: "Cycle or invalid depth detected in flatten graph",
        category: "FlowMapper",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
