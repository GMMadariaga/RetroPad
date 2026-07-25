namespace RetroPad.Core.Entities;

public class AppConfig
{
    public string Theme { get; set; } = "retro-dark";
    public string FontFamily { get; set; } = "Cascadia Mono";
    public double FontSize { get; set; } = 14;
    public int TabSize { get; set; } = 4;
    public bool InsertSpaces { get; set; } = true;
    public bool RememberSession { get; set; } = true;
    public string LastDirectory { get; set; } = string.Empty;
    public string LastLanguage { get; set; } = "PlainText";
    public bool WordWrap { get; set; }
    public bool ShowLineNumbers { get; set; } = true;
    public int WindowX { get; set; }
    public int WindowY { get; set; }
    public int WindowWidth { get; set; } = 1200;
    public int WindowHeight { get; set; } = 800;
    public bool IsMaximized { get; set; }
}
