#pragma warning disable CS0618

using System.Collections.Immutable;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using FlowMapper.Core;
using FlowMapper.SourceGenerator;
using FlowMapper.SourceGenerator.Models;
using FlowMapper.SourceGenerator.Pipeline;
using FlowMapper.SourceGenerator.Pipeline.Builder;
using FlowMapper.SourceGenerator.Pipeline.Validator;
using FlowMapper.SourceGenerator.Performance;

namespace FlowMapper.PerformanceTests;

[MemoryDiagnoser]
public class PipelineBenchmarks
{
    private CSharpCompilation _compilation = null!;
    private INamedTypeSymbol _userType = null!;
    private INamedTypeSymbol _userDtoType = null!;
    private INamedTypeSymbol _employeeType = null!;
    private INamedTypeSymbol _employeeDtoType = null!;
    private INamedTypeSymbol _addressType = null!;
    private INamedTypeSymbol _addressDtoType = null!;

    private MapperDefinition _basicCandidate = null!;
    private MapperDefinition _constructorCandidate = null!;
    private MapperDefinition _flattenCandidate = null!;
    private MapperDefinition _nestedCandidate = null!;

    [GlobalSetup]
    public void Setup()
    {
        var source = """
            using FlowMapper.Abstractions;

            [Map<User, UserDto>]
            public partial class UserMapper : IMapper<User, UserDto>;

            [Map<UserCtor, UserCtorDto>]
            public partial class CtorMapper : IMapper<UserCtor, UserCtorDto>;

            [Map<Employee, EmployeeDto>]
            public partial class EmpMapper : IMapper<Employee, EmployeeDto>;

            [Map<UserNested, UserNestedDto>]
            public partial class NestedUserMapper : IMapper<UserNested, UserNestedDto>;

            public class User
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
                public string Email { get; set; } = "";
                public int Age { get; set; }
            }

            public class UserDto
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
                public string Email { get; set; } = "";
                public int Age { get; set; }
            }

            public class UserCtor
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }

            public class UserCtorDto(int id, string name)
            {
                public int Id { get; } = id;
                public string Name { get; } = name;
            }

            public class Employee
            {
                public string Name { get; set; } = "";
                public Address Address { get; set; } = new();
            }

            public class Address
            {
                public string Street { get; set; } = "";
                public string City { get; set; } = "";
                public string ZipCode { get; set; } = "";
            }

            public class EmployeeDto
            {
                public string Name { get; set; } = "";
                public string AddressStreet { get; set; } = "";
                public string AddressCity { get; set; } = "";
                public string AddressZipCode { get; set; } = "";
            }

            public class UserNested
            {
                public int Id { get; set; }
                public AddressNested Address { get; set; } = new();
            }

            public class AddressNested
            {
                public string Street { get; set; } = "";
                public string City { get; set; } = "";
            }

            public class UserNestedDto
            {
                public int Id { get; set; }
                public AddressNestedDto Address { get; set; } = new();
            }

            public class AddressNestedDto
            {
                public string Street { get; set; } = "";
                public string City { get; set; } = "";
            }
            """;

        var syntaxTree = CSharpSyntaxTree.ParseText(source,
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp12));

        var metadataRefs = new List<MetadataReference>();
        var assemblies = new[]
        {
            typeof(object).Assembly,
            typeof(Enumerable).Assembly,
            typeof(FlowMapper.Abstractions.MapAttribute<,>).Assembly,
            typeof(FlowMapper.Core.Flow).Assembly,
        };
        foreach (var asm in assemblies)
            if (!string.IsNullOrEmpty(asm.Location))
                metadataRefs.Add(MetadataReference.CreateFromFile(asm.Location));
        metadataRefs.Add(MetadataReference.CreateFromFile(
            Assembly.Load("netstandard").Location));

        _compilation = CSharpCompilation.Create(
            "PerfAssembly",
            new[] { syntaxTree },
            metadataRefs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        _userType = GetTypeSymbol("User")!;
        _userDtoType = GetTypeSymbol("UserDto")!;
        _employeeType = GetTypeSymbol("Employee")!;
        _employeeDtoType = GetTypeSymbol("EmployeeDto")!;
        _addressType = GetTypeSymbol("Address")!;
        _addressDtoType = GetTypeSymbol("AddressDto")!;

        var userMapper = GetTypeSymbol("UserMapper")!;
        var ctorMapper = GetTypeSymbol("CtorMapper")!;
        var empMapper = GetTypeSymbol("EmpMapper")!;
        var nestedMapper = GetTypeSymbol("NestedUserMapper")!;

        var mapAttrUser = userMapper.GetAttributes()
            .First(a => a.AttributeClass?.Name == "MapAttribute");
        var mapAttrCtor = ctorMapper.GetAttributes()
            .First(a => a.AttributeClass?.Name == "MapAttribute");
        var mapAttrEmp = empMapper.GetAttributes()
            .First(a => a.AttributeClass?.Name == "MapAttribute");
        var mapAttrNested = nestedMapper.GetAttributes()
            .First(a => a.AttributeClass?.Name == "MapAttribute");

        _basicCandidate = MapperDefinitionFactory.Create(userMapper, mapAttrUser);
        _constructorCandidate = MapperDefinitionFactory.Create(ctorMapper, mapAttrCtor);
        _flattenCandidate = MapperDefinitionFactory.Create(empMapper, mapAttrEmp);
        _nestedCandidate = MapperDefinitionFactory.Create(nestedMapper, mapAttrNested);
    }

    private INamedTypeSymbol? GetTypeSymbol(string name)
    {
        return _compilation.GetSymbolsWithName(n => n == name, SymbolFilter.Type)
            .OfType<INamedTypeSymbol>()
            .FirstOrDefault();
    }

    [Benchmark]
    public Flow Build_BasicMapping()
    {
        return FlowBuilder.Build(_basicCandidate);
    }

    [Benchmark]
    public Flow Build_ConstructorMapping()
    {
        return FlowBuilder.Build(_constructorCandidate);
    }

    [Benchmark]
    public Flow Build_FlattenMapping()
    {
        return FlowBuilder.Build(_flattenCandidate);
    }

    [Benchmark]
    public Flow Build_NestedMapping()
    {
        return FlowBuilder.Build(_nestedCandidate);
    }

    [Benchmark]
    public FlowModel Pipeline_Basic()
    {
        return FlowPipeline.Execute(new[] { _basicCandidate });
    }

    [Benchmark]
    public FlowModel Pipeline_FourCandidates()
    {
        return FlowPipeline.Execute(new[]
        {
            _basicCandidate,
            _constructorCandidate,
            _flattenCandidate,
            _nestedCandidate
        });
    }

    [Benchmark]
    public List<FlowDiagnosticResult> Validate_Basic()
    {
        var flow = FlowBuilder.Build(_basicCandidate);
        return FlowValidator.Validate(_basicCandidate, flow);
    }

    [Benchmark]
    public FlattenPath? Resolve_FlattenPath()
    {
        var streetProp = _employeeDtoType.GetMembers()
            .OfType<IPropertySymbol>()
            .First(p => p.Name == "AddressStreet");
        return FlattenResolver.ResolvePath(_employeeType, "AddressStreet", streetProp.Type);
    }
}
