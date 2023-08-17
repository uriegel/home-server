using AspNetExtensions;
using CsTools.Extensions;
using LinqTools;
using static Configuration;
using static Extensions;
using static LinqTools.ChooseExtensions;

static class Requests
{
    public static Task ServeVideo(HttpContext context)
        => Serve(context, VideoPath, AspNetExtensions.Extensions.StreamRangeFile);

    public static Task ServePictures(HttpContext context)
        => Serve(context, PicturePath, SendFile);

    public static Task ServeMusic(HttpContext context)
        => Serve(context, MusicPath, AspNetExtensions.Extensions.StreamRangeFile);

    public static Task GetZipFile(HttpContext context, string file)
        => Serve(context, PicturePath.AppendPath(file), SendFile); 

    static Task Serve(HttpContext context, string environmentPath, Func<HttpContext, string, Task> serveFile)
        => GetEnvironmentVariable(environmentPath)
            .GetOrDefault("")
            .AppendPath(context.GetRouteValue("path") as string ?? "")
            .Choose(
                Switch(IsDirectory, p => ServeDirectory(context, p)),
                Switch(IsFile, p => serveFile(context, p)),
                Default(_ => NotFound(context)))
            .GetOrDefault(1.ToAsync());

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

    static Task SendFile(HttpContext context, string path)
        => File
            .OpenRead(path)
            .UseAsync(f => context.SendStream(f.SideEffect(f => Console.WriteLine(f.Length)), null, path.SideEffect(Console.WriteLine)));

    static Func<Predicate<string>, Func<string, Task>, SwitchType<string, Task>> Switch 
        = SwitchType<string, Task>.Switch;
    static Func<Func<string, Task>, SwitchType<string, Task>> Default 
        = SwitchType<string, Task>.Default;

    record DirectoryContent(string[] Directories, string[] Files);
}