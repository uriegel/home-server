use std::cmp::min;

use async_stream::try_stream;
use chrono::Utc;
use futures_util::Stream;
use tokio::{fs::File, io::{AsyncRead, AsyncReadExt}};
use warp::{http::header::HeaderValue, hyper::Body, reply::{Reply, Response}};

pub mod error;

pub fn add_headers(mut response: Response)->Response {
    let now = Utc::now().to_rfc2822(); // Current date in RFC 2822 format
    response.headers_mut().insert("Date", HeaderValue::from_str(&now).unwrap());
    response.headers_mut().insert("Expires", HeaderValue::from_str(&now).unwrap());
    response.headers_mut().insert("Server", HeaderValue::from_str("Uwes Home Server").unwrap());
    response
}

// pub async fn simple_file_send(filename: String) -> Result<impl warp::Reply, warp::Rejection> {
//     // Serve a file by asynchronously reading it by chunks using tokio-util crate.

//     if let Ok(file) = tokio::fs::File::open(filename).await {
//         let stream = tokio_util::codec::FramedRead::new(file, tokio_util::codec::BytesCodec::new());
//         let body = Body::wrap_stream(stream); // Convert to Bytes
//         return Ok(warp::http::Response::new(body));
//     } else {
//         Ok(warp::http::Response::builder()
//             .status(404)
//             .body(Body::empty())
//             .unwrap()
//         )
//     }
// }
pub async fn simple_file_send(filename: String)->Result<impl Reply, warp::Rejection> {
    async fn download_file(filename: String)->Result<Response, error::Error> 
    {
        let file = File::open(filename).await?;
        get_file(file, None).await
    }
    
    download_file(filename).await.into_response()
}

pub async fn get_file(file: File, headers: Option<Vec<(&str, &str)>>)->Result<Response, error::Error> {
    let metadata = file.metadata().await?;
    if metadata.is_dir() {
        // TODO return warp_utils::error::Error::not_found()
        return Ok(warp::http::Response::builder()
            .status(404)
            .body(hyper::Body::empty())
            .unwrap());
    }
    let byte_count = metadata.len();
    let stream = get_response_stream(file, byte_count);
    let response_builder = warp::http::Response::builder()
        .header("Content-Length", byte_count.to_string());
    let response_builder = match headers {
        Some(headers) => headers
                                            .iter()
                                            .fold(response_builder, |response_builder, header|response_builder.header(header.0, header.1)),
        None => response_builder 
    };
    let response = response_builder
        .body(hyper::Body::wrap_stream(stream))
        .into_response();
    Ok(response)
}

pub fn get_response_stream(mut stream: impl AsyncRead + Unpin, byte_count: u64) -> impl Stream<Item = tokio::io::Result<Vec<u8>>> {
    try_stream! {
        let bufsize = 16384;
        let cycles = byte_count / bufsize as u64 + 1;
        let mut sent_bytes: u64 = 0;
        for _ in 0..cycles {
            let mut buffer: Vec<u8> = vec![0; min(byte_count - sent_bytes, bufsize) as usize];
            let bytes_read = stream.read_exact(&mut buffer).await?;
            sent_bytes += bytes_read as u64;
            yield buffer;
        }
    }
}

pub trait ResultExt {
    fn into_response(self)->Result<Response, warp::Rejection>;
}

impl ResultExt for Result<Response, error::Error> {
    fn into_response(self)->Result<Response, warp::Rejection> {
        match self {
            Ok(reply) => Ok(reply.into_response()),
            Err(_e) => Ok(warp::http::Response::builder()
                                .status(404)
                                .body(Body::empty())
                                .unwrap())
        }
    }
}

    