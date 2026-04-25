using MainApplication.ViewModels.Core;
using System.Windows.Input;

namespace MainApplication.ViewModels.ThemeModel
{
    public class ThemeMenuItemViewModel
    {
        public string DisplayName { get; }
        public ICommand ApplyCommand { get; }

        public ThemeMenuItemViewModel(string name, Action applyAction)
        {
            DisplayName = name;
            ApplyCommand = new RelayCommand(applyAction);
        }
    }
}

/* --- End of file --- */
