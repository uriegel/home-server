using CsTools.Extensions;
using LinqTools;
using static Configuration;
using static Extensions;

static class Requests
{
    public static Task ServeVideo(HttpContext context)
        => GetEnvironmentVariable(VideoPath)
            .GetOrDefault("")
            .AppendPath(context.GetRouteValue("path") as string ?? "")
            .Choose(
                Switch(IsDirectory, ServeVideoDirectory),
                Switch(IsFile, ServeVideoFile),
                Default(_ => NotFound(context)))
            .GetOrDefault(1.ToAsync());

    static bool IsDirectory(string path) => Directory.Exists(path).SideEffect(Console.WriteLine);
    static bool IsFile(string path) => File.Exists(path).SideEffect(Console.WriteLine);

    static Task ServeVideoDirectory(string path) => "Ist Verzeichnis".SideEffect(Console.WriteLine).ToAsync();
    static Task ServeVideoFile(string path) => "Ist Datei".SideEffect(Console.WriteLine).ToAsync();

    public static Func<Predicate<string>, Func<string, Task>, SwitchType<Task, string>> Switch 
        = Extensions.SwitchType<Task, string>.Switch;
    public static Func<Func<string, Task>, SwitchType<Task, string>> Default 
        = Extensions.SwitchType<Task, string>.Default;
}