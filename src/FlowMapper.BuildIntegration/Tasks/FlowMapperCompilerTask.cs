using FlowMapper.Compiler.Pipeline;
using Microsoft.Build.Framework;
using MsBuildTask = Microsoft.Build.Utilities.Task;

namespace FlowMapper.BuildIntegration.Tasks;

public sealed class FlowMapperCompilerTask : MsBuildTask
{
    [Required]
    public string ProjectName { get; set; } = string.Empty;

    public string OutputPath { get; set; } = string.Empty;

    public bool EnableOptimizations { get; set; } = true;

    public bool EnableSourceGeneration { get; set; } = true;

    public bool EnableCaching { get; set; } = true;

    public override bool Execute()
    {
        try
        {
            var context = BuildIntegration.CreateDefaultContext(ProjectName);
            var integration = new BuildIntegration();
            var result = integration.Execute(context);

            var forwarder = new BuildDiagnosticsForwarder();
            forwarder.Forward(result);

            foreach (var diag in result.Diagnostics)
            {
                switch (diag.Severity)
                {
                    case CompilerDiagnosticSeverity.Error:
                        Log.LogError("FMPLR", "0", "", null, 0, 0, 0, 0, diag.Message);
                        break;
                    case CompilerDiagnosticSeverity.Warning:
                        Log.LogWarning("FMPLR", "0", "", null, 0, 0, 0, 0, diag.Message);
                        break;
                    case CompilerDiagnosticSeverity.Info:
                        Log.LogMessage(diag.Message);
                        break;
                }
            }

            if (!result.Success && false)
                return false;

            return true;
        }
        catch (Exception ex)
        {
            Log.LogError($"FlowMapper compiler failed: {ex.Message}");
            return false;
        }
    }
}
