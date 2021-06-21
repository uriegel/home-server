use hyper::{Body, Response};
use lexical_sort::natural_lexical_cmp;
use serde::{Serialize};
use std::{fs, path::Path};
use warp::{Reply, path::Tail};
use warp_range::get_range;

struct RangeFile {
    path: String,
    media_type: String

}

#[derive(Serialize)]
pub struct DirectoryList {
    files: Vec<String>
}

impl Reply for DirectoryList {
    fn into_response(self) -> Response<Body> { 
        let reply = warp::reply::json(&self);
        reply.into_response()
    }
}

fn reject(err: std::io::Error)->warp::Rejection {
    println!("Could not get video list: {:?}", err);
    warp::reject()
}

pub async fn get_directory(path: String, root_path: String)->Result<DirectoryList, warp::Rejection> {
    let path = &(root_path + "/" + &path);
    if path.ends_with("mp3") {
        Err(warp::reject())
        // let range_file = get_music_range_file(&root_path, &path);
        // get_range("".to_string(), &video.path, &video.media_type).await
    } else {
        let entries = fs::read_dir(path).map_err(reject)?;
        let mut files: Vec<String> = entries.filter_map(|n| {
            n.ok()
                .and_then(|n| { Some(n.file_name().to_str()?.to_string())})
        })
        .collect();
        files.sort_by(|a, b|natural_lexical_cmp(a, b));
        Ok(DirectoryList{ files })
    }
}

pub async fn get_music(path: String, root_path: String)->Result<impl warp::Reply, warp::Rejection> {
    let path = &(root_path.clone() + "/" + &path);
    if path.ends_with("mp3") {
        let range_file = get_music_range_file(&root_path, path.clone());
        get_range("".to_string(), &range_file.path, &range_file.media_type).await
    } else {
        Err(warp::reject())
    }
}

pub async fn get_video_list(path: String)->Result<DirectoryList, warp::Rejection> {
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
    Ok(DirectoryList{ files })
}

pub async fn get_video(file: String, path: String) -> Result<impl warp::Reply, warp::Rejection> {
    get_video_range_impl(file, path, "".to_string()).await
}

pub async fn get_video_range(file: String, path: String, range_header: String) -> Result<impl warp::Reply, warp::Rejection> {
    get_video_range_impl(file, path, range_header).await
}

async fn get_video_range_impl(file: String, path: String, range_header: String) -> Result<impl warp::Reply, warp::Rejection> {
    let video = get_range_file(&path, file);
    get_range(range_header, &video.path, &video.media_type).await
}

async fn get_music_range(file: String, path: String, range_header: String) -> Result<impl warp::Reply, warp::Rejection> {
    let video = get_range_file(&path, file);
    get_range(range_header, &video.path, &video.media_type).await
}


fn get_range_file(path: &str, file: String)->RangeFile {
    fn combine_path(path: &str, file: String, ext: &str)->Option<String> {
        let file_with_ext = file + ext;
        let file = percent_encoding::percent_decode(file_with_ext.as_bytes()).decode_utf8().unwrap();
        let path = Path::new(&path).join(file.to_string());
        if path.exists() { Some(path.to_string_lossy().to_string())} else { None }
    }
    
    if let Some(mp4) = combine_path(path, file.clone(),&".mp4") {
        RangeFile{ path: mp4, media_type: "video/mp4".to_string() }
    } else {
        if let Some(mkv) = combine_path(path, file, &".mkv") {
            RangeFile{ path: mkv, media_type: "video/mkv".to_string() }
        } else {
            // TODO 
            panic!("not mp4 and not mkv")
        }
    }
}

fn get_music_range_file(path: &str, file: String)->RangeFile {
    let file = percent_encoding::percent_decode(file.as_bytes()).decode_utf8().unwrap();
    let path = Path::new(&path).join(file.to_string());
    let path = path.to_string_lossy().to_string();
    RangeFile{ path, media_type: "audio/mp3".to_string() }
}

fn get_albums() {

}
