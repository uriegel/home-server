using AspNetExtensions;
using CsTools.Extensions;
using CsTools.Functional;
using GtkDotNet;

using static Configuration;
using static CsTools.Functional.ChooseExtensions;
using static Extensions;

static class Requests
{
    public static Task ServeVideo(HttpContext context)
        => Serve(context, VideoPath, (p, c) => AspNetExtensions.Extensions.StreamRangeFile(c, p));

    public static Task ServePictures(HttpContext context)
        => Serve(context, PicturePath, SendMedia);

    public static Task ServeThumbnail(HttpContext context)
        => GetEnvironmentVariable(PicturePath)
            .AppendPath(context.GetRouteValue("path") as string ?? "")
            .Choose(
                Switch(IsFile, p => p.SendThumbnail(context)),
                Default(_ => NotFound(context)))
            ?? 1.ToAsync();

    public static Task ServeMusic(HttpContext context)
        => Serve(context, MusicPath, (p, c) => AspNetExtensions.Extensions.StreamRangeFile(c, p));

    public static Task GetZipFile(HttpContext context, string file)
        => SendFile("/home/uwe".AppendPath(file), context);

    static Task SendMedia(this string path, HttpContext context)
        => path.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase)
            ? AspNetExtensions.Extensions.StreamRangeFile(context, path)
            : path.SendFile(context);
    
    public static Task SendFile(this string path, HttpContext context)
        => File
            .OpenRead(path)
            .UseAsync(f => context.SendStream(f, null, path));

    public static Task SendThumbnail(this string path, HttpContext context)
    {
        var stream = GetThumbnail(path);
        return stream != null
            ? context.SendStream(stream, null)
            : NotFound(context);
    }
    
    static Task Serve(HttpContext context, string environmentPath, Func<string, HttpContext, Task> serveFile)
        => GetEnvironmentVariable(environmentPath)
            .AppendPath(context.GetRouteValue("path") as string ?? "")
            .Choose(
                Switch(IsDirectory, p => ServeDirectory(context, p)),
                Switch(IsFile, p => serveFile(p, context)),
                Default(_ => NotFound(context)))
            ?? 1.ToAsync();

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
                            [.. i.GetDirectories()
                                .Select(n => n.Name)
                                .OrderBy(n => n)],
                            [.. i.GetFiles()
                                .Select(n => n.Name)
                                .OrderBy(n => n)]
                    )));

    static Stream? GetThumbnail(string filename)
    {
        var pb = Pixbuf.NewFromFile(filename);
        var (w, h) = Pixbuf.GetFileInfo(filename);
        var newh = 64 * h / w;
        var thumbnail = Pixbuf.Scale(pb, 64, newh, Interpolation.Bilinear);
        return Pixbuf.SaveJpgToBuffer(thumbnail);
    }

    record DirectoryContent(string[] Directories, string[] Files);
}