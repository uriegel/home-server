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

var uploadVideoPath = Environment.GetEnvironmentVariable("UPLOAD_VIDEO_PATH");
Console.WriteLine($"Using video upload path: {uploadVideoPath}");

var uploadPath = Environment.GetEnvironmentVariable("UPLOAD_PATH");
Console.WriteLine($"Using upload path: {uploadPath}");

var port = Environment.GetEnvironmentVariable("SERVER_PORT");
var serverPort = int.TryParse(port, out var val) ? val : 80;
Console.WriteLine($"Using server port: {serverPort}");

var tlsPort = Environment.GetEnvironmentVariable("SERVER_TLS_PORT");
var serverTlsPort = int.TryParse(tlsPort, out var tlsval) ? tlsval : 443;
Console.WriteLine($"Using tls server port: {serverTlsPort}");

var authName = Environment.GetEnvironmentVariable("AUTH_NAME");
var authPw = Environment.GetEnvironmentVariable("AUTH_PW");

var fritzHost = Environment.GetEnvironmentVariable("FRITZ_HOST");

var intranetHost = Environment.GetEnvironmentVariable("INTRANET_HOST");

var basicAuthentication = new BasicAuthentication
{
    Realm = "Home Media Server",
    Name = authName,
    Password = authPw
};

JsonRest createRouteVideoList(string host, BasicAuthentication auth, bool isSecure)
    => new JsonRest("/media/video/list", _ =>
    {
        var di = new DirectoryInfo(videoPath);
        var files = from n in di.EnumerateFiles()
                    orderby n.Name
                    select n.Name;
        return Task.FromResult<object>(new DirectoryList(files));
    })
    {
        Host = host,
        Tls = isSecure ? true : null,
        BasicAuthentication = auth    
    };

JsonRest createRouteMusicList(string host, BasicAuthentication auth, bool isSecure)
    => new JsonRest("/media/music", input =>
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
    })
    {
        Host = host,
        Tls = isSecure ? true : null,
        BasicAuthentication = auth
    };

var routeVideoList = createRouteVideoList(intranetHost, null, false);

var routeMusicList = createRouteMusicList(intranetHost, null, false);

var routeVideoServer = new MediaServer("/media/video", videoPath, false, intranetHost, null, false);

var routeMusicServer = new MediaServer("/media/music", musicPath, true, intranetHost, null, false);

var routeUpload = new UploadRoute("/upload", uploadPath) { Host = intranetHost };

var routeVideoUpload = new UploadRoute("/uploadvideo", uploadVideoPath) { Host = intranetHost };

var routeStatic = new Static() 
{ 
    FilePath = "webroot",
    Host = intranetHost
};

var routeVideoListInternet = createRouteVideoList(null, basicAuthentication, true);

var routeMusicListInternet = createRouteMusicList(null, basicAuthentication, true);

var routeVideoServerInternet = new MediaServer("/media/video", videoPath, false, null, basicAuthentication, true);

var routeMusicServerInternet = new MediaServer("/media/music", musicPath, true, null, basicAuthentication, true);

var routeLetsEncrypt = new LetsEncrypt();

var routeFritz = new ReverseProxy("http://fritz.box")
{
    Tls = true,
    Host = fritzHost
};

var server = new Server(new Settings()
{
    Port = serverPort,
    TlsPort = serverTlsPort,
    IsTlsEnabled = true,
    Routes = new Route[]
    {
        routeVideoList,
        routeVideoServer,
        routeMusicList,
        routeMusicServer,
        routeVideoUpload,
        routeUpload,
        routeLetsEncrypt,
        routeMusicListInternet,
        routeMusicServerInternet,
        routeVideoListInternet,
        routeVideoServerInternet,
        routeStatic,
        routeFritz
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
    public MediaServer(string path, string filePath, bool music, string host, BasicAuthentication auth, bool isSecure)
    {
        Path = path;
        this.filePath = filePath;
        this.music = music;
        Host = host;
        Tls = isSecure ? true : null;
        BasicAuthentication = auth;
    }

    public override async Task<bool> ProcessAsync(IRequest request, IRequestHeaders headers, Response response)
    {
        var query = new UrlComponents(headers.Url, Path);
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


// TODO: basic authentication
// TODO: lets encrypt