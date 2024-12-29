use tokio::runtime::Runtime;
use warp::{filters::{fs::{dir, File}, path::{tail, Tail}}, reply::Reply, Filter};
use warp_range::filter_range;

use crate::{config::Config, requests::{access_media, disk_needed, get_video, get_video_list}, warp_utils::{add_headers, simple_file_send}};

pub fn start_http_server(rt: &Runtime, config: Config) {

    let host_and_port = format!("{}:{}", config.intranet_host, config.port);
    let video_path = config.video_path.to_string();
    let route_get_video_list = 
        warp::host::exact(&host_and_port)
        .and(warp::path("media"))
        .and(warp::path("video"))
        .and(warp::path::tail())
        .and(warp::any().map(move || { video_path.to_string() }))
        .and_then(get_video_list)
        .with(warp::compression::gzip());    

    let video_path = config.video_path.to_string();
    let route_get_video =
        warp::host::exact(&host_and_port) 
        .and(warp::path("media"))
        .and(warp::path("video"))
        .and(warp::path::tail())
        .and(warp::any().map(move || { video_path.to_string() }))
        .and(filter_range())
        .and_then(get_video);
        

    // let music_path_clone = config.music_path.to_string();
    // let route_music_directories =
    //     warp::host::exact(&host_and_port) 
    //     .and(warp::path("media"))
    //     .and(warp::path("music"))
    //     .and(warp::path::tail().map(|n: Tail| {n.as_str().to_string()}))
    //     .and(warp::any().map(move || { music_path_clone.to_string() }))
    //     .and_then(get_directory);        

    // let music_path_clone = config.music_path.to_string();            
    // let route_music =
    //     warp::host::exact(&host_and_port) 
    //     .and(warp::path("media"))
    //     .and(warp::path("music"))
    //     .and(warp::path::tail().map(|n: Tail| {n.as_str().to_string()}))
    //     .and(warp::any().map(move || { music_path_clone.to_string() }))
    //     .and(filter_range())
    //     .and_then(get_music);
            
    let video_path_clone = config.video_path.to_string();        
    let media_mount_path_clone = config.media_mount_path.to_string();        
    let route_media_access =
        warp::host::exact(&host_and_port) 
            .and(warp::path("media"))
            .and(warp::path("accessdisk"))
            .and(warp::any().map(move || { video_path_clone.to_string() }))
            .and(warp::any().map(move || { media_mount_path_clone.to_string() }))
            .and(warp::any().map(move || { config.usb_media_port.clone() }))
            .and_then(access_media);

    let route_media_disk_needed =
        warp::host::exact(&host_and_port) 
            .and(warp::path("media"))
            .and(warp::path("diskneeded"))
            .and_then(disk_needed);

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

    // let dir = warp::fs::dir("webroot");
    // let with_headers = dir.map(|reply| {
    //     let mut response = reply.into_response();
    //     let now = chrono::Utc::now().to_rfc2822(); // Current date in RFC 2822 format
    //     response.headers_mut().insert(
    //         "Date",
    //         HeaderValue::from_str(&now).unwrap(),
    //     );
    //     response        
    // });

    let route_static = 
        warp::host::exact(&host_and_port) 
        .and(dir("webroot")
                .map(|r: File|r.into_response())
                .map(add_headers)
        );
    
    let routes =
        route_get_video
        .or(route_get_video_list)        
        // .or(route_music_directories)
        // .or(route_music)
        .or(route_media_disk_needed)
        .or(route_media_access)
        .or(route_acme)
        .or(route_static);    

    rt.spawn(async move {
        warp::serve(routes)
            .run(([0, 0, 0, 0], config.port))
            .await;         
    });

    println!("http server started on {}", config.port);        
}


