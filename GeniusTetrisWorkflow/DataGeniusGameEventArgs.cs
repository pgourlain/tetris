using System;
using System.Collections.Generic;
using System.Text;
using GeniusP2PManager;

namespace GeniusTetris
{
    /// <summary>
    /// Generic eventargs with a generic Data type, it used between GeniusTetrisApplication and GUI
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DataGeniusGameEventArgs<T> : BaseGeniusGameAppEventArgs
    {
        private T _Data;
        public DataGeniusGameEventArgs(string senderId, T aData)
            : base(senderId)
        {
            _Data = aData;
        }

        public T Data
        {
            get
            {
                return _Data;
            }
            set
            {
                _Data = value;
            }
        }
    }
}
