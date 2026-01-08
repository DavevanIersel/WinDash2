using System.ComponentModel;

namespace WinDash2.Models;

public class ForceInCurrentTabPattern : INotifyPropertyChanged
{
    private string _pattern = string.Empty;

    public string Pattern
    {
        get => _pattern;
        set
        {
            if (_pattern != value)
            {
                _pattern = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Pattern)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
