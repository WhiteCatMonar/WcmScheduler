using MainApplication.ViewModels;
using System.Windows;

namespace MainApplication
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public NodeEditorViewModel EditorViewModel { get; }

        public MainWindow()
        {
            InitializeComponent();

            EditorViewModel = new NodeEditorViewModel();
            DataContext = EditorViewModel;

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            EditorViewModel.RequestSaveAs += OnRequestSaveAs;
        }

        private void OnRequestSaveAs()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON ファイル (*.json)|*.json"
            };

            if (dialog.ShowDialog() == true)
            {
                var vm = DataContext as NodeEditorViewModel;
                vm.SaveAs(dialog.FileName);
            }
        }
    }
}
