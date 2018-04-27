using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows;
using GeniusTetris.Core;
using System.Diagnostics;

namespace GeniusTetris
{
    /// <summary>
    /// classe presenter pour l'ui
    /// </summary>
    class OneBlockPresenter : INotifyPropertyChanged
    {
        private Board _Owner;
        private int _x, _y;
        private int _lastNumber = -1;

        public OneBlockPresenter(Board owner, int x, int y)
        {
            _x = x;
            _y = y;
            _Owner = owner;
            _Owner.OnChanged += new EventHandler<BoardChangedEventArgs>(_Owner_OnChanged);
        }

        void _Owner_OnChanged(object sender, BoardChangedEventArgs e)
        {
            if (e.IsChanged(_x, _y))
            {
                Number = _Owner[_x, _y];
            }
        }

        public Visibility Visibility
        {
            get
            {
                //return Visibility.Visible;
                return Number > 0 ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public int Number
        {
            get
            {
                if (_lastNumber < 0)
                    _lastNumber = _Owner[_x, _y];
                return _lastNumber;
            }
            private set
            {
                if (value != _lastNumber)
                {
                    //bool visibilitychanged = (_lastNumber == 0 && value > 0) || (_lastNumber > 0 && value == 0);
                    _lastNumber = value;
                    DoChanged("");
                    //DoChanged("Number");
                    //if (visibilitychanged)
                    //    DoChanged("Visibility");
                }
            }
        }

        #region INotifyPropertyChanged Members

        private void DoChanged(string aproperty)
        {
            //Debug.WriteLine(string.Format("_Owner_DoChanged({0}, {1}, {2})", _x, _y, aproperty));
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(aproperty));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
