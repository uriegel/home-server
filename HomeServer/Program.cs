using CsTools;
using CsTools.Extensions;
using WebServerLight;
using WebServerLight.Routing;

using static System.Console;

const string VIDEO_PATH = "VIDEO_PATH";
const string PICTURE_PATH = "PICTURE_PATH";
const string SERVER_PORT = "SERVER_PORT";
const string SERVER_TLS_PORT = "SERVER_TLS_PORT";

var videoPath = VIDEO_PATH.GetEnvironmentVariable().SideEffect(n => WriteLine($"VIDEO_PATH: {n}"));
var picturePath = PICTURE_PATH.GetEnvironmentVariable().SideEffect(n => WriteLine($"PICTURE_PATH: {n}"));
var port = (SERVER_PORT.GetEnvironmentVariable()?.ParseInt() ?? 80).SideEffect(n => WriteLine($"SERVER_PORT: {n}"));
var httpsPort = (SERVER_TLS_PORT.GetEnvironmentVariable()?.ParseInt() ?? 443).SideEffect(n => WriteLine($"SERVER_TLS_PORT: {n}"));

WriteLine(@$"Test site:  http://localhost:{port}");
WriteLine(@$"Test site:  https://localhost:{httpsPort}");

ManualResetEvent shutdownEvent = new(false);

CancelKeyPress += (sender, eventArgs) =>
{
    eventArgs.Cancel = true;
    shutdownEvent.Set();
};

AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) => shutdownEvent.Set();

var server =
    ServerBuilder
        .New()
        .Http(port)
        .Https(httpsPort)
        .UseLetsEncrypt()
        .UseRange()
        .Route(HttpRoute
            .New()
            .Add(PathRoute
                .New("/media/video")
                .Add(MethodRoute
                    .New(Method.Get)
                    .Request(GetMediaFile)
                    .Request(GetMedia(videoPath))))
            .Add(PathRoute
                .New("/media/pics")
                .Add(MethodRoute
                    .New(Method.Get)
                    .Request(GetPictureFile)
                    .Request(GetMedia(picturePath))))
            .Add(PathRoute
                .New("/media/diskneeded")
                .Add(MethodRoute
                    .New(Method.Get)
                    .Request(SendOK)))
            .Add(PathRoute
                .New("/media/accessdisk")
                .Add(MethodRoute
                    .New(Method.Get)
                    .Request(SendOK))))
        .Route(HttpsRoute
            .New()
            .Add(MethodRoute
                .New(Method.Get)
                .Request(SendUnderConstruction)))
        .Build();
    
server.Start();
shutdownEvent.WaitOne(); // Wait until SIGINT/SIGTERM
server.Stop();

// TODO AccessDisk
// TODO DiskNeeded

async Task<bool> SendOK(IRequest request)
{
    await request.SendText("OK");
    return true;
}

async Task<bool> SendUnderConstruction(IRequest request)
{
    await request.SendText("Under construction...");
    return true;
}

Func<IRequest, Task<bool>> GetMedia(string? mediaPath)
{
    return GetMedia;

    async Task<bool> GetMedia(IRequest request)
    {
        var path = mediaPath.AppendPath(request.SubPath);
        if (request.SubPath?.Contains('.') == true)
            return false;
        WriteLine($"GetMedia: {path}");
        var info = new DirectoryInfo(path);
        if (!info.Exists)
            return false;
        var json = new DirectoryContent(
            [.. info.GetDirectories().Select(n => n.Name).OrderBy(n => n)],
            [.. info.GetFiles().Select(n => n.Name).OrderBy(n => n)]
        );
        await request.SendJsonAsync(json);
        return true;
    }
}

async Task<bool> GetMediaFile(IRequest request)
{
    var path = videoPath.AppendPath(request.SubPath);
    if (request.SubPath?.Contains('.') != true || !File.Exists(path))
        return false;
    WriteLine($"GetMediaFile: {path}, {File.Exists(path)}");
    using var video = File.OpenRead(path);
    if (video != null)
    {
        await request.SendAsync(video, video.Length, MimeType.Get(path.GetFileExtension()) ?? MimeTypes.TextPlain);
        return true;
    }
    else
        return false;
}

async Task<bool> GetPictureFile(IRequest request)
{
    var path = picturePath.AppendPath(request.SubPath);
    if (request.SubPath?.Contains('.') != true || !File.Exists(path))
        return false;
    WriteLine($"GetPictureFile: {path}, {File.Exists(path)}");
    using var pic = File.OpenRead(path);
    if (pic != null)
    {
        await request.SendAsync(pic, pic.Length, MimeType.Get(path.GetFileExtension()) ?? MimeTypes.ImageJpeg);
        return true;
    }
    else
        return false;
}

record DirectoryContent(string[] Directories, string[] Files);