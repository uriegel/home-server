use std::{fs::{self, ReadDir}, process::Output};

use tokio::{process::Command, io};

pub async fn mount_device(media_path: &str, media_mount_path: String, usb_media_port: u16) {

    match access_first_file(media_path) {
        true => println!("media device is accessible"),
        false => mount_device(media_path, media_mount_path, usb_media_port).await
    }

    fn access_first_file(media_path: &str) -> bool {
        fn get_first_file(read_dir: ReadDir) -> Option<bool> {
            //file_exists
            Some(false)
        }

        fs::read_dir(media_path)
        .ok()
        .and_then(get_first_file)
        .unwrap_or(false)
    }

    async fn mount_device(media_path: &str, media_mount_path: String, usb_media_port: u16) {
        println!("mounting {media_path} to access {media_mount_path} port {usb_media_port}");
    
        power_on(usb_media_port).await;
        // try 
        // {
        //     Console.WriteLine("Mounting...");
        let res = access_first_file(media_path);


        async fn power_on(usb_media_port: u16) {
            match Command::new("uhubctl")
            .arg("-p")
            .arg(format!("{usb_media_port}"))
            .arg("-a")
            .arg("1")
            .arg("-l")
            .arg("1-1")
            .output().await {
                Ok(output) => println!("uhubctl executed {}", String::from_utf8(output.stdout).unwrap()),
                Err(err) => println!("Could not power on usb: {err}")
            }
        }
                //     Console.WriteLine($"mount executed {text}");
        // }
        // catch(Exception e)
        // {
        //     Console.WriteLine($"mount error: {e}");
        //     await Task.Delay(2000);
        //     text = await Process.RunAsync("mount", mountPath);
        //     Console.WriteLine($"mount executed (2nd time) {text}");
        // }
    
        // Console.WriteLine("Retrying after mount");
        // return await function();    

    }
}

// TODO Check if file is accessed: lsof -t /home/uwe/Videos/Vietnam1.mp4 | wc -w