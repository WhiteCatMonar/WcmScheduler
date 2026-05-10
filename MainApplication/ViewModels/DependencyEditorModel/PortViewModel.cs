using MainApplication.ViewModels.Core;
using System.Windows;

namespace MainApplication.ViewModels.DependencyEditorModel
{
    /// <summary>
    /// 入出力ポートを表すViewModel。
    /// 基準点(ポートが所属するオブジェクト座標)からの相対座標・キャンバス上の絶対座標・接続線一覧などを管理する。
    /// </summary>
    public class PortViewModel : ViewModelBase
    {
        /* ---------------------------------------------------------
         * ポート種別
         * --------------------------------------------------------- */

        /// <summary>
        /// ポートの種類(入力 or 出力)
        /// </summary>
        public enum PortType { Input, Output }

        /// <summary>
        /// このポートに接続されている接続線一覧
        /// </summary>
        public List<ConnectionViewModel> ConnectedConnections { get; } = [];

        /* ---------------------------------------------------------
         * 識別子・基本情報
         * --------------------------------------------------------- */

        /// <summary> ポートを一意に識別するGUID </summary>
        public required Guid PortGuid { get; init; }

        /// <summary> ポート名(UI表示用) </summary>
        public required string Name{ get; init; }

        /// <summary> ポートの種類(入力 or 出力) </summary>
        public required PortType Type { get; init; }

        /* ---------------------------------------------------------
         * ポートの座標管理(論理座標)
         * --------------------------------------------------------- */

        private Point _controlPoint;

        /// <summary>
        /// ポートの座標計算の基準点(論理座標)
        /// </summary>
        public Point ControlPoint
        {
            get => _controlPoint;
            set => SetProperty(
                ref _controlPoint,
                value,
                [
                    nameof(AbsolutePosition)
                ]
            );
        }

        private Point _relativePosition;

        /// <summary>
        /// 基準点からの相対座標(論理座標)
        /// </summary>
        public Point RelativePosition
        {
            get => _relativePosition;
            set => SetProperty(
                ref _relativePosition,
                value,
                [
                    nameof(AbsolutePosition)
                ]
            );
        }

        /// <summary>
        /// キャンバス上の絶対座標(論理座標)
        /// </summary>
        public Point AbsolutePosition => ControlPoint.Add(RelativePosition);
    }
}

/* --- End of file --- */
