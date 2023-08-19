using AspNetExtensions;
using LinqTools;
using static LinqTools.ChooseExtensions;
using static Requests;

static class CommanderEngine
{
    public static Task<RemoteItem[]> GetFiles(Input input)
        => input.path.With(
                p => new DirectoryInfo(p),
                i => i
                        .GetDirectories()
                        .Select(d => new RemoteItem(
                            d.Name,
                            0,
                            true,
                            (i.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden || d.Name.StartsWith('.'),
                            (new DateTimeOffset(d.LastWriteTime).ToUnixTimeMilliseconds())))
                    .Concat(i
                        .GetFiles()
                        .Select(f => new RemoteItem(
                            f.Name,
                            f.Length,
                            false,
                            (f.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden || f.Name.StartsWith('.'),
                            (new DateTimeOffset(f.LastWriteTime).ToUnixTimeMilliseconds()))))
                    .ToArray()
                    .ToAsync());

    public static async Task GetFile(HttpContext context)                    
    {
        var path = await context.Request.ReadFromJsonAsync<CommanderEngine.Input>();
        await File
            .OpenRead(path!.path)
            .UseAsync(f => context.SendStream(f, null, path!.path));
    }

    public static Task PostFile(HttpContext context)
        => File
            .OpenWrite("/" + context.Request.Query["path"].ToString())
            .UseAsync(f => context.Request.Body.CopyToAsync(f));

            // TODO large files will not be copied
            // TODO copy file attributes for copy to remote and copy from remote

    public static Task Serve(HttpContext context)
        => ("/" + context.GetRouteValue("path") as string)
            .Choose(
                Switch(_ => true, p => p.SendFile(context)),
                Default(p => p.SendFile(context)))
            .GetOrDefault(1.ToAsync());                

    public record Input(string path);
    public record RemoteItem(
        string Name,
        long Size,
        bool IsDirectory,
        bool IsHidden,
        long Time
    );
}