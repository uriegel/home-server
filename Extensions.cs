using LinqTools;

static class Extensions
{
    public static string ReadAllTextFromFilePath(this string path)
        => new StreamReader(File.OpenRead(path))
            .Use(f => f.ReadToEnd());
}