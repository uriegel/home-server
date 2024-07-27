using CsTools.Async;
using CsTools.Extensions;

static class SuperFit
{
    public static Task Login(HttpContext context)
        => context.Request
            .ReadFromJsonAsync<LoginInput>()
            .AsTask()
            .SelectMany(n => context.Response.WriteAsJsonAsync(Login(n?.AndroidId)).ToUnit());

    static LoginOutput Login(string? androidId)
    {
        Console.WriteLine($"Partner login: {androidId}");
        return new(true);
    }
}

