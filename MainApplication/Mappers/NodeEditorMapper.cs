using MainApplication.ViewModels;
using MainApplication.Models.SaveData;
using System;
using System.Linq;

namespace MainApplication.Mappers
{
    public static class NodeEditorMapper
    {
        /* ---------------------------------------------------------
         * ViewModel → DataModel
         * --------------------------------------------------------- */
        public static TaskEditorDataModel ToDataModel(NodeEditorViewModel vm)
        {
            return new TaskEditorDataModel
            {
                Nodes = vm.Nodes.Nodes.Select(NodeMapper.ToDataModel).ToList(),
                Connections = vm.Connections.Connections.Select(ConnectionMapper.ToDataModel).ToList()
            };
        }

        /* ---------------------------------------------------------
         * DataModel → ViewModel
         * --------------------------------------------------------- */
        public static NodeEditorViewModel ToViewModel(TaskEditorDataModel data, NodeEditorViewModel editor)
        {
            NodeEditorViewModel loadedNodeEditor = new NodeEditorViewModel(editor.ModelName);
            loadedNodeEditor.UndoRedo = editor.UndoRedo;
            loadedNodeEditor.DateTimeEditor = editor.DateTimeEditor;
            foreach (var nodeData in data.Nodes)
            {
                loadedNodeEditor.Nodes.Nodes.Add(NodeMapper.ToViewModel(nodeData, loadedNodeEditor));
            }

            foreach (var connData in data.Connections)
            {
                /* 接続線は再構築時にノード情報を参照するため、ノード読み込み済みのViewModelを渡す */
                loadedNodeEditor.Connections.Connections.Add(ConnectionMapper.ToViewModel(connData, loadedNodeEditor));
            }

            return loadedNodeEditor;
        }
    }

}
