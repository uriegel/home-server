use std::fs::{self, ReadDir};

pub async fn mount_device(media_path: &str, media_mount_path: String, usb_media_port: u16) {

    match access_first_file(media_path) {
        true => println!("media device is accessible"),
        false => mount_device(media_path, media_mount_path, usb_media_port).await
    }

    fn access_first_file(media_path: &str) -> bool {
        fn get_first_file(read_dir: ReadDir) -> Option<bool> {
            //file_exists
            Some(true)
        }

        fs::read_dir(media_path)
        .ok()
        .and_then(get_first_file)
        .unwrap_or(false)
    }

    async fn mount_device(media_path: &str, media_mount_path: String, usb_media_port: u16) {
        println!("mounting {media_path} to access {media_mount_path} port {usb_media_port}");
    
        // var text = await Process.RunAsync("uhubctl", $"-p {usbMediaPort} -a 1 -l 1-1");
        // Console.WriteLine($"uhubctl executed {text}");
        // try 
        // {
        //     Console.WriteLine("Mounting...");
        let res = access_first_file(media_path);
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