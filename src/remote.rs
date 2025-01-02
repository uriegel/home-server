use std::{fs::read_dir, time::UNIX_EPOCH};

use serde::Serialize;
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