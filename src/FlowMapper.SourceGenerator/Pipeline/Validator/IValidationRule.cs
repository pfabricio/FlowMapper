using System.Collections.Generic;
using FlowMapper.Core;
using FlowMapper.SourceGenerator.Models;

namespace FlowMapper.SourceGenerator.Pipeline.Validator;

public interface IValidationRule
{
    string RuleId { get; }
    IEnumerable<FlowDiagnosticResult> Validate(MapperDefinition candidate, Flow flow);
}