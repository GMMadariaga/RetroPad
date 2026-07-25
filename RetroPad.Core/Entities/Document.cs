namespace RetroPad.Core.Entities;

public class Document
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = "Untitled";
    public string Content { get; set; } = string.Empty;
    public string Language { get; set; } = "PlainText";
    public int CursorOffset { get; set; }
    public double ScrollOffset { get; set; }
    public bool IsModified { get; set; }
    public DateTime LastModified { get; set; } = DateTime.UtcNow;

    public bool HasFilePath => !string.IsNullOrEmpty(FilePath);

    public string DisplayName => IsModified ? $"{FileName}*" : FileName;

    public void MarkModified()
    {
        IsModified = true;
        LastModified = DateTime.UtcNow;
    }
}
