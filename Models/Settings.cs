namespace WinDash2.Models;

public class Settings
{
    public DragMode DragMode { get; set; } = DragMode.GridBased;
    public int GridSize { get; set; } = 100;
    public string WidgetsFolderPath { get; set; } = "";
}

public enum DragMode
{
    Free,
    GridBased
}
