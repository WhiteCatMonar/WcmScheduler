using MainApplication.Models.SaveData;
using MainApplication.ViewModels.DependencyEditorModel;
using System.Linq;

namespace MainApplication.Mappers
{
    /// <summary>
    /// 依存関係編集ViewModelと保存データを相互変換するMapper
    /// </summary>
    public static class DependencyEditorMapper
    {
        /// <summary>
        /// 依存関係編集ViewModelを保存データへ変換する
        /// </summary>
        /// <param name="vm">依存関係編集ViewModel</param>
        /// <returns>タスク編集保存データ</returns>
        public static TaskEditorDataModel ToDataModel(DependencyEditorViewModel vm)
        {
            return new TaskEditorDataModel
            {
                Nodes = [.. vm.Nodes.Nodes.Select(NodeMapper.ToDataModel)],
                Connections = [..vm.Connections.Connections.Select(ConnectionMapper.ToDataModel)]
            };
        }

        /// <summary>
        /// 保存データを依存関係編集ViewModelへ変換する
        /// </summary>
        /// <param name="data">タスク編集保存データ</param>
        /// <param name="editor">既存の依存関係編集ViewModel</param>
        /// <returns>復元した依存関係編集ViewModel</returns>
        public static DependencyEditorViewModel ToViewModel(TaskEditorDataModel data, DependencyEditorViewModel editor)
        {
            DependencyEditorViewModel loadedDependencyEditor = new()
            {
                UndoRedo = editor.UndoRedo,
                DateTimeEditor = editor.DateTimeEditor
            };
            foreach (var nodeData in data.Nodes)
            {
                loadedDependencyEditor.Nodes.Nodes.Add(NodeMapper.ToViewModel(nodeData, loadedDependencyEditor));
            }

            foreach (var connData in data.Connections)
            {
                ConnectionViewModel? connection = ConnectionMapper.ToViewModel(connData, loadedDependencyEditor);
                if (connection is not null)
                {
                    loadedDependencyEditor.Connections.Connections.Add(connection);
                }
            }

            return loadedDependencyEditor;
        }
    }

}

/* --- End of file --- */
