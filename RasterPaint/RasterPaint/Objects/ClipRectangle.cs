using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using RasterPaint.Annotations;

namespace RasterPaint.Objects
{
    public class ClipRectangle : INotifyPropertyChanged
    {
        private int _xpos;
        private int _ypos;
        private int _xSize;
        private int _ySize;

        public ClipRectangle(int _xpos, int _ypos, int _xSize, int _ySize)
        {
            this._xpos = _xpos;
            this._ypos = _ypos;
            this._xSize = _xSize;
            this._ySize = _ySize;
        }

        public Point[] ToArray()
        {
            return new List<Point>
            {
                new Point(_xpos, _ypos),
                new Point(_xpos, _ypos + _ySize),
                new Point(_xpos + _xSize, _ypos + _ySize),
                new Point(_xpos + _xSize, _ypos)
            }.ToArray<Point>();
        }

        public int Xpos
        {
            get
            {
                return _xpos;
            }

            set
            {
                _xpos = value;
                OnPropertyChanged(nameof(Xpos));
            }
        }

        public int Ypos
        {
            get
            {
                return _ypos;
            }

            set
            {
                _ypos = value;
                OnPropertyChanged(nameof(Ypos));
            }
        }

        public int XSize
        {
            get
            {
                return _xSize;
            }

            set
            {
                _xSize = value;
                OnPropertyChanged(nameof(XSize));
            }
        }

        public int YSize
        {
            get
            {
                return _ySize;
            }

            set
            {
                _ySize = value;
                OnPropertyChanged(nameof(YSize));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            if (propertyName != null) Trace.WriteLine(propertyName);
        }
    }
}
