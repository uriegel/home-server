use tokio::runtime::Runtime;
use warp::{filters::{fs::{dir, File}, path::{tail, Tail}}, reply::Reply, Filter};
use warp_range::filter_range;

use crate::{
    config::Config, remote::{download_file, get_files, get_metadata, upload_file}, requests::{
        access_media, disk_needed, get_media_list, get_picture, get_thumbnail, get_video, simple_file_send
    }, warp_utils::add_headers
};

pub fn start_http_server(rt: &Runtime, config: Config) {

    let video_path = config.video_path.to_string();
    let route_get_video_list = 
        warp::path("video")
        .and(warp::path::tail())
        .and(warp::any().map(move ||video_path.to_string()))
        .and_then(get_media_list)
        .with(warp::compression::gzip());    

    let video_path = config.video_path.to_string();
    let route_get_video =
        warp::path("video")
        .and(warp::path::tail())
        .and(warp::any().map(move ||video_path.to_string()))
        .and(filter_range())
        .and_then(get_video);
        
    let picture_path = config.picture_path.to_string();
    let route_get_picture_list =
        warp::path("pics")
        .and(warp::path::tail())
        .and(warp::any().map(move ||picture_path.to_string()))
        .and_then(get_media_list)
        .with(warp::compression::gzip());    

    let picture_path = config.picture_path.to_string();
    let route_get_picture =
        warp::path("pics")
        .and(warp::path::tail())
        .and(warp::any().map(move ||picture_path.to_string()))
        .and_then(get_picture);
        
    let picture_path = config.picture_path.to_string();
    let route_get_thumbnail =
        warp::path("thumbnail")
        .and(warp::path::tail())
        .and(warp::any().map(move ||picture_path.to_string()))
        .and_then(get_thumbnail);

    let music_path = config.music_path.to_string();
    let route_get_music_list = 
        warp::path("music")
        .and(warp::path::tail())
        .and(warp::any().map(move ||music_path.to_string()))
        .and_then(get_media_list)
        .with(warp::compression::gzip());    

    let music_path = config.music_path.to_string();
    let route_get_music =
        warp::path("music")
        .and(warp::path::tail())
        .and(warp::any().map(move ||music_path.to_string()))
        .and(filter_range())
        .and_then(get_video);
    
    let video_path_clone = config.video_path.to_string();        
    let media_mount_path_clone = config.media_mount_path.to_string();        
    let route_media_access =
        warp::path("accessdisk")
        .and(warp::any().map(move ||video_path_clone.to_string()))
        .and(warp::any().map(move ||media_mount_path_clone.to_string()))
        .and(warp::any().map(move ||config.usb_media_port.clone()))
        .and_then(access_media);

    let route_media_disk_needed =
        warp::path("diskneeded")
        .and_then(disk_needed);

    async fn not_found()->Result<impl warp::Reply, warp::Rejection> {
        println!("Accessing media device");
        // mount_device(&path, media_mount_path, usb_media_port).await;
        Ok("Disk accessed not found".to_string())
    }
        
    let route_not_found =
        warp::path::end()
        .and_then(not_found);

            // let dir = warp::fs::dir("webroot");
    // let with_headers = dir.map(|reply| {
    //     let mut response = reply.into_response();
    //     let now = chrono::Utc::now().to_rfc2822(); // Current date in RFC 2822 format
    //     response.headers_mut().insert(
    //         "Date",
    //         HeaderValue::from_str(&now).unwrap(),
    //     );
    //     response        
    // });

    let route_get_files =
        warp::path("getfiles")
        .and(tail())
        .and_then(get_files);

    let route_download_file =
        warp::path("downloadfile")
        .and(tail())
        .and_then(download_file);
        
    let route_upload_file =
        warp::put()
        .and(warp::path("putfile"))
        .and(tail())
        .and(warp::body::stream())
        .and(warp::header::optional::<i64>("x-file-date"))
        .and_then(upload_file);

    let route_get_file =
        warp::path("getfile")
        .and(tail())
        .and_then(download_file);

    let route_get_metadata =
        warp::path("metadata")
        .and(tail())
        .and_then(get_metadata);

    let route_static = 
        dir("webroot")
            .map(|r: File|r.into_response())
            .map(add_headers);

    let host_and_port = format!("{}:{}", config.intranet_host, config.port);
    let route_host_media = 
        warp::host::exact(&host_and_port) 
        .and(warp::path("media"));
    
    let host_and_port = format!("{}:{}", config.intranet_host, config.port);
    let route_host = 
        warp::host::exact(&host_and_port); 

    let acme_challenge = config.lets_encrypt_dir.join("acme-challenge");
    let route_acme = 
        warp::path(".well-known")
        .and(warp::path("acme-challenge"))
        .and(tail().map(move|token: Tail| {
            let token_path = acme_challenge.join(token.as_str().to_string()).to_str().expect("Could not create token path").to_string();
            println!("Serving lets encrypt token: {token_path}");
            token_path
        }))
        .and_then(simple_file_send);

    let routes =
        route_host_media
            .and(route_get_video
                .or(route_get_picture)        
                .or(route_get_thumbnail)
                .or(route_get_music)        
                .or(route_get_video_list)        
                .or(route_get_picture_list)
                .or(route_get_music_list)
                .or(route_media_disk_needed)
                .or(route_media_access)
                .or(route_not_found)
            )
        .or(route_host
            .and(route_get_files    
                .or(route_get_file)    
                .or(route_download_file)
                .or(route_upload_file)
                .or(route_get_metadata)
            )
        )
        .or(route_acme)
        .or(route_static);    

    rt.spawn(async move {
        warp::serve(routes)
            .run(([0, 0, 0, 0], config.port))
            .await;         
    });

    println!("http server started on {}", config.port);        
}

