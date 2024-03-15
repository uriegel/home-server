using System.Security.Cryptography.X509Certificates;
using CsTools.Extensions;

using static CsTools.Functional.Memoization;
using static Configuration;

static class Certificate
{
    public static WebApplicationWithHost LetsEncrypt(this WebApplicationWithHost app)
        => app.SideEffect(_ => app.WithMapGet("/.well-known/acme-challenge/{secret}", GetFileContent));

    public static Func<X509Certificate2> Get { get; } = Memoize(InitCertificate, Resetter);

    static Resetter Resetter { get; } = new Resetter();
    static X509Certificate2 InitCertificate()
        => GetEnvironmentVariable(LetsEncryptDir)
            ?.AppendPath("certificate.pfx")
            ?.ReadCertificate()!;

    static string InitPfxPassword()
        => (OperatingSystem.IsLinux()
            ? "/etc"
            : Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData))
            ?.AppendPath("letsencrypt-uweb")
            ?.ReadAllTextFromFilePath()
            ?.Trim() 
            ?? "";

    static Func<string> GetPfxPassword { get; } = Memoize(InitPfxPassword);

    static X509Certificate2 ReadCertificate(this string fileName)
        => new(fileName, GetPfxPassword());

    static readonly Func<string, string?> GetFileContent = name =>
        GetEnvironmentVariable(LetsEncryptDir)
            ?.AppendPath(name)
            ?.ReadAllTextFromFilePath();

    static readonly Timer certificateResetter = new(
        _ => Resetter.Reset(), 
        null, 
        TimeSpan.FromDays(1), 
        TimeSpan.FromDays(1)
    );
}

