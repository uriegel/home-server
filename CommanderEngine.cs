using AspNetExtensions;
using CsTools.Extensions;
using CsTools.Functional;
using CsTools.HttpRequest;
using static Requests;
using static CsTools.Core;

static class CommanderEngine
{
    public static AsyncResult<RemoteItem[], RequestError> GetFiles(string? path) 
        => Try(
            () => ("/" + path).With(
                p => new DirectoryInfo(p),
                i => i
                        .GetDirectories()
                        .Select(d => new RemoteItem(
                            d.Name,
                            0,
                            true,
                            (i.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden || d.Name.StartsWith('.'),
                            new DateTimeOffset(d.LastWriteTime).ToUnixTimeMilliseconds()))
                    .Concat(i
                        .GetFiles()
                        .Select(f => new RemoteItem(
                            f.Name,
                            f.Length,
                            false,
                            (f.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden || f.Name.StartsWith('.'),
                            new DateTimeOffset(f.LastWriteTime).ToUnixTimeMilliseconds())))
                    .ToArray()),
            CatchRequestError)
            .ToAsyncResult();

    public static async Task GetFile(HttpContext context)                    
    {
        var path = await context.Request.ReadFromJsonAsync<Input>();
        var fileDate = context.Response.Headers.TryAdd("x-file-date",
                            new DateTimeOffset(new FileInfo(path?.Path ?? "")
                                                    .LastWriteTime)
                                .ToUnixTimeMilliseconds()
                                .ToString());
        await File
            .OpenRead(path?.Path ?? "")
            .UseAsync(f => context.SendStream(f, null, path?.Path ?? ""));
    }

    public static async Task PostFile(HttpContext context)
    {
        var path = "/" +
                context
                    .Request
                    .Query["path"]
                    .ToString();
        await File
            .Create(path)
            .UseAsync(f =>
                context
                    .Request
                    .BodyReader
                    .CopyToAsync(f));
        context
            .Request
            .Headers["x-file-date"]
            .FirstOrDefault()
            ?.ParseLong()
            ?.SetLastWriteTime(path);
    }
    // TODO Funktionales ConfigureKestrel

    public static Task Serve(HttpContext context)
        => ("/" + context.GetRouteValue("path") as string)
            .Choose(
                Switch(_ => true, p => p.SendFile(context)),
                Default(p => p.SendFile(context)))
            ?? 1.ToAsync();                

    static void SetLastWriteTime(this long unixTime, string targetFilename)
        => File.SetLastWriteTime(targetFilename, unixTime.FromUnixTime());

    static RequestError CatchRequestError(Exception e)
        =>  e is DirectoryNotFoundException
            ? new RequestError(404, "Nicht gefunden")
            : e is UnauthorizedAccessException
            ? new RequestError(403, "Verboten")
            : new RequestError(400, "Abfragefehler");


    public record Input(string Path);
    public record RemoteItem(
        string Name,
        long Size,
        bool IsDirectory,
        bool IsHidden,
        long Time
    );
}