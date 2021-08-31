using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UwebServer;
using UwebServer.Routes;

var videoPath = Environment.GetEnvironmentVariable("VIDEO_PATH");
Console.WriteLine($"Using video path: {videoPath}");

var musicPath = Environment.GetEnvironmentVariable("MUSIC_PATH");
Console.WriteLine($"Using music path: {musicPath}");

var port = Environment.GetEnvironmentVariable("SERVER_PORT");
var serverPort = int.TryParse(port, out var val) ? val : 9865;
Console.WriteLine($"Using server port: {serverPort}");

var routeVideoList = new JsonRest("/media/video/list", _ =>
{
    var di = new DirectoryInfo(videoPath);
    var files = from n in di.EnumerateFiles()
                orderby n.Name
                select n.Name;
    return Task.FromResult<object>(new DirectoryList(files));
});

var routeMusicList = new JsonRest("/media/music", input =>
{
    var path = input?.Path != null ? Path.Combine(musicPath, Uri.UnescapeDataString(input?.Path.Replace('+', ' '))) : musicPath;
    var di = new DirectoryInfo(path);
    var dirs = di.Exists 
        ? (from n in di.EnumerateDirectories()
           orderby n.Name
           select n.Name).ToArray()
        : null;
    var files = di.Exists 
        ? from n in di.EnumerateFiles()
          orderby n.Name
          select n.Name
        : null;
    if (dirs?.Length > 0)
        return Task.FromResult<object>(new DirectoryList(dirs));
    else if (files != null) 
        return Task.FromResult<object>(new DirectoryList(files));
    else
        return Task.FromResult<object>(null);
});

var routeVideoServer = new MediaServer("/media/video", videoPath, false);
var routeMusicServer = new MediaServer("/media/music", musicPath, true);

var server = new Server(new Settings()
{
    Port = serverPort,
    Routes = new Route[]
    {
        routeVideoList,
        routeVideoServer,
        routeMusicList,
        routeMusicServer
    } 
});

var stopEvent = new ManualResetEvent(false);
Native.signal(2, _ => 
{
    Console.WriteLine("Interrupt");
    stopEvent.Set();
});
Native.signal(15, _ => 
{
    Console.WriteLine("Terminate");
    stopEvent.Set();
});

server.Start();
stopEvent.WaitOne();
server.Stop();

record DirectoryList(IEnumerable<string> Files);

class MediaServer : Route
{
    public MediaServer(string path, string filePath, bool music)
    {
        Path = path;
        this.filePath = filePath;
        this.music = music;
    }

    public override async Task<bool> ProcessAsync(IRequest request, IRequestHeaders headers, Response response)
    {
        var path = headers.Url.Length > Path.Length+1 ? headers.Url[(Path.Length+1)..] : null;
        var query = new UrlComponents(path);
        var file = Uri.UnescapeDataString(query.Path.Replace('+', ' '));
        if (music)
            await response.SendFileAsync(System.IO.Path.Combine(filePath, file));
        else
        {
            var mp4 = System.IO.Path.Combine(filePath, file + ".mp4");
            if (File.Exists(mp4))
                await response.SendFileAsync(mp4);
            else
            {
                var mkv = System.IO.Path.Combine(filePath, file + ".mkv");
                await response.SendFileAsync(mkv);
            }
        }
        return true;
    }

    readonly string filePath;
    readonly bool music;
}

class Native
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet=CharSet.Auto)]
    public delegate void Callback(int code);

    [DllImport("libc", SetLastError = true)]
    public extern static int signal(int pid, Callback callback);
}


// TODO: Upload web site to an upload folder
// TODO: basic authentication
// TODO: lets encrypt