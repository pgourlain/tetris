using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusTetris.Core
{
    public class BoardOptionsCapturedEventArgs : EventArgs
    {
        byte[] _Options;

        public BoardOptionsCapturedEventArgs(byte[] options)
        {
            _Options = options;

        }

        public byte[] Options
        {
            get
            {
                return _Options;
            }
        }
    }
}
