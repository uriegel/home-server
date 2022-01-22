use std::path::PathBuf;

use tokio::runtime::Runtime;
use warp::{serve, path::{tail, Tail}, Filter, fs::dir};
use warp_range::filter_range;

use crate::{
    warp_utils::{
        simple_file_send, add_headers
    }, requests::{
        get_video_list, get_video, get_directory, get_music
    }
};

pub fn start_http_server(rt: &Runtime, port: u16, lets_encrypt_dir: &PathBuf, host: &str, video_path: &str, music_path: &str) {

    let host_and_port = format!("{}:{port}", host);
    let video_path_clone = video_path.to_string();
    let route_get_video_list = 
        warp::host::exact(&host_and_port)
        .and(warp::path("media"))
        .and(warp::path("video"))
        .and(warp::path("list"))
        .and(warp::path::end())
        .and(warp::any().map(move || { video_path_clone.to_string() }))
        .and_then(get_video_list);    

    let video_path_clone = video_path.to_string();        
    let route_get_video =
        warp::host::exact(&host_and_port) 
        .and(warp::path("media"))
        .and(warp::path("video"))
        .and(warp::path::param())
        .and(warp::any().map(move || { video_path_clone.to_string() }))
        .and(filter_range())
        .and_then(get_video);

    let music_path_clone = music_path.to_string();
    let route_music_directories =
        warp::host::exact(&host_and_port) 
        .and(warp::path("media"))
        .and(warp::path("music"))
        .and(warp::path::tail().map(|n: Tail| {n.as_str().to_string()}))
        .and(warp::any().map(move || { music_path_clone.to_string() }))
        .and_then(get_directory);        

    let music_path_clone = music_path.to_string();            
    let route_music =
        warp::host::exact(&host_and_port) 
        .and(warp::path("media"))
        .and(warp::path("music"))
        .and(warp::path::tail().map(|n: Tail| {n.as_str().to_string()}))
        .and(warp::any().map(move || { music_path_clone.to_string() }))
        .and(filter_range())
        .and_then(get_music);
            
    let acme_challenge = lets_encrypt_dir.join("acme-challenge");
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
        dir("webroot")
        .map(add_headers);

    let routes = 
        route_get_video_list
        .or(route_get_video)        
        .or(route_music_directories)
        .or(route_music)
        .or(route_acme)
        .or(route_static);    

    rt.spawn(async move {
        serve(routes)
            .run(([0, 0, 0, 0], port))
            .await;         
    });

    println!("http server started on {port}");        
}
