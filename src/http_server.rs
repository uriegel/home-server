use tokio::runtime::Runtime;
use warp::{serve, path::{tail, Tail}, Filter, fs::dir};
use warp_range::filter_range;

use crate::{
    warp_utils::{
        simple_file_send, add_headers
    }, requests::{
        get_video_list, get_video, get_directory, get_music, access_media
    }, config::Config
};

pub fn start_http_server(rt: &Runtime, config: Config) {

    let host_and_port = format!("{}:{}", config.intranet_host, config.port);
    let video_path_clone = config.video_path.to_string();
    let route_get_video_list = 
        warp::host::exact(&host_and_port)
        .and(warp::path("media"))
        .and(warp::path("video"))
        .and(warp::path("list"))
        .and(warp::path::end())
        .and(warp::any().map(move || { video_path_clone.to_string() }))
        .and_then(get_video_list);    

    let video_path_clone = config.video_path.to_string();        
    let route_get_video =
        warp::host::exact(&host_and_port) 
        .and(warp::path("media"))
        .and(warp::path("video"))
        .and(warp::path::param())
        .and(warp::any().map(move || { video_path_clone.to_string() }))
        .and(filter_range())
        .and_then(get_video);

    let music_path_clone = config.music_path.to_string();
    let route_music_directories =
        warp::host::exact(&host_and_port) 
        .and(warp::path("media"))
        .and(warp::path("music"))
        .and(warp::path::tail().map(|n: Tail| {n.as_str().to_string()}))
        .and(warp::any().map(move || { music_path_clone.to_string() }))
        .and_then(get_directory);        

    let music_path_clone = config.music_path.to_string();            
    let route_music =
        warp::host::exact(&host_and_port) 
        .and(warp::path("media"))
        .and(warp::path("music"))
        .and(warp::path::tail().map(|n: Tail| {n.as_str().to_string()}))
        .and(warp::any().map(move || { music_path_clone.to_string() }))
        .and(filter_range())
        .and_then(get_music);
            
    let video_path_clone = config.video_path.to_string();        
    let media_mount_path_clone = config.media_mount_path.to_string();        
    let route_media_access =
        warp::host::exact(&host_and_port) 
        .and(warp::path("media"))
        .and(warp::path("access"))
        .and(warp::any().map(move || { video_path_clone.to_string() }))
        .and(warp::any().map(move || { media_mount_path_clone.to_string() }))
        .and(warp::any().map(move || { config.usb_media_port.clone() }))
        .and_then(access_media);

    let acme_challenge = config.lets_encrypt_dir.join("acme-challenge");
    let route_acme = 
        warp::path(".well-known")
        .and(warp::path("acme-challenge"))
        .and(tail().map(move|token: Tail| {
            let token_path = acme_challenge.join(token.as_str().to_string()).to_str().expect("Could not create token path").to_string();
            println!("Serving lets encrypt token: {token_path}");
            token_path
        }))
        .and_then(simple_file_send);

    let route_static = 
        warp::host::exact(&host_and_port) 
        .and(dir("webroot"))
        .map(add_headers);

    let routes = 
        route_get_video_list
        .or(route_get_video)        
        .or(route_music_directories)
        .or(route_music)
        .or(route_media_access)
        .or(route_acme)
        .or(route_static);    

    rt.spawn(async move {
        serve(routes)
            .run(([0, 0, 0, 0], config.port))
            .await;         
    });

    println!("http server started on {}", config.port);        
}


