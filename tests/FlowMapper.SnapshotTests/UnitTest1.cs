using FlowMapper.Abstractions;
using FlowMapper.Core;
using FlowMapper.SourceGenerator.Pipeline;
using FlowMapper.SourceGenerator.Pipeline.Generator;
using FlowMapper.SourceGenerator.Models;

namespace FlowMapper.SnapshotTests;

public class GeneratedCodeSnapshotTests
{
    [Fact]
    public void BasicMapping_Generated_Code_Contains_MapMethod()
    {
        var flow = new Flow
        {
            SourceType = "User",
            DestinationType = "UserDto",
            Properties = new List<PropertyFlow>
            {
                new() { SourceProperty = "Id", DestinationProperty = "Id", Strategy = MappingStrategy.Direct },
                new() { SourceProperty = "Name", DestinationProperty = "Name", Strategy = MappingStrategy.Direct }
            }
        };

        var model = new FlowModel(new List<Flow> { flow }, "UserMapper", new());
        var code = FlowCodeGenerator.Generate(model);

        Assert.Contains("IMapper<User, UserDto>", code);
        Assert.Contains("UserDto Map(User source)", code);
        Assert.Contains("Id = source.Id", code);
        Assert.Contains("Name = source.Name", code);
    }

    [Fact]
    public void NestedMapping_Generated_Code_Contains_HelperMethod()
    {
        var childFlow = new Flow
        {
            SourceType = "Address",
            DestinationType = "AddressDto",
            Properties = new List<PropertyFlow>
            {
                new() { SourceProperty = "Street", DestinationProperty = "Street", Strategy = MappingStrategy.Direct }
            }
        };

        var flow = new Flow
        {
            SourceType = "User",
            DestinationType = "UserDto",
            Properties = new List<PropertyFlow>(),
            NestedFlows = new List<NestedFlow>
            {
                new()
                {
                    ParentProperty = "Address",
                    ChildFlow = childFlow,
                    Strategy = MappingStrategy.Nested
                }
            }
        };

        var model = new FlowModel(new List<Flow> { flow }, "NestedUserMapper", new());
        var code = FlowCodeGenerator.Generate(model);

        Assert.Contains("MapAddress", code);
        Assert.Contains("AddressDto MapAddress(Address source)", code);
    }
}
