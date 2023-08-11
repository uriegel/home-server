using LinqTools;
using CsTools.Extensions;

static class Requests
{
    public static WebApplicationWithHost LetsEncrypt(this WebApplicationWithHost app)
        => app.SideEffect(_ => app.WithMapGet("/.well-known/acme-challenge/{secret}", 
                                                (string secret) => getFileContent($"{secret}")));

    static Func<string, string> getFileContent = name => 
        new StreamReader(File.OpenRead(Configuration
                                        .GetEnvironmentVariable("LETS_ENCRYPT_DIR")
                                        .GetOrDefault("")
                                        .AppendPath(name)))
            .Use(f => f.ReadToEnd());
}