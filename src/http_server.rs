use tokio::runtime::Runtime;
use warp::{serve, path::{tail, Tail}, Filter, fs::dir};
use warp_range::{with_partial_content_status, filter_range};

use crate::{warp_utils::{simple_file_send, add_headers}, requests::{get_video_list, get_video_range, get_video}};

pub fn start_http_server(rt: &Runtime, port: u16, video_path: &str) {

    let video_path_clone = video_path.to_string();
    let route_get_video_list = 
        warp::path("media")
        .and(warp::path("video"))
        .and(warp::path("list"))
        .and(warp::path::end())
        .and(warp::any().map(move || { video_path_clone.to_string() }))
        .and_then(get_video_list);    

    let video_path_clone = video_path.to_string();        
    let route_get_video = 
        warp::path("media")
        .and(warp::path("video"))
        .and(warp::path::param())
        .and(warp::any().map(move || { video_path_clone.to_string() }))
        .and_then(get_video);

    let video_path_clone = video_path.to_string();
    let route_get_video_range = 
        warp::path("media")
        .and(warp::path("video"))
        .and(warp::path::param())
        .and(warp::any().map(move || { video_path_clone.to_string() }))
        .and(filter_range())
        .and_then(get_video_range)
        .map(with_partial_content_status);        

    let acme_challenge = dirs::config_dir().expect("Could not find config dir")
        .join("letsencrypt-cert")
        .join("acme-challenge");

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
        .or(route_get_video_range)
        .or(route_get_video)        .or(route_acme)
        .or(route_static);    

    rt.spawn(async move {
        serve(routes)
            .run(([0, 0, 0, 0], port))
            .await;         
    });

    println!("http server started on {port}");        
}
