use std::env;

use warp::Filter;

use crate::requests::get_video_list;

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
    
    let route_get_video_list = 
        warp::path("video")
        .and(warp::path("list"))
        .and(warp::path::end())
        .and(warp::query::query())
        .and(warp::any().map(move || { video_path.to_string() }))
        .and_then(get_video_list);

    let routes = route_get_video_list;

    warp::serve(routes)
        .run(([127, 0, 0, 1], port))
        .await;        
}