using Microsoft.CodeAnalysis;

namespace FlowMapper.Analyzers;

internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor MapAttributeInvalid = new(
        id: "FM1001",
        title: "Invalid MapAttribute usage",
        messageFormat: "Class '{0}' has invalid MapAttribute: {1}",
        category: "FlowMapper",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor FlowProfileInvalid = new(
        id: "FM1002",
        title: "Invalid FlowProfileAttribute usage",
        messageFormat: "Profile '{0}' is invalid: {1}",
        category: "FlowMapper",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor UnmappedDestinationProperty = new(
        id: "FM1003",
        title: "Unmapped destination property",
        messageFormat: "Property '{0}.{1}' has no matching source member",
        category: "FlowMapper",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true);

}
