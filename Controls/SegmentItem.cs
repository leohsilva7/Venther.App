using System.ComponentModel;

namespace Venther.App.Controls;

public class SegmentItem : INotifyPropertyChanged
{
    private string _title = string.Empty;
    private int _count;
    private string _key = string.Empty;

    public string Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
            }
        }
    }

    public int Count
    {
        get => _count;
        set
        {
            if (_count != value)
            {
                _count = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
            }
        }
    }

    public string Key
    {
        get => _key;
        set
        {
            if (_key != value)
            {
                _key = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Key)));
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public SegmentItem() { }

    public SegmentItem(string title, int count = 0, string key = "")
    {
        Title = title;
        Count = count;
        Key = string.IsNullOrEmpty(key) ? title : key;
    }
}
