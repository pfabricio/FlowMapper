using System.Security.Cryptography;
using System.Text;
using FlowMapper.Core;

namespace FlowMapper.Compiler.Compilation;

public sealed record CompilationKey(
    Type ComponentType,
    string? ProfileName,
    string? ProviderName,
    string Hash)
{
    public static CompilationKey Create<TComponent>(
        IReadOnlyList<ProfileDefinition> profiles,
        string? providerName = null)
    {
        var hash = ComputeHash(profiles);
        return new CompilationKey(typeof(TComponent), null, providerName, hash);
    }

    public static CompilationKey CreateForProfile<TComponent>(
        string profileName,
        IReadOnlyList<ProfileDefinition> profiles,
        string? providerName = null)
    {
        var hash = ComputeHash(profiles);
        return new CompilationKey(typeof(TComponent), profileName, providerName, hash);
    }

    public static string ComputeHash(IReadOnlyList<ProfileDefinition> profiles)
    {
        var sb = new StringBuilder();
        foreach (var profile in profiles.OrderBy(p => p.ProfileName))
        {
            sb.Append(profile.ProfileName);
            foreach (var reg in profile.Registrations.OrderBy(r => r.SourceType.Name))
            {
                sb.Append(reg.SourceType.FullName);
                sb.Append(reg.DestinationType.FullName);
            }
        }
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
