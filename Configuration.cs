using CsTools.Extensions;

using static CsTools.Functional.Memoization;

static class Configuration
{
    public const string LetsEncryptDir = "LETS_ENCRYPT_DIR";
    public const string ServerPort = "SERVER_PORT";
    public const string ServerTlsPort = "SERVER_TLS_PORT";
    public const string IntranetHost = "INTRANET_HOST";
    public const string VideoPath = "VIDEO_PATH";
    public const string PicturePath = "PICTURE_PATH";
    public const string MusicPath = "MUSIC_PATH";
    public const string UsbMediaPort = "USB_MEDIA_PORT";

    static Func<string, string?, string?> Init {get; } 
        = (key, _) => key.GetEnvironmentVariableWithLogging();

    public static Func<string, string?> GetEnvironmentVariable { get; }
        = Memoize(Init, false);

    static string? GetEnvironmentVariableWithLogging(this string key)
        => key
            .GetEnvironmentVariable()
            ?.SideEffect(v => Console.WriteLine($"Reading environment {key}: {v}"));
}

