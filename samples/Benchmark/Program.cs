using System.Diagnostics;
using FlowMapper.Abstractions;

Console.WriteLine("=== Performance Benchmark ===");
Console.WriteLine("Comparing manual mapping vs IMapper dispatch:\n");

var user = new User
{
    Id = 42,
    Name = "Frank",
    Email = "frank@example.com",
    Age = 30
};

var manualMapper = new ManualUserMapper();
var interfaceMapper = new InterfaceUserMapper();

const int iterations = 1_000_000;

// Warmup
manualMapper.Map(user);
interfaceMapper.Map(user);

// Manual mapping
var sw = Stopwatch.StartNew();
for (var i = 0; i < iterations; i++)
    manualMapper.Map(user);
sw.Stop();
Console.WriteLine($"Manual mapping ({iterations:N0} ops): {sw.ElapsedMilliseconds} ms");

// Interface dispatch (equivalent to generated IMapper)
sw.Restart();
for (var i = 0; i < iterations; i++)
    interfaceMapper.Map(user);
sw.Stop();
Console.WriteLine($"IMapper dispatch ({iterations:N0} ops): {sw.ElapsedMilliseconds} ms");

Console.WriteLine($"\nOverhead: {(double)sw.ElapsedMilliseconds / (double)(sw.ElapsedMilliseconds * 2):P0}");

// --- Benchmark models ---

public class ManualUserMapper
{
    public UserDto Map(User source) => new()
    {
        Id = source.Id,
        Name = source.Name,
        Email = source.Email,
        Age = source.Age
    };
}

public class InterfaceUserMapper : IMapper<User, UserDto>
{
    public UserDto Map(User source) => new()
    {
        Id = source.Id,
        Name = source.Name,
        Email = source.Email,
        Age = source.Age
    };
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
}
