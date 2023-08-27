using AspNetExtensions;
using CsTools.Extensions;
using LinqTools;
using static Configuration;
using static Extensions;
using static LinqTools.ChooseExtensions;

static class Requests
{
    public static Task ServeVideo(HttpContext context)
        => Serve(context, VideoPath, (p, c) => AspNetExtensions.Extensions.StreamRangeFile(c, p));

    public static Task ServePictures(HttpContext context)
        => Serve(context, PicturePath, SendFile);

    public static Task ServeMusic(HttpContext context)
        => Serve(context, MusicPath, (p, c) => AspNetExtensions.Extensions.StreamRangeFile(c, p));

    public static Task GetZipFile(HttpContext context, string file)
        => SendFile("/home/uwe".AppendPath(file), context); 

    public static Task SendFile(this string path, HttpContext context)
        => File
            .OpenRead(path)
            .UseAsync(f => context.SendStream(f, null, path));

    static Task Serve(HttpContext context, string environmentPath, Func<string, HttpContext, Task> serveFile)
        => GetEnvironmentVariable(environmentPath)
            .GetOrDefault("")
            .AppendPath(context.GetRouteValue("path") as string ?? "")
            .Choose(
                Switch(IsDirectory, p => ServeDirectory(context, p)),
                Switch(IsFile, p => serveFile(p, context)),
                Default(_ => NotFound(context)))
            .GetOrDefault(1.ToAsync());

    public static Func<Predicate<string>, Func<string, Task>, SwitchType<string, Task>> Switch 
        = SwitchType<string, Task>.Switch;
    public static Func<Func<string, Task>, SwitchType<string, Task>> Default 
        = SwitchType<string, Task>.Default;

    static bool IsDirectory(string path) => Directory.Exists(path);
    static bool IsFile(string path) => File.Exists(path);

    static Task ServeDirectory(HttpContext context, string path)
        => context.Response.WriteAsJsonAsync<DirectoryContent>(
                path
                    .With(
                        p => new DirectoryInfo(p),
                        i => new DirectoryContent(
                            i.GetDirectories()
                                .Select(n => n.Name)
                                .OrderBy(n => n)
                                .ToArray(),
                            i.GetFiles()
                                .Select(n => n.Name)
                                .OrderBy(n => n)
                                .ToArray()
                    )));

    record DirectoryContent(string[] Directories, string[] Files);
}