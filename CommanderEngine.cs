using LinqTools;

static class CommanderEngine
{
    public static Task<RemoteItem[]> GetFiles(Input input)
        => input.path.With(
                p => new DirectoryInfo(p),
                i => i
                        .GetDirectories()
                        .Select(d => new RemoteItem(
                            d.Name,
                            0,
                            true,
                            (i.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden || d.Name.StartsWith('.'),
                            (new DateTimeOffset(d.LastWriteTime).ToUnixTimeMilliseconds())))
                    .Concat(i
                        .GetFiles()
                        .Select(f => new RemoteItem(
                            f.Name,
                            f.Length,
                            false,
                            (f.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden || f.Name.StartsWith('.'),
                            (new DateTimeOffset(f.LastWriteTime).ToUnixTimeMilliseconds()))))
                    .ToArray()
                    .ToAsync());

    public record Input(string path);
    public record RemoteItem(
        string Name,
        long Size,
        bool IsDirectory,
        bool IsHidden,
        long Time
    );
}