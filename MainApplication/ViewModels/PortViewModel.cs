using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace MainApplication.ViewModels
{
    public class PortViewModel : INotifyPropertyChanged
    {
        public enum PortType { Input, Output }
        public List<ConnectionViewModel> ConnectedConnections { get; } = new List<ConnectionViewModel>();

        private Guid _portGuid;
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

        private string _name;
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        private PortType _type;
        public PortType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(Type)); }
        }

        /* ノード内の相対座標(Canvas論理座標) */
        private double _relativeX;
        public double RelativeX
        {
            get => _relativeX;
            set { _relativeX = value; OnPropertyChanged(nameof(RelativeX)); }
        }

        private double _relativeY;
        public double RelativeY
        {
            get => _relativeY;
            set { _relativeY = value; OnPropertyChanged(nameof(RelativeY)); }
        }

        /* 絶対座標(Canvas論理座標) */
        private Point _absolutePosition;
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


        public NodeViewModel _parentNode;
        public NodeViewModel ParentNode
        {
            get => _parentNode;
            set
            {
                if (_parentNode != value)
                {
                    _parentNode = value;
                    OnPropertyChanged(nameof(ParentNode));
                }
            }
        }

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
