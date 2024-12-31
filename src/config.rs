use std::{env, path::PathBuf, error::Error};

#[derive(Clone)]
pub struct Config {
    pub port: u16,
    pub tls_port: u16,
    pub lets_encrypt_dir: PathBuf,
    pub media_mount_path: String,
    pub usb_media_port: u16,
    pub video_path: String,
    pub picture_path: String,
    pub music_path: String,
    pub intranet_host: String
}

impl Config {
    pub fn get() -> Result<Config, Box<dyn Error>> {
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
    
        let picture_path = env::var("PICTURE_PATH").map_err(|_| { "Please specify PICTURE_PATH" })?;
        println!("picture path: {picture_path}");

        let intranet_host = env::var("INTRANET_HOST").map_err(|_| { "Please specify INTRANET_HOST" })?;
        println!("intranet host: {intranet_host}");

        Ok(Config { port, tls_port, lets_encrypt_dir, media_mount_path, usb_media_port, video_path, picture_path, music_path, intranet_host})
    }
}
