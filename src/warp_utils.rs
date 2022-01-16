use chrono::Utc;
use hyper::{Response, Body, HeaderMap, header::HeaderValue};
use warp::{fs::File, Reply};

pub fn add_headers(reply: File)->Response<Body> {
    let mut res = reply.into_response();
    let headers = res.headers_mut();
    let header_map = create_headers();
    headers.extend(header_map);
    res
}

pub async fn simple_file_send(filename: String) -> Result<impl warp::Reply, warp::Rejection> {
    // Serve a file by asynchronously reading it by chunks using tokio-util crate.

    if let Ok(file) = tokio::fs::File::open(filename).await {
        let stream = tokio_util::codec::FramedRead::new(file, tokio_util::codec::BytesCodec::new());
        let body = Body::wrap_stream(stream);
        return Ok(Response::new(body));
    }

    Ok(Response::new(Body::empty()))
    //Ok(not_found())
}

fn create_headers() -> HeaderMap {
    let mut header_map = HeaderMap::new();
    let now = Utc::now();
    let now_str = now.format("%a, %d %h %Y %T GMT").to_string();
    header_map.insert("Expires", HeaderValue::from_str(now_str.as_str()).unwrap());
    header_map.insert("Server", HeaderValue::from_str("Uwes Home Server").unwrap());
    header_map
}
