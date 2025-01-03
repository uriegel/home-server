use std::{cmp::min, fs::{self, read_dir}, time::{SystemTime, UNIX_EPOCH}};

use async_stream::stream;
use chrono::DateTime;
use futures_util::{Stream, StreamExt};
use serde::Serialize;
use tokio::{fs::File, io::{AsyncReadExt, AsyncWriteExt}};
use tokio_util::bytes::Buf;
use warp::{filters::path::Tail, reply::Reply};

use crate::requests::{decode_path, reject};

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
struct DirectoryItem {
    name: String,
    is_directory: bool,
    size: u64,  
    is_hidden: bool,
    time: i64
}

#[derive(Serialize)]
#[serde(rename_all = "camelCase")]
struct MetaData {
    size: i64, 
    time: i64
}

pub async fn get_files(path: Tail)->Result<impl Reply, warp::Rejection> {
    let items = read_dir(decode_path(format!("/{}", path.as_str()).as_str()))
        .map_err(|e|reject(e, "Could not get file list"))
        ?.filter_map(|file|file.ok())
        .filter_map(|file| {
            if let Ok(metadata) = file.metadata() {
                Some((file, metadata))
            } else {
                None
            }
        })        
        .map(|(entry, meta)| {
            let name = entry.file_name().to_string_lossy().to_string();
            let is_hidden = name.starts_with(".");
            DirectoryItem {
                name,
                is_directory: meta.is_dir(),
                is_hidden,
                size: meta.len(),
                time: meta
                        .modified()
                        .ok()
                        .and_then(|t|t.duration_since(UNIX_EPOCH).ok())
                        .map(|d|d.as_millis() as i64) 
                        .unwrap_or(0)
            }
        })
        .collect::<Vec<_>>();
    Ok(warp::reply::json(&items).into_response())
}

pub async fn download_file(path: Tail)->Result<impl Reply, warp::Rejection> {
    let path = decode_path(format!("/{}", path.as_str()).as_str());
    match File::open(&path).await {
        Ok(mut file) => {
            let metadata = file.metadata().await.unwrap();
            let modified = metadata.modified()
                                .ok()
                                .and_then(|t|t.duration_since(UNIX_EPOCH).ok())
                                .map(|d|d.as_millis() as i64)
                                .unwrap_or(0); 

            let byte_count = metadata.len();

            let stream = stream! {
                let bufsize = 16384;
                let cycles = byte_count / bufsize as u64 + 1;
                let mut sent_bytes: u64 = 0;
                for _ in 0..cycles {
                    let mut buffer: Vec<u8> = vec![0; min(byte_count - sent_bytes, bufsize) as usize];
                    let bytes_read = file.read_exact(&mut buffer).await.unwrap();
                    sent_bytes += bytes_read as u64;
                    yield Ok(buffer) as Result<Vec<u8>, warp::http::Error>;
                }
            };

            let response = warp::http::Response::builder()
                .header("x-file-date", modified.to_string())
                .header("Content-Length", byte_count.to_string())
                .body(hyper::Body::wrap_stream(stream))
                .unwrap();
            Ok(response)
        }
        Err(_) => Err(warp::reject::not_found())
    }
}

pub async fn get_metadata(path: Tail)->Result<impl Reply, warp::Rejection> {
    let path = decode_path(format!("/{}", path.as_str()).as_str());
    let metadata = fs::metadata(path).ok().map(|m|(
        m.len() as i64, 
        m.modified()
            .ok()
            .and_then(|t|t.duration_since(UNIX_EPOCH).ok())
            .map(|d|d.as_millis() as i64)
            .unwrap_or(0)))
        .unwrap_or((-1, 0));
    Ok(warp::reply::json(&MetaData{size: metadata.0, time: metadata.1 }).into_response())
}

pub async fn upload_file(path: Tail, mut body: impl Stream<Item = Result<impl Buf, warp::Error>> + Unpin + Send + Sync, modified: Option<i64>)->Result<impl Reply, warp::Rejection> {
    let path = decode_path(format!("/{}",  path.as_str()).as_str());

    // TODO ensure that directory exists 

    let mut file = File::create(path).await.map_err(|e|reject(e, "Could not create file"))?;
    while let Some(buf) = body.next().await {
        let mut buf = buf.unwrap();
        file.write_all_buf( &mut buf).await.unwrap();
    }
    let file = file.into_std().await;
    modified
        .and_then(|m |DateTime::from_timestamp_millis(m))
        .map(|dt|SystemTime::from(dt))
        .inspect(|m|{ let _ = file.set_modified(*m); });

    Ok("file uploaded")
}

// TODO delete file