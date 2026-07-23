using FlowMapper.SourceGenerator.Models;

namespace FlowMapper.SourceGenerator.Pipeline.Validator;

public interface IValidationRule
{
    string RuleId { get; }
    IEnumerable<FlowDiagnosticResult> Validate(MapperDefinition candidate, FlowDescriptor flow);
}