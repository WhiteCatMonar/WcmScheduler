using MainApplication.ViewModels.Core;
using System.Windows;

namespace MainApplication.ViewModels.ProjectModel
{
    /// <summary>
    /// ノードの入出力ポートを表すViewModel。
    /// ノード内の相対座標・キャンバス上の絶対座標・接続線一覧などを管理する。
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

        private Guid _portGuid;

        /// <summary>
        /// ポートを一意に識別するGUID
        /// </summary>
        public Guid PortGuid
        {
            get => _portGuid;
            set => SetProperty(ref _portGuid, value);
        }

        /// <summary>
        /// ポート名(UI表示用)
        /// </summary>
        public required string Name{ get; init; }

        /// <summary>
        /// ポートの種類(入力 or 出力)
        /// </summary>
        public required PortType Type { get; init; }

        /* ---------------------------------------------------------
         * ノード内の相対座標(論理座標)
         * --------------------------------------------------------- */

        private Point _relativePosition;

        public Point RelativePosition
        {
            get => _relativePosition;
            set => SetProperty(
                ref _relativePosition,
                value
            );
        }

        /* ---------------------------------------------------------
         * キャンバス上の絶対座標(論理座標)
         * --------------------------------------------------------- */

        private Point _absolutePosition;
        
        /// <summary>
        /// キャンバス上の絶対座標(論理座標)
        /// </summary>
        public Point AbsolutePosition
        {
            get => _absolutePosition;
            set => SetProperty(ref _absolutePosition, value);
        }

        /* ---------------------------------------------------------
         * 親ノード
         * --------------------------------------------------------- */

        /// <summary>
        /// このポートが属するノード
        /// </summary>
        public required NodeViewModel ParentNode { get; init; }

        /* ---------------------------------------------------------
         * 座標更新
         * --------------------------------------------------------- */

        /// <summary>
        /// 親ノードの位置と相対座標から絶対座標を再計算する。
        /// ノード移動時に呼び出される。
        /// </summary>
        public void UpdateAbsolutePosition()
        {
            if (ParentNode == null)
            {
                return;
            }

            AbsolutePosition = PointEx.Add(ParentNode.Position, RelativePosition);
        }
    }
}

/* --- End of file --- */
