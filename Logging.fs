module Logging

open Microsoft.Extensions.Logging

let configureLogging (builder : ILoggingBuilder) =
    // Set a logging filter (optional)
    let filter l = l.Equals LogLevel.Warning

    // Configure the logging factory
    builder.AddFilter(filter) // Optional filter
           .AddConsole()      // Set up the Console logger
           .AddDebug()        // Set up the Debug logger

           // Add additional loggers if wanted...
    |> ignore