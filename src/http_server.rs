use tokio::runtime::Runtime;
use warp::{serve, path::{tail, Tail}, Filter, fs::dir};

use crate::warp_utils::{simple_file_send, add_headers};

pub fn start_http_server(rt: &Runtime) {
    // TODO directory!!
    let route_acme = 
        warp::path(".well-known")
        .and(warp::path("acme-challenge"))
        .and(tail().map(|n: Tail| { 
            let file = format!("/home/uwe/acme-challenge/{}", n.as_str().to_string());
            file 
        }))
        .and_then(simple_file_send);

    let route_static = 
        dir("webroot")
        .map(add_headers);

    let port = 8080;

    let routes = 
        route_acme
        .or(route_static);    

    rt.spawn(async move {
        serve(routes)
            .run(([0, 0, 0, 0], port))
            .await;         
    });
}
