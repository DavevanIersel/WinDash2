namespace WinDash2.Models;

public class Settings
{
    public DragMode DragMode { get; set; } = DragMode.GridBased;
}

public enum DragMode
{
    Free,
    GridBased
}
