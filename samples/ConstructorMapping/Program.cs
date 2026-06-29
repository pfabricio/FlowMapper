using FlowMapper.Abstractions;

Console.WriteLine("=== Constructor Mapping ===");
Console.WriteLine("Maps to immutable types via constructor parameters:");
Console.WriteLine("  record PersonDto(string Name, string Email)");
Console.WriteLine();

var mapper = new PersonToPersonDtoMapper();
var person = new Person
{
    Id = 1,
    Name = "Diana",
    Email = "diana@example.com"
};
var dto = mapper.Map(person);

Console.WriteLine($"PersonDto: Id={dto.Id}, Name={dto.Name}, Email={dto.Email}");

Console.WriteLine();
Console.WriteLine("=== ConstructUsing with Factory Method ===");
Console.WriteLine("ProfileDefinition now supports strongly-typed factory callbacks:");
Console.WriteLine("  CreateMap<Input, Output>()");
Console.WriteLine("      .ConstructUsing(source => new Output(source.Value * 2))");
Console.WriteLine();

// Equivalent runtime behavior:
var factoryMapper = new InputToOutputMapper();
var input = new Input { Value = 21 };
var output = factoryMapper.Map(input);
Console.WriteLine($"Output: Value={output.Value} (Input.Value={input.Value} * 2)");

// Immutable types (records, classes with init-only setters) are mapped
// via constructor when they have no public parameterless constructor.
// FlowMapper resolves the best constructor by matching parameter
// names and types against source properties.

public class PersonToPersonDtoMapper : IMapper<Person, PersonDto>
{
    public PersonDto Map(Person source) => new(
        Id: source.Id,
        Name: source.Name,
        Email: source.Email
    );
}

public class Person
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public record PersonDto(int Id, string Name, string Email);

// --- Input/Output with ConstructUsing ---
// Equivalent to what the source generator produces from:
//   CreateMap<Input, Output>().ConstructUsing(source => new Output(source.Value * 2))

public class InputToOutputMapper : IMapper<Input, Output>
{
    public Output Map(Input source)
    {
        var target = new Output(source.Value * 2);
        return target;
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
