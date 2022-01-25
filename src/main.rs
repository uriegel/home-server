mod http_server;
mod https_server;
mod warp_utils;
mod requests;
mod media_access;

use std::{env, thread, path::PathBuf, error::Error};

use http_server::start_http_server;
use https_server::start_https_server;
use signal_hook::{iterator::Signals, consts::{SIGINT, SIGTERM}};
use tokio::{runtime::Runtime};

fn main() -> Result<(), Box<dyn Error>> {
    println!("starting home server...");

    let port_string = env::var("SERVER_PORT").map_err(|_| { "Please specify SERVER_PORT" })?;
    let port = port_string.parse::<u16>().map_err(|_| { "Could not parse server port"})?;

    let tls_port_string = env::var("SERVER_TLS_PORT").or::<String>(Ok("0".to_string()))?;
    let tls_port = tls_port_string.parse::<u16>().map_err(|_| { "Could not parse server tls port"})?;

    let lets_encrypt_dir = PathBuf::from(env::var("LETS_ENCRYPT_DIR").map_err(|_| { "Please specify LETS_ENCRYPT_DIR" })?);
    println!("lets encrypt path: {lets_encrypt_dir:?}");

    let media_mount_path = env::var("MEDIA_MOUNT_PATH").map_err(|_| { "Please specify MEDIA_MOUNT_PATH"})?;
    println!("media mount path: {media_mount_path}");

    let usb_media_port_str = env::var("USB_MEDIA_PORT").or::<String>(Ok("0".to_string()))?;
    let usb_media_port = usb_media_port_str.parse::<u16>().map_err(|_| { "Could not parse usb media port" })?;
    println!("usb media port: {usb_media_port}");

    let video_path = env::var("VIDEO_PATH").map_err(|_| { "Please specify VIDEO_PATH" })?;
    println!("video path: {video_path}");

    let music_path = env::var("MUSIC_PATH").map_err(|_| { "Please specify MUSIC_PATH" })?;
    println!("music path: {music_path}");

    let intranet_host = env::var("INTRANET_HOST").map_err(|_| { "Please specify INTRANET_HOST" })?;
    println!("media host: {intranet_host}");

    let rt = Runtime::new()?;
    start_http_server(&rt, port, &lets_encrypt_dir, &intranet_host, usb_media_port, &media_mount_path, &video_path, &music_path);
    if tls_port > 0 { start_https_server(&rt, tls_port, &lets_encrypt_dir); }

    println!("Home server started");

    let mut signals = Signals::new(&[SIGINT, SIGTERM])?;
    let shutdown_listener = thread::spawn(move || {
        for sig in signals.forever() {
            println!("Received signal {sig:?}");
            break;
        }
    });    
    shutdown_listener.join().unwrap();

    println!("Stopping home server...");
    println!("Home server stopped");
    Ok(())
}





