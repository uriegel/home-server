pub async fn mount_device(media_path: &str, media_mount_path: String, usb_media_port: u16) {
    println!("mounting {media_path} to access {media_mount_path} port {usb_media_port}");
    //TODO check first video file exists
    // var text = await Process.RunAsync("uhubctl", $"-p {usbMediaPort} -a 1 -l 1-1");
    // Console.WriteLine($"uhubctl executed {text}");
    // try 
    // {
    //     Console.WriteLine("Mounting...");
    //     text = await Process.RunAsync("mount", mountPath);
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

// TODO Check if file is accessed: lsof -t /home/uwe/Videos/Vietnam1.mp4 | wc -w