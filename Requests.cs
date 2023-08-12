using AspNetExtensions;
using CsTools.Extensions;
using LinqTools;
using static Configuration;
using static Extensions;
using static LinqTools.ChooseExtensions;

static class Requests
{
    public static Task ServeVideo(HttpContext context)
        => GetEnvironmentVariable(VideoPath)
            .GetOrDefault("")
            .AppendPath(context.GetRouteValue("path") as string ?? "")
            .Choose(
                Switch(IsDirectory, p => ServeVideoDirectory(context, p)),
                Switch(IsFile, p => ServeVideoFile(context, p)),
                Default(_ => NotFound(context)))
            .GetOrDefault(1.ToAsync());

    static bool IsDirectory(string path) => Directory.Exists(path);
    static bool IsFile(string path) => File.Exists(path);

    static Task ServeVideoDirectory(HttpContext context, string path)
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

    static Task ServeVideoFile(HttpContext context, string path) 
        => context.StreamRangeFile(path);

    public static Func<Predicate<string>, Func<string, Task>, SwitchType<string, Task>> Switch 
        = SwitchType<string, Task>.Switch;
    public static Func<Func<string, Task>, SwitchType<string, Task>> Default 
        = SwitchType<string, Task>.Default;

    record DirectoryContent(string[] Directories, string[] Files);
}