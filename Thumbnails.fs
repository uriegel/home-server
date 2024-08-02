module Thumbnails

open GtkDotNet

let getThumbnail (filename: string) =
    let pb = Pixbuf.NewFromFile filename
    let struct (w, h) = Pixbuf.GetFileInfo filename
    let newh = 64 * h / w
    let thumbnail = Pixbuf.Scale (pb, 64, newh, Interpolation.Bilinear)
    Pixbuf.SaveJpgToBuffer thumbnail
