use std::{fs::{self, ReadDir}};

use tokio::{process::Command};

pub async fn mount_device(media_path: &str, media_mount_path: String, usb_media_port: u16) {

    match access_first_file(media_path) {
        true => println!("media device is accessible"),
        false => mount_device(media_path, media_mount_path, usb_media_port).await
    }

    fn access_first_file(media_path: &str) -> bool {
        fn get_first_file(mut read_dir: ReadDir) -> Option<bool> {
            if let Some(val) = 
                read_dir
                .next()
                .and_then(|file|{
                    file.and_then(|file| { 
                        Ok(file.path().exists())
                    }).ok()
                }) { Some(val)}
            else { None }
        }

        fs::read_dir(media_path)
        .ok()
        .and_then(get_first_file)
        .unwrap_or(false)
    }

    async fn mount_device(media_path: &str, media_mount_path: String, usb_media_port: u16) {
        println!("mounting {media_path} to access {media_mount_path} port {usb_media_port}");
    
        power_on(usb_media_port).await;
        mount(&media_mount_path).await;
        let res = access_first_file(media_path);

        async fn power_on(usb_media_port: u16) {
            println!("Power on usb hub...");
            match Command::new("uhubctl")
            .arg("-p")
            .arg(format!("{usb_media_port}"))
            .arg("-a")
            .arg("1")
            .arg("-l")
            .arg("1-1")
            .output().await {
                Ok(output) => println!("Usb hub powered on {}", String::from_utf8(output.stdout).unwrap()),
                Err(err) => println!("Could not power on usb hub: {err}")
            }
        }

        async fn mount(media_mount_path: &str) -> Option<()> {
            println!("Mounting media device...");
            match Command::new("mount")
            .arg(media_mount_path)
            .output().await {
                Ok(output) => {
                    println!("Mounted {}", String::from_utf8(output.stdout).unwrap());
                    Some(())
                },
                Err(err) => {
                    println!("Could not mount: {err}");
                    None
                }
            }
        }
// }
        // catch(Exception e)
        // {
        //     await Task.Delay(2000);
        //     text = await Process.RunAsync("mount", mountPath);
        //     Console.WriteLine($"mount executed (2nd time) {text}");
        // }
    }

}

// TODO Check if file is accessed: lsof -t /home/uwe/Videos/Vietnam1.mp4 | wc -w