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
        // .Route(SubpathRoute
        //         .New("/media")
        //         .Add(MethodRoute
        //             .New(Method.Get)
        //             .Request(GetMediaVideo)))

        .AddAllowedOrigin("http://localhost:5050")
        .UseRange()
        .Build();
    
server.Start();
ReadLine();
server.Stop();
