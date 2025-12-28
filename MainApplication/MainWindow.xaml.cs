using MainApplication.ViewModels;
using System.Collections.Generic;
using System.Windows;

namespace MainApplication
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public SchedulerViewModel SchedulerVM { get; }

        public MainWindow()
        {
            InitializeComponent();

            SchedulerVM = new SchedulerViewModel(new Dictionary<string, string>(){
                { "NodeEditor", "タスク編集" }
            });
            DataContext = SchedulerVM;

            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SchedulerVM.RequestSaveAs += OnRequestSaveAs;
        }

        private void OnRequestSaveAs()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "JSON ファイル (*.json)|*.json"
            };

            if (dialog.ShowDialog() == true)
            {
                SchedulerVM.SaveAs(dialog.FileName);
            }
        }
    }
}
