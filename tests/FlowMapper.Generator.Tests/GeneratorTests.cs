using Xunit;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using FlowMapper.SourceGenerator;

namespace FlowMapper.Generator.Tests;

public class GeneratorTests
{
    [Fact]
    public void BasicMapping_GeneratesMapMethod()
    {
        var source = """
            using FlowMapper.Abstractions;

            [Map<User, UserDto>]
            public partial class UserMapper : IMapper<User, UserDto>;

            public class User
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }

            public class UserDto
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }
            """;

        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("Map(User source)", generated);
        Assert.Contains("Id = source.Id", generated);
        Assert.Contains("Name = source.Name", generated);
    }

    [Fact]
    public void ConstructorMapping_GeneratesConstructorCall()
    {
        var source = """
            using FlowMapper.Abstractions;

            [Map<Source, Dest>]
            public partial class CtorMapper : IMapper<Source, Dest>;

            public class Source
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }

            public class Dest(int id, string name)
            {
                public int Id { get; } = id;
                public string Name { get; } = name;
            }
            """;

        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("new Dest(", generated);
    }

    [Fact]
    public void FlattenMapping_GeneratesFlattenPath()
    {
        var source = """
            using FlowMapper.Abstractions;

            [Map<Employee, EmployeeDto>]
            public partial class EmpMapper : IMapper<Employee, EmployeeDto>;

            public class Employee
            {
                public string Name { get; set; } = "";
                public Address Address { get; set; } = new();
            }

            public class Address
            {
                public string Street { get; set; } = "";
                public string City { get; set; } = "";
            }

            public class EmployeeDto
            {
                public string Name { get; set; } = "";
                public string AddressStreet { get; set; } = "";
                public string AddressCity { get; set; } = "";
            }
            """;

        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("Address.Street", generated);
        Assert.Contains("Address.City", generated);
    }

    [Fact]
    public void NestedMapping_GeneratesNestedHelpers()
    {
        var source = """
            using FlowMapper.Abstractions;

            [Map<User, UserDto>]
            public partial class UserMapper : IMapper<User, UserDto>;

            public class User
            {
                public int Id { get; set; }
                public Address Address { get; set; } = new();
            }

            public class Address
            {
                public string Street { get; set; } = "";
            }

            public class UserDto
            {
                public int Id { get; set; }
                public AddressDto Address { get; set; } = new();
            }

            public class AddressDto
            {
                public string Street { get; set; } = "";
            }
            """;

        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("MapAddress", generated);
        Assert.Contains("target.Address = MapAddress(source.Address)", generated);
    }

