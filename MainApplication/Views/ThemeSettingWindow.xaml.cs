using System.Windows;

namespace MainApplication.Views
{
    public partial class ThemeSettingWindow : Window
    {
        public ThemeSettingWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
