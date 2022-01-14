use std::env;

use chrono::Utc;
use hyper::{HeaderMap, header::HeaderValue, Response, Body};
use warp::{fs::{dir, File}, Reply, Filter};

pub fn add_headers(reply: File)->Response<Body> {
    let mut res = reply.into_response();
    let headers = res.headers_mut();
    let header_map = create_headers();
    headers.extend(header_map);
    res
}

fn create_headers() -> HeaderMap {
    let mut header_map = HeaderMap::new();
    let now = Utc::now();
    let now_str = now.format("%a, %d %h %Y %T GMT").to_string();
    header_map.insert("Expires", HeaderValue::from_str(now_str.as_str()).unwrap());
    header_map.insert("Server", HeaderValue::from_str("Uwes Home Server").unwrap());
    header_map
}

#[tokio::main]
async fn main() {
    let port_string = 
        env::var("SERVER_PORT")
        .or::<String>(Ok("8080".to_string()))
        .unwrap();
    let port = 
        port_string.parse::<u16>()
        .or::<u16>(Ok(9865))
        .unwrap();

    println!("port: {}", port);        
        
    let route_static = dir("webroot")
        .map(add_headers);

    let routes = route_static;

    warp::serve(routes)
        .run(([0, 0, 0, 0], port))
        .await; 
}
