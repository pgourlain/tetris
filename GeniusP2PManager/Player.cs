using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusP2PManager
{
    /// <summary>
    /// base class that represents a generic player
    /// </summary>
    public class BasePlayer
    {
        private Guid _ID;
        private string _DisplayName;

        /// <summary>
        /// name of player, it's used on mesh
        /// </summary>
        public string DisplayName 
        {
            get
            {
                return _DisplayName;
            }
            set
            {
                _DisplayName = value;
            }
        }

        public Guid ID
        {
            get
            {
                return _ID;
            }
            set
            {
                _ID = value;
            }
        }
    }
}
