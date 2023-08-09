using CsTools.Extensions;
using LinqTools;
using CsTools.Functional;

using static CsTools.Functional.Memoization;

static class Configuration
{
    static Func<string, Option<string>, Option<string>> Init {get; } 
        = (key, _) => key.GetEnvironmentVariableWithLogging();
    public static Func<string, Option<string>> GetEnvironmentVariable { get; }
        = Memoize<string>(Init, false);
}

static class StringExtensions
{
    public static Option<string> GetEnvironmentVariableWithLogging(this string key)
        => key
            .GetEnvironmentVariableOption()
            .SideEffect(v => Console.WriteLine($"Reading environment {key}: {v}"));
}