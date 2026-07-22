using FlowMapper.Core;
using Xunit;

namespace FlowMapper.UnitTests;

public class ProfileDefinitionTests
{
    [Fact]
    public void CreateMap_AddsRegistration()
    {
        var profile = new TestProfile();

        Assert.Single(profile.Registrations);
        Assert.Equal(typeof(Source), profile.Registrations[0].SourceType);
        Assert.Equal(typeof(Dest), profile.Registrations[0].DestinationType);
    }

    [Fact]
    public void Profile_HasCorrectName()
    {
        var profile = new TestProfile();
        Assert.Equal("TestProfile", profile.ProfileName);
    }
}

public class TestProfile : ProfileDefinition
{
    public TestProfile()
    {
        ProfileName = "TestProfile";
        CreateMap<Source, Dest>();
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
    public string Name { get; set; } = "";
}
