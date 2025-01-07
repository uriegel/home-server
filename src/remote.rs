use std::{fs::{self, read_dir}, time::{SystemTime, UNIX_EPOCH}};

use chrono::DateTime;
use futures_util::{Stream, StreamExt};
use serde::Serialize;
use tokio::{fs::{metadata, File}, io::AsyncWriteExt};
use tokio_util::bytes::Buf;
use warp::{filters::path::Tail, reply::Reply};

use crate::{requests::{decode_path, reject}, warp_utils::{error::Error, get_file}};

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
    let metadata = metadata(&path).await.map_err(Error::from_io)?;
    let modified = metadata.modified()
                        .ok()
                        .and_then(|t|t.duration_since(UNIX_EPOCH).ok())
                        .map(|d|d.as_millis() as i64)
                        .unwrap_or(0); 
    Ok(get_file(&path, Some(vec!(("x-file-date", &modified.to_string())))).await?)
    // TODO warp::Rejection instead of warp_utils::error::Error
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