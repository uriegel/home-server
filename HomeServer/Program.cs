using CsTools;
using CsTools.Extensions;
using WebServerLight;
using WebServerLight.Routing;

using static System.Console;

WriteLine(@"Test site:  http://localhost:5050");

var server =
    ServerBuilder
        .New()
        .Http(5050)
        .Route(PathRoute
                .New("/media")
                .Add(MethodRoute
                    .New(Method.Get)
                    .Request(GetMediaFile)
                    .Request(GetMedia)))
        .AddAllowedOrigin("http://localhost:5050")
        .UseRange()
        .Build();
    
server.Start();
ReadLine();
server.Stop();

// TODO read config from environment 
// TODO check on raspi

async Task<bool> GetMedia(IRequest request)
{
    var path = "/daten/Videos".AppendPath(request.SubPath);
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

async Task<bool> GetMediaFile(IRequest request)
{
    var path = "/daten/Videos".AppendPath(request.SubPath);
    if (request.SubPath?.Contains('.') != true || !File.Exists(path))
        return false;
    WriteLine($"GetMediaFile: {path}, {File.Exists(path)}");
    using var video = File.OpenRead(path);
    if (video != null)
    {
        await request.SendAsync(video, video.Length, MimeType.Get(".mp4") ?? MimeTypes.TextPlain);
        return true;
    }
    else
        return false;
}
record DirectoryContent(string[] Directories, string[] Files);