using System.ComponentModel;
using System.Windows.Media;

namespace MainApplication.ViewModels
{
    /// <summary>
    /// グリッド線 1 本を表す ViewModel。
    /// 座標・太さ・色などの描画情報を保持する。
    /// </summary>
    public class LineViewModel : INotifyPropertyChanged
    {
        /* ---------------------------------------------------------
         * フィールド
         * --------------------------------------------------------- */

        private double _x1, _y1, _x2, _y2;
        public bool _isMajor;
        private Brush _stroke = Brushes.LightGray;
        private double _thickness = 0.5;

        /* ---------------------------------------------------------
         * 座標(論理座標)
         * --------------------------------------------------------- */
        /// <summary>線分の始点X座標</summary>
        public double X1
        {
            get => _x1;
            set
            {
                _x1 = value;
                OnPropertyChanged(nameof(X1));
            }
        }

        /// <summary>線分の始点Y座標</summary>
        public double Y1
        {
            get => _y1;
            set
            {
                _y1 = value;
                OnPropertyChanged(nameof(Y1));
            }
        
        }

        /// <summary>線分の終点X座標</summary>
        public double X2
        {
            get => _x2;
            set
            {
                _x2 = value;
                OnPropertyChanged(nameof(X2));
            }
        }

        /// <summary>線分の終点Y座標</summary>
        public double Y2
        {
            get => _y2;
            set
            {
                _y2 = value;
                OnPropertyChanged(nameof(Y2));
            }
        }

        /* ---------------------------------------------------------
         * グリッド線の種類
         * --------------------------------------------------------- */

        /// <summary>
        /// 主要グリッド線かどうか(trueなら太線)
        /// </summary>
        public bool IsMajor
        {
            get => _isMajor;
            set
            {
                _isMajor = value;
                OnPropertyChanged(nameof(IsMajor));
            }
        }

        /* ---------------------------------------------------------
         * 描画スタイル
         * --------------------------------------------------------- */

        /// <summary>線の色</summary>
        public Brush Stroke
        {
            get => _stroke;
            set
            {
                _stroke = value;
                OnPropertyChanged(nameof(Stroke));
            }
        }

        /// <summary>線の太さ</summary>
        public double Thickness
        {
            get => _thickness;
            set
            {
                _thickness = value;
                OnPropertyChanged(nameof(Thickness));
            }
        }

        /* ---------------------------------------------------------
         * INotifyPropertyChanged
         * --------------------------------------------------------- */

        public event PropertyChangedEventHandler PropertyChanged;
        
        /// <summary>
        /// プロパティ変更通知を発行する
        /// </summary>
        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

/* --- End of file --- */
