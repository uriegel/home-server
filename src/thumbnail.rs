use std::io::Cursor;
use image::{imageops::FilterType, ImageFormat, ImageReader};

pub fn create_thumbnail(image: &str)->Option<Vec<u8>> {
    ImageReader::open(image)
    .ok()
    .and_then(|ir|ir.decode().ok())
    .map(|i|i.resize(64, 64, FilterType::Lanczos3))
    .and_then(|r|{
        let mut bytes: Vec<u8> = Vec::new();
        let res = r.write_to(&mut Cursor::new(&mut bytes), ImageFormat::Jpeg).ok();
        if res.is_some() {
            Some(bytes)
        } else {
            None
        }
    })

    //None

    // let pixbuf = Pixbuf::from_file(image).ok();
    // let new_height = pixbuf.clone().map(|pb|{
    //     let w = pb.width() as f32;
    //     let h = pb.height() as f32;
    //     (64.0 * h / w) as i32
    // }).unwrap_or(0);
    // pixbuf
    //     .and_then(|pb|pb.scale_simple(64, new_height, InterpType::Bilinear))
    //     .and_then(|th|th.save_to_bufferv("jpeg", &[("quality", "50")]).ok())
}