using LinqTools;

static class Requests
{
    public static WebApplicationWithHost LetsEncrypt(this WebApplicationWithHost app)
        => app.SideEffect(_ => app.WithMapGet("/.well-known/acme-challenge/{secret}", (string secret) => $"{secret}"));
        //=> app.SideEffect(_ => app.WithMapGet("/.well-known/acme-challenge/{**secret}", (string secret) => $"{secret}"));
}