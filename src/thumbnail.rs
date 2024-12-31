use gdk_pixbuf::{InterpType, Pixbuf};

pub fn create_thumbnail(image: &str)->Option<Vec<u8>> {
    let pixbuf = Pixbuf::from_file(image).ok();
    let new_height = pixbuf.clone().map(|pb|{
        let w = pb.width() as f32;
        let h = pb.height() as f32;
        (64.0 * h / w) as i32
    }).unwrap_or(0);
    pixbuf
        .and_then(|pb|pb.scale_simple(64, new_height, InterpType::Bilinear))
        .and_then(|th|th.save_to_bufferv("jpeg", &[("quality", "50")]).ok())
}