    [Fact]
    public void ProfileDefinition_CreateFromProfileDirectTest()
    {
        var source = """
            using FlowMapper.Abstractions;
            using FlowMapper.Core;

            public class MyProfile : ProfileDefinition
            {
                public MyProfile()
                {
                    CreateMap<User, UserDto>();
                }
            }

            public class User
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }

            public class UserDto
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }
            """;

        var syntaxTree = CSharpSyntaxTree.ParseText(source,
            CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp12));
        var metadataRefs = GeneratorTestHelper.GetMetadataReferences();
        var compilation = CSharpCompilation.Create("Test",
            new[] { syntaxTree },
            metadataRefs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var classDecl = syntaxTree.GetRoot().DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .First(c => c.Identifier.Text == "MyProfile");

        var model = compilation.GetSemanticModel(syntaxTree);
        var symbol = model.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
        Assert.NotNull(symbol);
        Assert.Equal("MyProfile", symbol!.Name);

        var baseType = symbol.BaseType;
        Assert.NotNull(baseType);
        Assert.Equal("ProfileDefinition", baseType!.Name);

        // Trace through CreateFromProfile logic manually
        foreach (var syntaxRef in symbol.DeclaringSyntaxReferences)
        {
            var syntax = syntaxRef.GetSyntax();
            Assert.IsType<ClassDeclarationSyntax>(syntax);
            var clsDecl = (ClassDeclarationSyntax)syntax;

            var ctor = clsDecl.Members.OfType<ConstructorDeclarationSyntax>().FirstOrDefault();
            Assert.NotNull(ctor);

            var semanticModel2 = compilation.GetSemanticModel(ctor.SyntaxTree);
            Assert.NotNull(semanticModel2);

            var invocations = ctor.DescendantNodes()
                .OfType<InvocationExpressionSyntax>()
                .Where(inv => inv.Expression is MemberAccessExpressionSyntax memberAccess
                           && memberAccess.Name.Identifier.Text == "CreateMap"
                           || inv.Expression is IdentifierNameSyntax idName
                           && idName.Identifier.Text == "CreateMap")
                .ToList();

            System.Console.Error.WriteLine($"  Found {invocations.Count} CreateMap invocations");

            foreach (var inv in invocations)
            {
                System.Console.Error.WriteLine($"  Invocation: {inv}");
                var symbolInfo = semanticModel2.GetSymbolInfo(inv);
                System.Console.Error.WriteLine($"  SymbolInfo.Symbol: {symbolInfo.Symbol}");
                System.Console.Error.WriteLine($"  SymbolInfo.CandidateReason: {symbolInfo.CandidateReason}");

                if (symbolInfo.Symbol is IMethodSymbol ms)
                {
                    System.Console.Error.WriteLine($"  Method: {ms.Name}, TypeArgs: {ms.TypeArguments.Length}");
                }
                else
                {
                    foreach (var cs in symbolInfo.CandidateSymbols)
                        System.Console.Error.WriteLine($"  Candidate: {cs}, Kind: {cs.Kind}");
                }
            }
        }

        // Now call the actual factory
        var candidates = MapperDefinitionFactory.CreateFromProfile(symbol, compilation);
        System.Console.Error.WriteLine($"  CreateFromProfile returned {candidates.Count} candidates");
        Assert.NotEmpty(candidates);
        Assert.Single(candidates);
        var candidate = candidates[0];
        Assert.Equal("UserToUserDtoMapper", candidate.MapperName);
    }

    [Fact]
    public void ProfileDefinition_BasicMapping()
    {
        var source = """
            using FlowMapper.Abstractions;
            using FlowMapper.Core;

            public class MyProfile : ProfileDefinition
            {
                public MyProfile()
                {
                    CreateMap<User, UserDto>();
                }
            }

            public class User
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }

            public class UserDto
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }
            """;

        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("Map(User source)", generated);
        Assert.Contains("Id = source.Id", generated);
        Assert.Contains("Name = source.Name", generated);
    }

    [Fact]
    public void ProfileDefinition_ConstructorMapping()
    {
        var source = """
            using FlowMapper.Abstractions;
            using FlowMapper.Core;

            public class MyProfile : ProfileDefinition
            {
                public MyProfile()
                {
                    CreateMap<Source, Dest>();
                }
            }

            public class Source
            {
                public int Id { get; set; }
                public string Name { get; set; } = "";
            }

            public class Dest(int id, string name)
            {
                public int Id { get; } = id;
                public string Name { get; } = name;
            }
            """;

        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("new Dest(", generated);
    }

    [Fact]
    public void ProfileDefinition_FlattenMapping()
    {
        var source = """
            using FlowMapper.Abstractions;
            using FlowMapper.Core;

            public class MyProfile : ProfileDefinition
            {
                public MyProfile()
                {
                    CreateMap<Employee, EmployeeDto>();
                }
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
            }

            public class EmployeeDto
            {
                public string Name { get; set; } = "";
                public string AddressStreet { get; set; } = "";
                public string AddressCity { get; set; } = "";
            }
            """;

        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("Address.Street", generated);
    }

