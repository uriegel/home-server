use chrono::Utc;
use warp::{http::header::HeaderValue, hyper::Body, reply::Response};

pub fn add_headers(mut response: Response)->Response {
    let now = Utc::now().to_rfc2822(); // Current date in RFC 2822 format
    response.headers_mut().insert("Date", HeaderValue::from_str(&now).unwrap());
    response.headers_mut().insert("Expires", HeaderValue::from_str(&now).unwrap());
    response.headers_mut().insert("Server", HeaderValue::from_str("Uwes Home Server").unwrap());
    response
}

pub async fn simple_file_send(filename: String) -> Result<impl warp::Reply, warp::Rejection> {
    // Serve a file by asynchronously reading it by chunks using tokio-util crate.

    if let Ok(file) = tokio::fs::File::open(filename).await {
        let stream = tokio_util::codec::FramedRead::new(file, tokio_util::codec::BytesCodec::new());
        let body = Body::wrap_stream(stream); // Convert to Bytes
        return Ok(warp::http::Response::new(body));
    } else {
        Ok(warp::http::Response::builder()
            .status(404)
            .body(Body::empty())
            .unwrap()
        )
    }
}
