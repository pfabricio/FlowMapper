using FlowMapper.Core;
using FluentAssertions;

namespace FlowMapper.Tests.Core;

public class MappingExpressionTests
{
    [Fact]
    public void ForMember_WithSourceProperty_CreatesExplicitMapping()
    {
        var expr = new MappingExpression<Source, Dest>();
        expr.ForMember(d => d.FullName, o => o.MapFrom("Name"));

        expr.ExplicitMappings.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new
            {
                SourceProperty = "Name",
                DestinationProperty = "FullName",
                IsIgnored = false,
                MapFromExpression = (string?)null
            });
    }

    [Fact]
    public void ForMember_WithIgnore_SetsIsIgnored()
    {
        var expr = new MappingExpression<Source, Dest>();
        expr.ForMember(d => d.FullName, o => o.Ignore());

        expr.ExplicitMappings.Should().ContainSingle(m => m.IsIgnored && m.DestinationProperty == "FullName");
    }

    [Fact]
    public void ForMember_WithExpression_SetsMapFromExpression()
    {
        var expr = new MappingExpression<Source, Dest>();
        expr.ForMember(d => d.FullName, o => o.MapFrom((Source s) => $"{s.Name} ({s.Email})"));

        expr.ExplicitMappings.Should().ContainSingle()
            .Which.MapFromExpression.Should().NotBeNull();
    }

    [Fact]
    public void Ignore_AddsToIgnoredProperties()
    {
        var expr = new MappingExpression<Source, Dest>();
        expr.Ignore(d => d.FullName);

        expr.IgnoredProperties.Should().Contain("FullName");
    }

    [Fact]
    public void UseConstructor_SetsPreferConstructor()
    {
        var expr = new MappingExpression<Source, Dest>();
        expr.UseConstructor();

        expr.PreferConstructor.Should().BeTrue();
    }

    [Fact]
    public void DisableFlatten_SetsEnableFlattenToFalse()
    {
        var expr = new MappingExpression<Source, Dest>();
        expr.DisableFlatten();

        expr.EnableFlatten.Should().BeFalse();
    }

    [Fact]
    public void AfterMap_WithExpression_SetsAfterMapMethod()
    {
        var expr = new MappingExpression<Source, Dest>();
        expr.AfterMap((s, d) => Console.Write(s.Name));

        expr.AfterMapMethod.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ConstructUsing_WithExpression_SetsConstructUsingMethod()
    {
        var expr = new MappingExpression<Source, Dest>();
        expr.ConstructUsing(s => new Dest());

        expr.ConstructUsingMethod.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ForPath_WithTwoSegments_CreatesPathMapping()
    {
        var expr = new MappingExpression<Source, DestWithNested>();
        expr.ForPath(d => d.Nested.FullName, o => o.MapFrom("Name"));

        expr.ExplicitMappings.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new
            {
                DestinationProperty = "Nested.FullName",
                SourceProperty = "Name",
                IsPathMapping = true,
                PathSegments = new List<string> { "Nested", "FullName" },
                IsIgnored = false,
                MapFromExpression = (string?)null
            });
    }

    [Fact]
    public void ForPath_WithOneSegment_CreatesPathMapping()
    {
        var expr = new MappingExpression<Source, Dest>();
        expr.ForPath(d => d.FullName, o => o.MapFrom("Name"));

        expr.ExplicitMappings.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new
            {
                DestinationProperty = "FullName",
                SourceProperty = "Name",
                IsPathMapping = true,
                PathSegments = new List<string> { "FullName" },
                IsIgnored = false,
                MapFromExpression = (string?)null
            });
    }

    [Fact]
    public void ForPath_WithoutExplicitMapFrom_UsesLastSegment()
    {
        var expr = new MappingExpression<Source, DestWithNested>();
        expr.ForPath(d => d.Nested.FullName, o => { });

        expr.ExplicitMappings.Should().ContainSingle()
            .Which.SourceProperty.Should().Be("FullName");
    }

    [Fact]
    public void ReverseMap_SetsReverseMappedFlag()
    {
        var expr = new MappingExpression<Source, Dest>();
        expr.ReverseMap();

        expr.ReverseMapped.Should().BeTrue();
    }

    [Fact]
    public void ReverseMap_WithoutCall_ReverseMappedIsFalse()
    {
        var expr = new MappingExpression<Source, Dest>();

        expr.ReverseMapped.Should().BeFalse();
    }

    [Fact]
    public void ReverseMap_ReturnsSameInstance()
    {
        var expr = new MappingExpression<Source, Dest>();
        var result = expr.ReverseMap();

        result.Should().BeSameAs(expr);
    }

    [Fact]
    public void ForPath_WithExpression_SetsMapFromExpression()
    {
        var expr = new MappingExpression<Source, DestWithNested>();
        expr.ForPath(d => d.Nested.FullName, o => o.MapFrom((Source s) => $"{s.Name}"));

        expr.ExplicitMappings.Should().ContainSingle()
            .Which.MapFromExpression.Should().NotBeNull();
    }

    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
    }

    public class Dest
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
    }

    public class DestWithNested
    {
        public int Id { get; set; }
        public NestedClass Nested { get; set; } = new();
    }

    public class NestedClass
    {
        public string FullName { get; set; } = "";
    }
}
