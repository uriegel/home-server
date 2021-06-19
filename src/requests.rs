use std::fs;
use hyper::{Body, Response};
use lexical_sort::natural_lexical_cmp;
use serde::{Serialize, Deserialize};
use warp::Reply;

#[derive(Deserialize)]
pub struct GetVideoList {
    
}

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

pub async fn get_video_list(param: GetVideoList, path: String)->Result<VideoList, warp::Rejection> {
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