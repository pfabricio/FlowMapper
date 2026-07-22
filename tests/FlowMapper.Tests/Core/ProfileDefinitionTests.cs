using FlowMapper.Core;
using FluentAssertions;

namespace FlowMapper.Tests.Core;

public class ProfileDefinitionTests
{
    [Fact]
    public void CreateMap_AddsMappingToList()
    {
        var profile = new TestProfile();
        profile.Initialize();

        profile.Mappings.Should().ContainItemsAssignableTo<MappingExpression<Source, Dest>>();
    }

    [Fact]
    public void CreateDataReaderMap_AddsMappingToList()
    {
        var profile = new TestProfile();
        profile.Initialize();

        profile.Mappings.Should().ContainItemsAssignableTo<DataReaderMappingExpression<Dest>>();
    }

    [Fact]
    public void DefaultValues_AreSet()
    {
        var profile = new ProfileDefinition();

        profile.Name.Should().BeEmpty();
        profile.EnableFlatten.Should().BeTrue();
        profile.PreferConstructor.Should().BeFalse();
        profile.StrictMapping.Should().BeFalse();
    }

    private class TestProfile : ProfileDefinition
    {
        public void Initialize()
        {
            CreateMap<Source, Dest>()
                .ForMember(d => d.FullName, o => o.MapFrom("Name"));
            CreateDataReaderMap<Dest>();
        }
    }

    public class Source
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class Dest
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
    }
}
