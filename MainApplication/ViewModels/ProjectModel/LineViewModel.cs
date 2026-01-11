using MainApplication.ViewModels.Core;
using System.ComponentModel;
using System.Windows.Media;

namespace MainApplication.ViewModels.ProjectModel
{
    /// <summary>
    /// グリッド線 1 本を表す ViewModel。
    /// 座標・太さ・色などの描画情報を保持する。
    /// </summary>
    public class LineViewModel : ViewModelBase
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
            set => SetProperty(ref _x1, value);
        }

        /// <summary>線分の始点Y座標</summary>
        public double Y1
        {
            get => _y1;
            set => SetProperty(ref _y1, value);

        }

        /// <summary>線分の終点X座標</summary>
        public double X2
        {
            get => _x2;
            set => SetProperty(ref _x2, value);
        }

        /// <summary>線分の終点Y座標</summary>
        public double Y2
        {
            get => _y2;
            set => SetProperty(ref _y2, value);
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
            set => SetProperty(ref _isMajor, value);
        }

        /* ---------------------------------------------------------
         * 描画スタイル
         * --------------------------------------------------------- */

        /// <summary>線の色</summary>
        public Brush Stroke
        {
            get => _stroke;
            set => SetProperty(ref _stroke, value);
        }

        /// <summary>線の太さ</summary>
        public double Thickness
        {
            get => _thickness;
            set => SetProperty(ref _thickness, value);
        }
    }
}

/* --- End of file --- */
