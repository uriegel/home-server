using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UwebServer;
using UwebServer.Routes;

var videoPath = Environment.GetEnvironmentVariable("VIDEO_PATH");
Console.WriteLine($"Using video path: {videoPath}");

var routeVideoList = new JsonRest("/media/video/list", _ =>
{
    var di = new DirectoryInfo(videoPath);
    var files = from n in di.EnumerateFiles()
                orderby n.Name
                select n.Name;
    return Task.FromResult<object>(new DirectoryList(files));
});

var routeVideoServer = new VideoServer(videoPath);

var server = new Server(new Settings()
{
    Port = 9865,
    Routes = new Route[]
    {
        routeVideoList,
        routeVideoServer
    } 
});

server.Start();
Console.ReadLine();
server.Stop();

record DirectoryList(IEnumerable<string> Files);

class VideoServer : Route
{
    public VideoServer(string videoPath)
    {
        Path = "/media/video";
        this.videoPath = videoPath;
    }

    public override async Task ProcessAsync(IRequest request, IRequestHeaders headers, Response response)
    {
        var path = headers.Url[(Path.Length+1)..];
        var query = new UrlComponents(path);
        var file = Uri.UnescapeDataString(query.Path);
        var mp4 = System.IO.Path.Combine(videoPath, file + ".mp4");
        if (File.Exists(mp4))
            await response.SendFileAsync(mp4);
        else
        {
            var mkv = System.IO.Path.Combine(videoPath, file + ".mkv");
            await response.SendFileAsync(mkv);
        }

    }

    readonly string videoPath;
}