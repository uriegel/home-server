use std::env;
use warp::{Filter, path::Tail};
use warp_range::{filter_range, with_partial_content_status};

use crate::requests::{get_directory, get_music, get_video, get_video_list, get_video_range};

mod requests;

#[tokio::main]
async fn main() {
    let port_string = 
        env::var("SERVER_PORT")
        .or::<String>(Ok("".to_string()))
        .unwrap();

    let port = 
        port_string.parse::<u16>()
        .or::<u16>(Ok(9865))
        .unwrap();

    let video_path = 
        env::var("VIDEO_PATH")
        .or::<String>(Ok("video".to_string()))
        .unwrap();

    let music_path = 
        env::var("MUSIC_PATH")
        .or::<String>(Ok("music".to_string()))
        .unwrap();
        
    println!("port: {}", port);
    println!("video path: {}", video_path);
    println!("music path: {}", music_path);
    
    let video_path_clone = video_path.clone();
    let route_get_video_list = 
        warp::path("video")
        .and(warp::path("list"))
        .and(warp::path::end())
        .and(warp::any().map(move || { video_path_clone.to_string() }))
        .and_then(get_video_list);

    let video_path_clone = video_path.clone();
    let route_get_video = 
        warp::path("video")
        .and(warp::path::param())
        .and(warp::any().map(move || { video_path_clone.to_string() }))
        .and_then(get_video);

    let route_get_video_range = 
        warp::path("video")
        .and(warp::path::param())
        .and(warp::any().map(move || { video_path.to_string() }))
        .and(filter_range())
        .and_then(get_video_range)
        .map(with_partial_content_status);

    let music_path_clone = music_path.clone();        
    let route_music_directories =
        warp::path("music")
        .and(warp::path::tail().map(|n: Tail| {n.as_str().to_string()}))
        .and(warp::any().map(move || { music_path_clone.to_string() }))
        .and_then(get_directory);

    let route_music =
        warp::path("music")
        .and(warp::path::tail().map(|n: Tail| {n.as_str().to_string()}))
        .and(warp::any().map(move || { music_path.to_string() }))
        .and_then(get_music);

    let routes = 
        route_get_video_list
        .or(route_get_video_range)
        .or(route_get_video)
        .or(route_music_directories)
        .or(route_music);

    warp::serve(routes)
        .run(([0, 0, 0, 0], port))
        .await;        
}