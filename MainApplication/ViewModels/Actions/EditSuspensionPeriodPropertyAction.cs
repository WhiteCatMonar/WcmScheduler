using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.DependencyEditorModel;
using MainApplication.ViewModels.ProjectModel;
using System.ComponentModel;
using System.Reflection;

namespace MainApplication.ViewModels.Actions
{
    /// <summary>
    /// 中断期間プロパティ編集操作を表すUndo/Redoアクション。
    /// </summary>
    public class EditSuspensionPeriodPropertyAction : IUndoableAction
    {
        private readonly SuspensionPeriodViewModel _period;
        private readonly string _propertyName;
        private readonly object? _oldValue;
        private readonly object? _newValue;
        private readonly string _displayName;

        /// <summary>
        /// アクション種別。
        /// </summary>
        public string ActionType => "EditSuspensionPeriodProperty";

        /// <summary>
        /// アクションの説明。
        /// </summary>
        public string Description => $"{_displayName} を {_oldValue} から {_newValue} に変更";

        /// <summary>
        /// 中断期間プロパティ編集アクションを生成する。
        /// </summary>
        /// <param name="period">対象の中断期間。</param>
        /// <param name="propertyName">変更対象プロパティ名。</param>
        /// <param name="oldValue">変更前の値。</param>
        /// <param name="newValue">変更後の値。</param>
        public EditSuspensionPeriodPropertyAction(
            SuspensionPeriodViewModel period,
            string propertyName,
            object? oldValue,
            object? newValue
        )
        {
            _period = period;
            _propertyName = propertyName;
            _oldValue = oldValue;
            _newValue = newValue;

            var prop = _period.GetType().GetProperty(_propertyName);
            var displayAttr = prop?.GetCustomAttribute<DisplayNameAttribute>();
            _displayName = displayAttr?.DisplayName ?? _propertyName;
        }

        /// <summary>
        /// プロパティ値を変更前に戻す。
        /// </summary>
        public void Undo()
        {
            SetValue(_oldValue);
        }

        /// <summary>
        /// プロパティ値を変更後に設定する。
        /// </summary>
        public void Redo()
        {
            SetValue(_newValue);
        }

        /// <summary>
        /// 対象プロパティへ値を設定する。
        /// </summary>
        /// <param name="value">設定値。</param>
        private void SetValue(object? value)
        {
            var prop = _period.GetType().GetProperty(_propertyName);
            prop?.SetValue(_period, value);
        }
    }
}

/* --- End of file --- */
