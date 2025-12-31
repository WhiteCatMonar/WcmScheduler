using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.ProjectModel;
using System.ComponentModel;
using System.Reflection;

namespace MainApplication.ViewModels.Actions
{
    public class EditNodePropertyAction : IUndoableAction
    {
        /* ---------------------------------------------------------
         * アクションのメタ情報
         * --------------------------------------------------------- */

        /// <summary>
        /// アクション種別(識別用)
        /// </summary>
        public string ActionType => "EditNodeProperty";
        
        /// <summary>
        /// アクションの説明(UI表示用)
        /// </summary>
        public string Description => $"{_displayName} を {_oldValue} から {_newValue} に変更";

        /* ---------------------------------------------------------
         * フィールド
         * --------------------------------------------------------- */

        private readonly NodeViewModel _node;
        private readonly string _propertyName;
        private readonly object _oldValue;
        private readonly object _newValue;
        private readonly string _displayName;

        /* ---------------------------------------------------------
         * コンストラクタ
         * --------------------------------------------------------- */

        /// <summary>
        /// ノードの任意プロパティ変更アクションを生成する
        /// </summary>
        /// <param name="node">対象ノード</param>
        /// <param name="propertyName">変更対象プロパティ名</param>
        /// <param name="oldValue">変更前の値</param>
        /// <param name="newValue">変更後の値</param>
        public EditNodePropertyAction(NodeViewModel node, string propertyName, object oldValue, object newValue)
        {
            _node = node;
            _propertyName = propertyName;
            _oldValue = oldValue;
            _newValue = newValue;

            /* DisplayNameAttributeが付いていればUI表示名として使用 */
            var prop = _node.GetType().GetProperty(_propertyName);
            var displayAttr = prop?.GetCustomAttribute<DisplayNameAttribute>();
            _displayName = displayAttr?.DisplayName ?? _propertyName;
        }

        /* ---------------------------------------------------------
         * Undo 処理
         * --------------------------------------------------------- */

        /// <summary>
        /// プロパティ値を変更前の値に戻す
        /// </summary>
        public void Undo()
        {
            SetValue(_oldValue);
        }

        /* ---------------------------------------------------------
         * Redo 処理
         * --------------------------------------------------------- */

        /// <summary>
        /// プロパティ値を変更後の値に再設定する
        /// </summary>
        public void Redo()
        {
            SetValue(_newValue);
        }

        /* ---------------------------------------------------------
         * 内部処理(リフレクションで値を設定)
         * --------------------------------------------------------- */

        /// <summary>
        /// リフレクションを用いて対象プロパティに値を設定する
        /// </summary>
        private void SetValue(object value)
        {
            var prop = _node.GetType().GetProperty(_propertyName);
            prop?.SetValue(_node, value);
        }
    }
}

/* --- End of file --- */
