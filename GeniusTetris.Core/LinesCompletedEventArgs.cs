using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusTetris.Core
{
    public class LinesCompletedEventArgs : EventArgs
    {
        private int _CompletedLines;

        public LinesCompletedEventArgs(int nb)
        {
            _CompletedLines = nb;
        }

        public int CompletedLines
        {
            get
            {
                return _CompletedLines;
            }
        }
    }
}
