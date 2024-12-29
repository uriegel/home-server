use std::{fs, path::PathBuf};

use lexical_sort::natural_lexical_cmp;
use serde::Serialize;
use warp::{filters::path::Tail, reply::Response, Reply};
use warp_range::get_range_with_cb;

struct VideoFile {
    path: String,
    media_type: String
}

#[derive(Serialize)]
pub struct DirectoryList {
    directories: Vec<String>,
    files: Vec<String>
}

impl Reply for DirectoryList {
    fn into_response(self) -> Response { 
        let reply = warp::reply::json(&self);
        reply.into_response()
    }
}

pub async fn get_media_list(sub_path: Tail, path: String)->Result<impl Reply, warp::Rejection> {
    let path = PathBuf::from(path).join(decode_path(sub_path.as_str()));
    let entries = fs::read_dir(&path).map_err(reject)?;

    let dir_files = entries.filter_map(|e| {
        e.ok().map(|e|
            if e.metadata().ok().map(|e|e.is_dir()).unwrap_or(false) {
                FileDir { 
                    dir: Some(e.file_name().to_string_lossy().to_string()), 
                    file: None
                }
            } else {
                FileDir { 
                    dir: None, 
                    file: Some(e.file_name().to_string_lossy().to_string())
                }
            }
        )
    }).collect::<Vec<_>>();
    let mut directories = dir_files.iter().filter_map(|n|n.dir.clone()).collect::<Vec<_>>();
    let mut files = dir_files.iter().filter_map(|n|n.file.clone()).collect::<Vec<_>>();
    directories.sort_by(|a, b|natural_lexical_cmp(a, b));
    files.sort_by(|a, b|natural_lexical_cmp(a, b));
    Ok(DirectoryList{ directories, files }.into_response())
}

pub async fn get_video(sub_path: Tail, path: String, range: Option<String>) -> Result<impl warp::Reply, warp::Rejection> {
    let path = PathBuf::from(path).join(decode_path(sub_path.as_str()));
    let video = get_video_file(&path)?;
    get_range_with_cb(range, &video.path, &video.media_type, |_| { 
        // media_access::i_am_alive();
     }).await
}

pub async fn access_media(_path: String, _media_mount_path: String, _usb_media_port: u16)->Result<impl warp::Reply, warp::Rejection> {
    println!("Accessing media device");
    // mount_device(&path, media_mount_path, usb_media_port).await;
    Ok("Disk accessed".to_string())
}

pub async fn disk_needed()->Result<impl warp::Reply, warp::Rejection> {
    println!("Delaying disk shutdown");
    // mount_device(&path, media_mount_path, usb_media_port).await;
    Ok("Disk shutdown delayed".to_string())
}
    
fn reject(err: std::io::Error)->warp::Rejection {
    println!("Could not get video list: {err:?}");
    warp::reject()
}

fn get_video_file(path: &PathBuf) -> Result<VideoFile, warp::Rejection> {
    if path.is_file() {
        let path = path.to_string_lossy().to_string();
        println!("Serving video {path}");
        Ok(VideoFile{ path, media_type: "video/mp4".to_string() }) // TODO MediaTypes:"audio/mp3", mkv
    } else {
        Err(warp::reject())
    }
}

fn decode_path(file: &str)->String {
    percent_encoding::percent_decode(file.as_bytes())
        .decode_utf8()
        .unwrap()
        .replace("+", " ")
}

struct FileDir {
    file: Option<String>,
    dir: Option<String>
}