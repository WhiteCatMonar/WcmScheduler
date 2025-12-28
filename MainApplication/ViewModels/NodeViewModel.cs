using MainApplication.ViewModels.Actions;
using MainApplication.ViewModels.Core;
using MainApplication.ViewModels.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

namespace MainApplication.ViewModels
{
    public class NodeViewModel : INotifyPropertyChanged
    {
        private readonly UndoRedoManager _undoRedo;
        private readonly IDateTimeEditorService _dateTimeEditor;
        private DispatcherTimer _editTimer;
        private List<EditableField<string>> _editableFields;

        public NodeViewModel(UndoRedoManager undoRedo, IDateTimeEditorService dateTimeEditor)
        {
            _undoRedo = undoRedo ?? throw new ArgumentNullException(nameof(undoRedo));
            _dateTimeEditor = dateTimeEditor;
            _nodeGuid = Guid.NewGuid();
            EditStartDateTimeCommand = new RelayCommand(() => EditDateTime(true));
            EditEndDateTimeCommand = new RelayCommand(() => EditDateTime(false));
            NotifyEditedCommand = new RelayCommand(() => NotifyEdited());

            _editTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(5)
            };
            _editTimer.Tick += (s, e) =>
            {
                _editTimer.Stop();
                CommitEdits();
            };

            _editableFields = new List<EditableField<string>>
            {
                new EditableField<string>("TaskName", () => TaskName, v => TaskName = v),
                new EditableField<string>("Person", () => Person, v => Person = v),
                new EditableField<string>("Comment", () => Comment, v => Comment = v)
            };
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
            var picked = _dateTimeEditor.EditDateTime(
                initial,
                isStart ? (Func<DateTime?, bool>)CanSetStartDateTime : CanSetEndDateTime
            );

            if (picked == initial)
            {
                return;
            }

            if (isStart)
            {
                if (!CanSetStartDateTime(picked))
                {
                    return;
                }
                StartDateTime = picked;
            }
            else
            {
                if (!CanSetEndDateTime(picked))
                {
                    return;
                }
                EndDateTime = picked;
            }
        }

        public bool CanSetStartDateTime(DateTime? newStartDateTime)
        {
            if ((newStartDateTime == null) || (EndDateTime == null))
            {
                return true;
            }
            return newStartDateTime <= EndDateTime;
        }

        public bool CanSetEndDateTime(DateTime? newEndDateTime)
        {
            if ((newEndDateTime == null) || (StartDateTime == null))
            {
                return true;
            }
            return StartDateTime <= newEndDateTime;
        }

        public static readonly double _minWidth = 100;
        public double MinWidth
        {
            get => _minWidth;
        }

        public static readonly double _minHeight = 60;
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

        public void CommitEdits()
        {
            foreach (var field in _editableFields)
            {
                field.TryCommit(CommitHistory);
            }
        }

        public ICommand NotifyEditedCommand { get; }
        public void NotifyEdited()
        {
            _editTimer.Stop();
            _editTimer.Start();
        }

        public ObservableCollection<PortViewModel> InputPorts { get; } = new ObservableCollection<PortViewModel>();
        public ObservableCollection<PortViewModel> OutputPorts { get; } = new ObservableCollection<PortViewModel>();
        public IEnumerable<PortViewModel> AllPorts => InputPorts.Concat(OutputPorts);

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
