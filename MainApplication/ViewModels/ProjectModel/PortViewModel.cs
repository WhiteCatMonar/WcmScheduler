using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace MainApplication.ViewModels.ProjectModel
{
    /// <summary>
    /// ノードの入出力ポートを表すViewModel。
    /// ノード内の相対座標・キャンバス上の絶対座標・接続線一覧などを管理する。
    /// </summary>
    public class PortViewModel : INotifyPropertyChanged
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
            set
            {
                if (_portGuid == value)
                {
                    return;
                }
                _portGuid = value;
                OnPropertyChanged(nameof(PortGuid));
            }
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

        private double _relativeX;

        /// <summary>
        /// ノード左上からの相対X座標(論理座標)
        /// </summary>
        public double RelativeX
        {
            get => _relativeX;
            set { _relativeX = value; OnPropertyChanged(nameof(RelativeX)); }
        }

        private double _relativeY;

        /// <summary>
        /// ノード左上からの相対Y座標(論理座標)
        /// </summary>
        public double RelativeY
        {
            get => _relativeY;
            set { _relativeY = value; OnPropertyChanged(nameof(RelativeY)); }
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
            set
            {
                if (_absolutePosition != value)
                {
                    _absolutePosition = value;
                    OnPropertyChanged(nameof(AbsolutePosition));
                }
            }
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

            AbsolutePosition = new Point(
                ParentNode.X + RelativeX,
                ParentNode.Y + RelativeY
            );
        }

        /* ---------------------------------------------------------
         * INotifyPropertyChanged
         * --------------------------------------------------------- */

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// プロパティ変更通知を発行する
        /// </summary>
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

/* --- End of file --- */
