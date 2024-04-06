using CsTools.Async;
using CsTools.Extensions;
using CsTools.Functional;
static class Tracker
{
    public static Task Ping(HttpContext context)
        => context.Request
            .ReadFromJsonAsync<PingInput>()
            .AsTask()
            .SelectMany(n => context.Response.WriteAsJsonAsync(new PingOutput(n?.Input ?? "Empty")).ToUnit());
}

record PingInput(string Input);
record PingOutput(string Output);