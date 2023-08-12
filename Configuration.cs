using CsTools.Extensions;
using LinqTools;

using static CsTools.Functional.Memoization;

static class Configuration
{
    public const string LetsEncryptDir = "LETS_ENCRYPT_DIR";
    public const string ServerPort = "SERVER_PORT";
    public const string ServerTlsPort = "SERVER_TLS_PORT";
    public const string IntranetHost = "INTRANET_HOST";
    public const string VideoPath = "VIDEO_PATH";

    static Func<string, Option<string>, Option<string>> Init {get; } 
        = (key, _) => key.GetEnvironmentVariableWithLogging();

    public static Func<string, Option<string>> GetEnvironmentVariable { get; }
        = Memoize<string>(Init, false);

    static Option<string> GetEnvironmentVariableWithLogging(this string key)
        => key
            .GetEnvironmentVariable()
            .SideEffect(v => Console.WriteLine($"Reading environment {key}: {v}"));
}

