using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace GeniusTetris.Core
{
    public class BoardChangedEventArgs : EventArgs
    {
        Dictionary<Point, bool> _Changed;
        bool _AlwaysChanges;

        public BoardChangedEventArgs(Dictionary<Point, bool> changed)
        {
            _Changed = changed;
            _AlwaysChanges = false;
        }
        
        public BoardChangedEventArgs(Dictionary<Point, bool> changed, bool alwaysChange)
        {
            _Changed = changed;
            _AlwaysChanges = alwaysChange;
        }
        
        public bool IsChanged(int x, int y)
        {
            if (_AlwaysChanges)
                return true;
            if (_Changed.ContainsKey(new Point(x, y)))
                return true;
            return false;
        }
    }
}
