using System.ComponentModel;
using System.Windows;

namespace AkashaNavigator.ViewModels.Dialogs
{
    /// <summary>
    /// 插件选择项
    /// </summary>
    public class PluginSelectorItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0.0";
        public string? Description { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
                }
            }
        }

        public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
        public Visibility HasDescriptionVisibility => HasDescription ? Visibility.Visible : Visibility.Collapsed;

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
