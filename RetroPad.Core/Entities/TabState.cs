namespace RetroPad.Core.Entities;

public class TabState
{
    public string DocumentId { get; set; } = string.Empty;
    public string FileName { get; set; } = "Untitled";
    public string FilePath { get; set; } = string.Empty;
    public string Language { get; set; } = "PlainText";
    public int CursorOffset { get; set; }
    public double ScrollOffset { get; set; }
    public bool IsActive { get; set; }
    public bool IsModified { get; set; }
}
