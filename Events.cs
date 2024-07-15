enum DirectoryChangedType
{
    Created,
    Changed,
    Renamed,
    Deleted
}

record DirectoryItem(
    string Name,
    long Size,
    bool IsDirectory,
    string? IconPath,
    bool IsHidden,
    DateTime Time
){
    public static DirectoryItem CreateDirItem(DirectoryInfo info)
        => new(
            info.Name,
            0,
            true,
            null,
            (info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
            info.LastWriteTime);

    public static DirectoryItem CreateFileItem(FileInfo info)
        => new(
            info.Name,
            info.Length,
            false,
            GetIconPath(info),
            (info.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden,
            info.LastWriteTime);
    
    public static string GetIconPath(FileInfo info)
        => info.Extension?.Length > 0 ? info.Extension : ".noextension";
};


record DirectoryChangedEvent(
    string FolderId,
    string? Path,
    DirectoryChangedType Type,
    DirectoryItem Item,
    string? OldName
);

record Events(
    DirectoryChangedEvent? DirectoryChanged
) {
    public static void SendDirectoryChanged(string folderId, string? path, DirectoryChangedType type, DirectoryItem item, string? oldName = null) {}
};