    [Fact]
    public void ProfileDefinition_NestedMapping()
    {
        var source = """
            using FlowMapper.Abstractions;
            using FlowMapper.Core;

            public class MyProfile : ProfileDefinition
            {
                public MyProfile()
                {
                    CreateMap<User, UserDto>();
                }
            }

            public class User
            {
                public int Id { get; set; }
                public Address Address { get; set; } = new();
            }

            public class Address
            {
                public string Street { get; set; } = "";
            }

            public class UserDto
            {
                public int Id { get; set; }
                public AddressDto Address { get; set; } = new();
            }

            public class AddressDto
            {
                public string Street { get; set; } = "";
            }
            """;

        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("MapAddress", generated);
    }

    [Fact]
    public void ProfileMapping_SetsProfileName()
    {
        var source = """
            using FlowMapper.Abstractions;

            [FlowProfile("MyProfile")]
            [Map<A, B>]
            public partial class ProfileMapper : IMapper<A, B>;

            public class A
            {
                public int X { get; set; }
            }

            public class B
            {
                public int X { get; set; }
            }
            """;

        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("FlowProfile", generated);
        Assert.Contains("MyProfile", generated);
    }

    [Fact]
    public void AfterMap_MethodGroup_GeneratesCallbackCall()
    {
        var source = """
            using FlowMapper.Abstractions;
            using FlowMapper.Core;

            public class MyProfile : ProfileDefinition
            {
                public MyProfile()
                {
                    CreateMap<Order, OrderDto>()
                        .AfterMap(CalculateTotals);
                }
            }

            public class Order
            {
                public int Id { get; set; }
                public decimal Price { get; set; }
                public int Quantity { get; set; }
            }

            public class OrderDto
            {
                public int Id { get; set; }
                public decimal Total { get; set; }
            }
            """;

        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("CalculateTotals(source, target)", generated);
    }

    [Fact]
    public void AfterMap_LambdaInline_GeneratesBodyDirectly()
    {
        var source = """
            using FlowMapper.Abstractions;
            using FlowMapper.Core;

            public class MyProfile : ProfileDefinition
            {
                public MyProfile()
                {
                    CreateMap<Order, OrderDto>()
                        .AfterMap((source, target) => target.Total = source.Price * source.Quantity);
                }
            }

            public class Order
            {
                public int Id { get; set; }
                public decimal Price { get; set; }
                public int Quantity { get; set; }
            }

            public class OrderDto
            {
                public int Id { get; set; }
                public decimal Total { get; set; }
            }
            """;

        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("target.Total = source.Price * source.Quantity;", generated);
    }

    [Fact]
    public void ConstructUsing_MethodGroup_GeneratesFactoryCall()
    {
        var source = """
            using FlowMapper.Abstractions;
            using FlowMapper.Core;

            public class MyProfile : ProfileDefinition
            {
                public MyProfile()
                {
                    CreateMap<Input, Output>()
                        .ConstructUsing(CreateOutput);
                }
            }

            public class Input
            {
                public int Value { get; set; }
            }

            public class Output
            {
                public int Value { get; }
                public Output(int value) => Value = value;
            }
            """;

        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("var target = CreateOutput(source);", generated);
    }

    [Fact]
    public void ConstructUsing_LambdaInline_GeneratesBodyDirectly()
    {
        var source = """
            using FlowMapper.Abstractions;
            using FlowMapper.Core;

            public class MyProfile : ProfileDefinition
            {
                public MyProfile()
                {
                    CreateMap<Input, Output>()
                        .ConstructUsing(source => new Output(source.Value * 2));
                }
            }

            public class Input
            {
                public int Value { get; set; }
            }

            public class Output
            {
                public int Value { get; }
                public Output(int value) => Value = value;
            }
            """;

        var (diagnostics, generated) = GeneratorTestHelper.RunGenerator(source);

        Assert.Empty(diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
        Assert.Contains("var target = new Output(source.Value * 2);", generated);
    }
}
