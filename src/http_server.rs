use hyper::{header::HOST, HeaderMap};
use tokio::runtime::Runtime;
use warp::{serve, path::{tail, Tail}, Filter, fs::dir};
use warp_reverse_proxy::{reverse_proxy_filter, proxy_to_and_forward_response, extract_request_data_filter};

use crate::warp_utils::{simple_file_send, add_headers};

pub fn start_http_server(rt: &Runtime) {

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

    let request_filter = extract_request_data_filter();
    let fritz_proxy = warp::header::exact("Host", "uriegel.de")
        .map(|| ("http://fritz.box/".to_string(), "".to_string()))
        .untuple_one()
        .and(request_filter)
        .and_then(|pa, bp, uri, ps, m, headers: HeaderMap, body| {
            let mut proxy_headers = headers.clone();
            proxy_headers.remove(HOST);
            proxy_headers.insert(HOST, "fritz.box".parse().unwrap());
            proxy_to_and_forward_response(pa, bp, uri, ps, m, proxy_headers, body)
        });

    let port = 8080;

    let routes = 
        route_acme
        .or(fritz_proxy)
        .or(route_static);    

    rt.spawn(async move {
        serve(routes)
            .run(([0, 0, 0, 0], port))
            .await;         
    });
}
