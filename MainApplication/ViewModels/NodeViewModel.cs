using MainApplication.ViewModels.Actions;
using MainApplication.ViewModels.Infrastructure;
using MainApplication.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MainApplication.ViewModels
{
    public class NodeViewModel : INotifyPropertyChanged
    {
        private readonly UndoRedoManager _undoRedo;

        public NodeViewModel(UndoRedoManager undoRedo)
        {
            _undoRedo = undoRedo ?? throw new ArgumentNullException(nameof(undoRedo));
            _nodeGuid = Guid.NewGuid();
            EditStartDateTimeCommand = new RelayCommand(() => EditDateTime(true));
            EditEndDateTimeCommand = new RelayCommand(() => EditDateTime(false));
        }

        public void CommitHistory(string propertyName, object oldValue, object newValue)
        {
            if (!_undoRedo.IsApplyingHistory)
            {
                var action = new EditNodePropertyAction(this, propertyName, oldValue, newValue);
                _undoRedo.Execute(action);
            }
        }

        public ICommand EditStartDateTimeCommand { get; }
        public ICommand EditEndDateTimeCommand { get; }
        private void EditDateTime(bool isStart)
        {
            var initial = isStart ? StartDateTime : EndDateTime;
            var window = new DateTimeEditorWindow(initial)
            {
                Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
            };

            if (window.ShowDialog() == true && window.DataContext is DateTimeEditorViewModel vm)
            {
                var picked = vm.Result ?? vm.Composed;

                if (isStart)
                {
                    if (picked.HasValue)
                    {
                        // Start更新時、Endが設定済みなら整合性チェック
                        if (EndDateTime.HasValue && picked.Value > EndDateTime.Value)
                        {
                            // ユーザー通知(例：MessageBox)かValidationへ
                            MessageBox.Show("開始が終了より後です。終了時刻も合わせて見直してください。");
                        }
                        StartDateTime = picked.Value;
                    }
                    else
                    {
                        StartDateTime = null;
                    }
                }
                else
                {
                    if (picked.HasValue)
                    {
                        if (StartDateTime.HasValue && picked.Value < StartDateTime.Value)
                        {
                            MessageBox.Show("終了が開始より前です。開始時刻より後の時刻を選んでください。");
                        }
                        EndDateTime = picked.Value;
                    }
                    else
                    {
                        EndDateTime = null;
                    }
                }
            }
        }

        public const double _minWidth = 100;
        public double MinWidth
        {
            get => _minWidth;
        }

        public const double _minHeight = 60;
        public double MinHeight
        {
            get => _minHeight;
        }

        private double _width = _minWidth;
        public double Width
        {
            get => _width;
            set
            {
                if (_width != value)
                {
                    _width = value;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }

        private double _height = _minHeight;
        public double Height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                {
                    return;
                }
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        private Guid _nodeGuid;
        [DisplayName("タスクID")]
        public Guid NodeGuid
        {
            get => _nodeGuid;
            set
            {
                if (_nodeGuid == value)
                {
                    return;
                }
                _nodeGuid = value;
                OnPropertyChanged(nameof(NodeGuid));
            }
        }

        private string _oldTaskName = $"(New Task)";
        public string OldTaskName
        {
            get => _oldTaskName;
            set
            {
                if (_oldTaskName == value)
                {
                    return;
                }
                _oldTaskName = value;
                OnPropertyChanged(nameof(OldTaskName));
            }
        }

        private string _taskName = $"(New Task)";
        [DisplayName("タスク名")]
        public string TaskName
        {
            get => _taskName;
            set
            {
                if (_taskName == value)
                {
                    return;
                }
                _taskName = value;
                OnPropertyChanged(nameof(TaskName));
            }
        }

        private string _oldPerson;
        public string OldPerson
        {
            get => _oldPerson;
            set
            {
                if (_oldPerson == value)
                {
                    return;
                }
                _oldPerson = value;
                OnPropertyChanged(nameof(OldPerson));
            }
        }

        private string _person;
        [DisplayName("担当者")]
        public string Person
        {
            get => _person;
            set
            {
                if (_person == value)
                {
                    return;
                }
                _person = value;
                OnPropertyChanged(nameof(Person));
            }
        }

        private DateTime? _startDateTime;
        [DisplayName("開始日時")]
        public DateTime? StartDateTime
        {
            get => _startDateTime;
            set
            {
                if (_startDateTime == value)
                {
                    return;
                }
                if (!_undoRedo.IsApplyingHistory)
                {
                    _undoRedo.Execute(new EditNodePropertyAction(this, nameof(StartDateTime), _startDateTime, value));
                }
                _startDateTime = value;
                OnPropertyChanged(nameof(StartDateTime));
            }
        }

        private DateTime? _endDateTime;
        [DisplayName("終了日時")]
        public DateTime? EndDateTime
        {
            get => _endDateTime;
            set
            {
                if (_endDateTime == value)
                {
                    return;
                }
                if (!_undoRedo.IsApplyingHistory)
                {
                    _undoRedo.Execute(new EditNodePropertyAction(this, nameof(EndDateTime), _endDateTime, value));
                }
                _endDateTime = value;
                OnPropertyChanged(nameof(EndDateTime));
            }
        }

        private string _oldComment;
        public string OldComment
        {
            get => _oldComment;
            set
            {
                if (_oldComment == value)
                {
                    return;
                }
                _oldComment = value;
                OnPropertyChanged(nameof(OldComment));
            }
        }

        private string _comment;
        [DisplayName("コメント")]
        public string Comment
        {
            get => _comment;
            set
            {
                if (_comment == value)
                {
                    return;
                }
                _comment = value;
                OnPropertyChanged(nameof(Comment));
            }
        }

        private double _x;
        public double X
        {
            get => _x;
            set
            {
                if (_x == value)
                {
                    return;
                }
                _x = value;
                OnPropertyChanged(nameof(X));
            }
        }

        private double _y;
        public double Y
        {
            get => _y;
            set
            {
                if (_y == value)
                {
                    return;
                }
                _y = value;
                OnPropertyChanged(nameof(Y));
            }
        }

        public void UpdateAllPortPositions()
        {
            foreach (var port in InputPorts)
            {
                port.UpdateAbsolutePosition();
            }

            foreach (var port in OutputPorts)
            {
                port.UpdateAbsolutePosition();
            }
        }

        public ObservableCollection<PortViewModel> InputPorts { get; } = new ObservableCollection<PortViewModel>();
        public ObservableCollection<PortViewModel> OutputPorts { get; } = new ObservableCollection<PortViewModel>();
        public IEnumerable<PortViewModel> AllPorts => InputPorts.Concat(OutputPorts);

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
