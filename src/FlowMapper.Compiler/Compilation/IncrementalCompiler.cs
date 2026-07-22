using System.Diagnostics;
using FlowMapper.Core;
using FlowMapper.Execution;

namespace FlowMapper.Compiler.Compilation;

public sealed class IncrementalCompiler : IIncrementalCompiler
{
    private readonly Compiler _compiler;
    private readonly FlowBuilder _flowBuilder;
    private readonly ICompilationCache _cache;
    private readonly IDependencyGraph _dependencyGraph;
    private readonly IChangeTracker _changeTracker;

    public IncrementalCompiler(
        Compiler compiler,
        FlowBuilder flowBuilder,
        ICompilationCache? cache = null,
        IDependencyGraph? dependencyGraph = null,
        IChangeTracker? changeTracker = null)
    {
        _compiler = compiler;
        _flowBuilder = flowBuilder;
        _cache = cache ?? new CompilationCache();
        _dependencyGraph = dependencyGraph ?? new DependencyGraph();
        _changeTracker = changeTracker ?? new ChangeTracker();
    }

    public CompilationResult Compile(IReadOnlyList<ProfileDefinition> profiles)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var artifacts = _compiler.Compile(profiles);
            var key = CompilationKey.Create<ExecutionArtifact>(profiles);
            _cache.Store(key, artifacts);
            _dependencyGraph.AddDependency(key, key);

            foreach (var profile in profiles)
                TrackProfile(profile);

            sw.Stop();
            return CompilationResult.Compiled(artifacts, sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return CompilationResult.Failed(ex.Message, sw.Elapsed);
        }
    }

    public CompilationResult CompileIncremental(IReadOnlyList<ProfileDefinition> profiles)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var key = CompilationKey.Create<ExecutionArtifact>(profiles);

            if (_cache.TryGet<IReadOnlyList<ExecutionArtifact>>(key, out var cached))
            {
                var valid = true;
                foreach (var dep in _dependencyGraph.GetDependents(key))
                    if (!_dependencyGraph.IsValid(dep))
                        valid = false;

                if (valid)
                {
                    sw.Stop();
                    return CompilationResult.Cached(cached!);
                }
            }

            var changedProfiles = profiles
                .Where(p => HasProfileChanged(p))
                .ToList();

            if (changedProfiles.Count == 0 && _cache.Count > 0)
            {
                var allArtifacts = new List<ExecutionArtifact>();
                foreach (var k in _cache.Keys)
                    if (_cache.TryGet<IReadOnlyList<ExecutionArtifact>>(k, out var entry))
                        allArtifacts.AddRange(entry!);
                return CompilationResult.Cached(allArtifacts);
            }

            var artifacts = _compiler.Compile(profiles);
            _cache.Store(key, artifacts);
            _dependencyGraph.AddDependency(key, key);

            foreach (var profile in profiles)
                TrackProfile(profile);

            sw.Stop();
            return CompilationResult.Compiled(artifacts, sw.Elapsed);
        }
        catch (Exception ex)
        {
            sw.Stop();
            return CompilationResult.Failed(ex.Message, sw.Elapsed);
        }
    }

    public void InvalidateCache(Type type)
    {
        var matchingKeys = _cache.Keys
            .Where(k => k.ComponentType == (object)type)
            .ToList();

        foreach (var key in matchingKeys)
        {
            _cache.Invalidate(key);
            _dependencyGraph.Invalidate(key);
        }
    }

    public void InvalidateAll()
    {
        _cache.Clear();
        _dependencyGraph.Clear();
        _changeTracker.Reset();
    }

    private bool HasProfileChanged(ProfileDefinition profile)
    {
        var content = GetProfileContent(profile);
        return _changeTracker.HasChanged(profile.ProfileName, content);
    }

    private void TrackProfile(ProfileDefinition profile)
    {
        var content = GetProfileContent(profile);
        _changeTracker.Track(profile.ProfileName, content);
    }

    private static string GetProfileContent(ProfileDefinition profile)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(profile.ProfileName);

        foreach (var reg in profile.Registrations)
        {
            sb.Append(reg.SourceType.FullName);
            sb.Append(reg.DestinationType.FullName);
        }

        return sb.ToString();
    }
}
