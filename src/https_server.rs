use std::path::Path;

use hyper::{header::HOST, HeaderMap};
use tokio::runtime::Runtime;
use warp::{serve, Filter, fs::dir};
use warp_reverse_proxy::{proxy_to_and_forward_response, extract_request_data_filter};

use crate::warp_utils::add_headers;

pub fn start_https_server(rt: &Runtime, port: u16) -> bool {

    let route_static = 
        dir("webroot")
        .map(add_headers);

    let request_filter = extract_request_data_filter();
    let fritz_proxy = warp::header::exact("Host", "fritz.uriegel.de")
        .map(|| ("http://fritz.box/".to_string(), "".to_string()))
        .untuple_one()
        .and(request_filter)
        .and_then(|pa, bp, uri, ps, m, headers: HeaderMap, body| {
            let mut proxy_headers = headers.clone();
            proxy_headers.remove(HOST);
            proxy_headers.insert(HOST, "fritz.box".parse().unwrap());
            proxy_to_and_forward_response(pa, bp, uri, ps, m, proxy_headers, body)
        });

    let routes = fritz_proxy.or(route_static);    

    let config_dir = dirs::config_dir().expect("Could not find config dir")
        .join("letsencrypt-certiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiiii");
    let cert_file = config_dir.join("cert.pem");
    let key_file = config_dir.join("key.pem");

    if Path::new(&cert_file).exists() && Path::new(&key_file).exists() {
        rt.spawn(async move {
            serve(routes)
            .tls()
            .cert_path(cert_file)
            .key_path(key_file)
            .run(([0, 0, 0, 0], port))
            .await;         
        });

        println!("https server started on {port}");        
        true
    } else {
        println!("https server could not be started, no certificate");        
        false 
    }
}

