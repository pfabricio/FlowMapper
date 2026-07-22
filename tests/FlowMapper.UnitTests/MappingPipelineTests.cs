using FlowMapper.Core;
using FlowMapper.Execution.Artifacts;
using FlowMapper.Mapping.Pipeline;
using Xunit;

namespace FlowMapper.UnitTests;

public class MappingPipelineTests
{
    private sealed class SourcePerson
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int Age { get; set; }
    }

    private sealed class DestPerson
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public int Age { get; set; }
    }

    private sealed class SourceWithNested
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public SourcePerson? Child { get; set; }
    }

    private sealed class DestWithNested
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public DestPerson? Child { get; set; }
    }

    private sealed class DestAddress
    {
        public string City { get; set; } = "";
    }

    private sealed class SourceFlat
    {
        public string CityName { get; set; } = "";
    }

    private sealed class DestFlat
    {
        public DestAddress Address { get; set; } = new();
    }

    [Fact]
    public void Pipeline_SimpleMapping_FromArtifact()
    {
        var artifact = new MappingArtifact(
            Name: "PersonMapping",
            Version: new Version(2, 0),
            SourceType: typeof(SourcePerson),
            DestinationType: typeof(DestPerson),
            MappingDelegate: null,
            ReverseMappingDelegate: null,
            BeforeMapDelegate: null,
            AfterMapDelegate: null
        );

        var pipeline = new MappingPipeline();
        var source = new SourcePerson { Id = 1, Name = "Alice", Age = 30 };

        var result = pipeline.Map<SourcePerson, DestPerson>(source, artifact);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("", result.FullName);  // different name — not matched by auto-mapping
        Assert.Equal(30, result.Age);
    }

    [Fact]
    public void Pipeline_SimpleMapping_FromFlow()
    {
        var flow = new Flow
        {
            Name = "Test",
            Signature = new FlowSignature
            {
                SourceType = typeof(SourcePerson),
                DestinationType = typeof(DestPerson)
            },
            Properties =
            [
                new PropertyFlow
                {
                    SourceProperty = "Id",
                    DestinationProperty = "Id",
                    SourceType = typeof(int),
                    DestinationType = typeof(int)
                },
                new PropertyFlow
                {
                    SourceProperty = "Name",
                    DestinationProperty = "FullName",
                    SourceType = typeof(string),
                    DestinationType = typeof(string)
                },
                new PropertyFlow
                {
                    SourceProperty = "Age",
                    DestinationProperty = "Age",
                    SourceType = typeof(int),
                    DestinationType = typeof(int)
                }
            ]
        };

        var builder = new MappingDelegateBuilder();
        var mapper = builder.BuildFromFlow<SourcePerson, DestPerson>(flow);
        var source = new SourcePerson { Id = 1, Name = "Alice", Age = 30 };

        var result = mapper(source);

        Assert.Equal(1, result.Id);
        Assert.Equal("Alice", result.FullName);
        Assert.Equal(30, result.Age);
    }

    [Fact]
    public void Pipeline_MapAll_MultipleItems()
    {
        var artifact = new MappingArtifact(
            Name: "PersonMapping",
            Version: new Version(2, 0),
            SourceType: typeof(SourcePerson),
            DestinationType: typeof(DestPerson),
            MappingDelegate: null,
            ReverseMappingDelegate: null,
            BeforeMapDelegate: null,
            AfterMapDelegate: null
        );

        var pipeline = new MappingPipeline();
        var sources = new[]
        {
            new SourcePerson { Id = 1, Name = "Alice", Age = 30 },
            new SourcePerson { Id = 2, Name = "Bob", Age = 25 }
        };

        var results = pipeline.MapAll<SourcePerson, DestPerson>(sources, artifact);

        Assert.Equal(2, results.Count);
        Assert.Equal("", results[0].FullName);  // different name — not matched by auto-mapping
        Assert.Equal("", results[1].FullName);
    }

    [Fact]
    public void Pipeline_WithCustomMappingDelegate_UsesIt()
    {
        var artifact = new MappingArtifact(
            Name: "Custom",
            Version: new Version(2, 0),
            SourceType: typeof(SourcePerson),
            DestinationType: typeof(DestPerson),
            MappingDelegate: new MappingDelegate<SourcePerson, DestPerson>(
                s => new DestPerson { Id = s.Id, FullName = $"Mapped: {s.Name}", Age = s.Age }),
            ReverseMappingDelegate: null,
            BeforeMapDelegate: null,
            AfterMapDelegate: null
        );

        var pipeline = new MappingPipeline();
        var source = new SourcePerson { Id = 1, Name = "Alice", Age = 30 };

        var result = pipeline.Map<SourcePerson, DestPerson>(source, artifact);

        Assert.Equal("Mapped: Alice", result.FullName);
    }

    [Fact]
    public void Pipeline_NullPropagationMiddleware_ThrowsOnNullSource()
    {
        var artifact = new MappingArtifact(
            Name: "Test",
            Version: new Version(2, 0),
            SourceType: typeof(SourcePerson),
            DestinationType: typeof(DestPerson),
            MappingDelegate: null,
            ReverseMappingDelegate: null,
            BeforeMapDelegate: null,
            AfterMapDelegate: null
        );

        var pipeline = new MappingPipeline(
            [new FlowMapper.Mapping.Pipeline.Middlewares.NullPropagationMiddleware()]);

        Assert.Throws<ArgumentNullException>(() =>
            pipeline.Map<SourcePerson, DestPerson>(null!, artifact));
    }
}
