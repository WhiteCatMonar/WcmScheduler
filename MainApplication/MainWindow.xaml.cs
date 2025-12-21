using System.Windows;

namespace MainApplication
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("新規作成がクリックされました");
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("開くがクリックされました");
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("コピーがクリックされました");
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("貼り付けがクリックされました");
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("このアプリはWPFで作成されています");
        }
    }
}
