use std::{fs, path::Path};

use hyper::{Response, Body};
use lexical_sort::natural_lexical_cmp;
use serde::Serialize;
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

struct VideoFile {
    path: String,
    media_type: String

}

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

pub async fn get_video_list(path: String)->Result<VideoList, warp::Rejection> {
    let entries = fs::read_dir(&path).map_err(reject)?;
    let mut files: Vec<String> = entries.filter_map(|n| {
        n.ok().and_then(|n| {
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

pub async fn get_video(file: String, path: String, range: Option<String>) -> Result<impl warp::Reply, warp::Rejection> {
    async fn get_video_range(file: String, path: String, range: Option<String>) -> Result<impl warp::Reply, warp::Rejection> {
        let video = get_video_file(&path, file)?;
        get_range(range, &video.path, &video.media_type).await
    }

    match get_video_range(file.clone(), path.clone(), range.clone()).await {
        Ok(res) => Ok(res),
        Err(_err) => get_video_range(file, path, range).await
    }
}

pub async fn get_directory(path: String, root_path: String)->Result<DirectoryList, warp::Rejection> {
    let path = percent_encoding::percent_decode(path.as_bytes()).decode_utf8().unwrap().replace("+", " ");
    let path = &(root_path + "/" + &path);
    if path.ends_with("mp3") {
        Err(warp::reject())
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

pub async fn get_music(path: String, root_path: String, range: Option<String>)->Result<impl warp::Reply, warp::Rejection> {
    let path = &(root_path.clone() + "/" + &path);
    if path.ends_with("mp3") {
        let range_file = get_music_file(&root_path, path.clone());
        get_range(range, &range_file.path, &range_file.media_type).await
    } else {
        Err(warp::reject())
    }
}

fn reject(err: std::io::Error)->warp::Rejection {
    println!("Could not get video list: {err:?}");
    warp::reject()
}

fn get_video_file(path: &str, file: String) -> Result<VideoFile, warp::Rejection> {
    fn combine_path(path: &str, file: String, ext: &str)->Option<String> {
        let file_with_ext = file + ext;
        let file = percent_encoding::percent_decode(file_with_ext.as_bytes()).decode_utf8().unwrap();
        let path = Path::new(&path).join(file.to_string());
        println!("Serving video {file}");

        if path.exists() { Some(path.to_string_lossy().to_string())} else { None }
    }
    
    if let Some(mp4) = combine_path(path, file.clone(),&".mp4") {
        Ok(VideoFile{ path: mp4, media_type: "video/mp4".to_string() })
    } else {
        if let Some(mkv) = combine_path(path, file, &".mkv") {
            Ok(VideoFile{ path: mkv, media_type: "video/mp4".to_string() })
        } else {
            println!("Could not access video file");
            Err(warp::reject())
            //panic!("not mp4 and not mkv")
        }
    }
}

fn get_music_file(path: &str, file: String)->RangeFile {
    let file = percent_encoding::percent_decode(file.as_bytes())
        .decode_utf8()
        .unwrap()
        .replace("+", " ");
    let path = Path::new(&path).join(file.to_string());
    let path = path.to_string_lossy().to_string();
    RangeFile{ path, media_type: "audio/mp3".to_string() }
}