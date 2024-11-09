using Monopost.DAL.Entities;
using System.ComponentModel;
using System.Windows.Media;

public class TemplateDropdownItem : INotifyPropertyChanged
{
    private string _name;
    private ImageSource _previewImage;
    private Template _template;

    public string Name
    {
        get => _name;
        set
        {
            if (_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
    }

    public ImageSource PreviewImage
    {
        get => _previewImage;
        set
        {
            if (_previewImage != value)
            {
                _previewImage = value;
                OnPropertyChanged(nameof(PreviewImage));
            }
        }
    }

    public Template Template
    {
        get => _template;
        set
        {
            if (_template != value)
            {
                _template = value;
                OnPropertyChanged(nameof(Template));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
