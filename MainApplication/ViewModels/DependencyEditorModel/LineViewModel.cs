using MainApplication.ViewModels.Core;
using System.Windows;
using System.Windows.Media;

namespace MainApplication.ViewModels.DependencyEditorModel
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

        private Point _start, _end;
        public bool _isMajor;
        private Brush _stroke = Brushes.LightGray;
        private double _thickness = 0.5;

        /* ---------------------------------------------------------
         * 座標(論理座標)
         * --------------------------------------------------------- */

        /// <summary>線分の始点座標</summary>
        public Point Start
        {
            get => _start;
            set => SetProperty(
                ref _start,
                value
            );
        }

        /// <summary>線分の終点座標</summary>
        public Point End
        {
            get => _end;
            set => SetProperty(
                ref _end, 
                value
            );
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
