using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace MainApplication.ViewModels
{
    public class LineViewModel : INotifyPropertyChanged
    {
        private double _x1, _y1, _x2, _y2;
        private Brush _stroke = Brushes.LightGray;
        private double _thickness = 0.5;

        public double X1 { get => _x1; set { _x1 = value; OnPropertyChanged(); } }
        public double Y1 { get => _y1; set { _y1 = value; OnPropertyChanged(); } }
        public double X2 { get => _x2; set { _x2 = value; OnPropertyChanged(); } }
        public double Y2 { get => _y2; set { _y2 = value; OnPropertyChanged(); } }

        public Brush Stroke { get => _stroke; set { _stroke = value; OnPropertyChanged(); } }
        public double Thickness { get => _thickness; set { _thickness = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
