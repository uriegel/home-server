static class CommanderEngine
{
    public static async Task<RemoteItem[]> GetFiles(Input input)
        => new RemoteItem[] 
            {
                new("Hallo", 9, false, false, 1231),
                new("Hallo 5", 9, false, false, 4567)
            };

    public record Input(string path);
    public record RemoteItem(
        string Name,
        long Size,
        bool IsDirectory,
        bool IsHidden,
        long Time
    );
}