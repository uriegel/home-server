use hyper::{Body, Response};
use lexical_sort::natural_lexical_cmp;
use serde::{Serialize};
use std::{fs, path::Path};
use warp::Reply;
use warp_range::get_range;

#[derive(Serialize)]
pub struct VideoList {
    files: Vec<String>
}

impl Reply for VideoList {
    fn into_response(self) -> Response<Body> { 
        let reply = warp::reply::json(&self);
        reply.into_response()
    }
}

fn reject(err: std::io::Error)->warp::Rejection {
    println!("Could not get video list: {:?}", err);
    warp::reject()
}

pub async fn get_video_list(path: String)->Result<VideoList, warp::Rejection> {
    let entries = fs::read_dir(&path).map_err(reject)?;
    let mut files: Vec<String> = entries.filter_map(|n| {
        n.ok()
            .and_then(|n| {
                let is_file = n.metadata().ok().and_then(|n|{
                    if n.is_file() { Some(()) } else { None }
                }).is_some();
                if is_file { Some(n.file_name().to_str()?.to_string())} else { None}
            })
    })
    .collect();
    files.sort_by(|a, b|natural_lexical_cmp(a, b));
    Ok(VideoList{ files })
}

// TODO: Content_type
pub async fn get_video(file: String, path: String) -> Result<impl warp::Reply, warp::Rejection> {
    get_range("".to_string(), &combine_path(path, file), "video/mp4").await
}

pub async fn get_video_range(file: String, path: String, range_header: String) -> Result<impl warp::Reply, warp::Rejection> {
    get_range(range_header, &combine_path(path, file), "video/mp4").await
}

fn combine_path(path: String, file: String)->String {
    let file = percent_encoding::percent_decode(file.as_bytes()).decode_utf8().unwrap();
    Path::new(&path).join(file.to_string()).to_str().unwrap().to_string()
}

