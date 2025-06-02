using CsTools.Extensions;
using WebServerLight;
using WebServerLight.Routing;

using static System.Console;

WriteLine(@"Test site:  http://localhost:5050");

var server =
    ServerBuilder
        .New()
        .Http(5050)
        // .Route(MethodRoute
        //         .New(Method.Get)
        //         .Add(SubpathRoute
        //                 .New("/image")
        //                 .Request(GetImage))
        //         .Add(SubpathRoute
        //                 .New("/video")
        //                 .Request(GetVideo)))
        // .Route(MethodRoute
        //         .New(Method.Post)
        //         .Add(SubpathRoute
        //                 .New("/json/cmd4")
        //                 .Request(JsonPost4))
        //         .Add(SubpathRoute
        //                 .New("/json")
        //                 .Request(JsonPost)))
        .Route(SubpathRoute
                .New("/media")
                .Add(MethodRoute
                    .New(Method.Get)
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
    var info = new DirectoryInfo("/daten/Videos");
    var json = new DirectoryContent(
        [.. info.GetDirectories().Select(n => n.Name).OrderBy(n => n)],
        [.. info.GetFiles().Select(n => n.Name).OrderBy(n => n)]
    );
    await request.SendJsonAsync(json);
    return true;
}
record DirectoryContent(string[] Directories, string[] Files);