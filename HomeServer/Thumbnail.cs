using GtkDotNet;

static class Thumbnail
{
    public static Stream? Get(string filename)
    {
        var pb = Pixbuf.NewFromFile(filename);
        var (w, h) = Pixbuf.GetFileInfo(filename);
        var newh = SIZE * h / w;
        var thumbnail = Pixbuf.Scale(pb, SIZE, newh, Interpolation.Bilinear);
        return Pixbuf.SaveJpgToBuffer(thumbnail);
    }

    const int SIZE = 128;
}

