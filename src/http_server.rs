use tokio::runtime::Runtime;
use warp::{serve, path::{tail, Tail}, Filter, fs::dir};

use crate::warp_utils::{simple_file_send, add_headers};

pub fn start_http_server(rt: &Runtime, port: u16) {

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
        route_acme
        .or(route_static);    

    rt.spawn(async move {
        serve(routes)
            .run(([0, 0, 0, 0], port))
            .await;         
    });

    println!("http server started on {port}");        
}
