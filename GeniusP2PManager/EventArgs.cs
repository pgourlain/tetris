using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusP2PManager
{
    /// <summary>
    /// base class for all event produced in GeniusGameApp and all of descendants
    /// </summary>
    public class BaseGeniusGameAppEventArgs : EventArgs
    {
        private string _SenderId;

        public BaseGeniusGameAppEventArgs(string senderID)
        {
            _SenderId = senderID;
        }

        public string SenderId
        {
            get { return _SenderId; }
            set { _SenderId = value; }
        }
    }

    /// <summary>
    /// message chat eventargs
    /// </summary>
    public class MessageReceivedEventArgs : BaseGeniusGameAppEventArgs
    {
        private string _Message;

        public MessageReceivedEventArgs(string senderID, string message) : base(senderID)
        {
            _Message = message;
        }

        public string Message
        {
            get { return _Message; }
            set { _Message = value; }
        }
    }

    public class GameRequestReceivedEventArgs : MessageReceivedEventArgs
    {
        public GameRequestReceivedEventArgs(string senderID, string message) : base (senderID, message)
        {
        }
    }
}
