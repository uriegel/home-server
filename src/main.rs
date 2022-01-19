mod http_server;
mod https_server;
mod warp_utils;
mod requests;

use std::{env, thread};

use http_server::start_http_server;
use https_server::start_https_server;
use signal_hook::{iterator::Signals, consts::{SIGINT, SIGTERM}};
use tokio::{runtime::Runtime};

fn main() {
    println!("starting home server...");

    let port_string = 
        env::var("SERVER_PORT")
        .or::<String>(Ok("8080".to_string()))
        .unwrap();
    let port = 
        port_string.parse::<u16>()
        .or::<u16>(Ok(8080))
        .unwrap();

    let tls_port_string = 
        env::var("TLS_SERVER_PORT")
        .or::<String>(Ok("4433".to_string()))
        .unwrap();
    let tls_port = 
        tls_port_string.parse::<u16>()
        .or::<u16>(Ok(4433))
        .unwrap();

    let video_path = 
        env::var("VIDEO_PATH")
        .or::<String>(Ok("/home/uwe/Videos".to_string()))
        .unwrap();        

    println!("video path: {video_path}");

    let rt = Runtime::new().unwrap();
    start_http_server(&rt, port, &video_path);
    start_https_server(&rt, tls_port);

    println!("Home server started");

    let mut signals = Signals::new(&[SIGINT, SIGTERM]).unwrap();
    let shutdown_listener = thread::spawn(move || {
        for sig in signals.forever() {
            println!("Received signal {:?}", sig);
            break;
        }
    });    
    shutdown_listener.join().unwrap();

    println!("Stopping home server...");
    println!("Home server stopped");
}





