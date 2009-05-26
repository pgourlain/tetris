using System;
using System.Collections.Generic;
using System.Text;
using GeniusTetris.Core;
using GeniusTetris.Multiplayer;

namespace GeniusTetris
{
    /// <summary>
    /// Proxied a remote player, send a option when ExecuteOption is called
    /// </summary>
    class ProxyGame : IGame
    {
        ITetrisMultiplayerApplication _app;
        Guid _toMember;
        Board _toMemberBoard;

        public ProxyGame(ITetrisMultiplayerApplication app, Guid toMember, Board toMemberBoard)
        {
            _app = app;
            _toMember = toMember;
            _toMemberBoard = toMemberBoard;
        }

        #region IGame Members

        public void ExecuteOption(byte option, IGame sender)
        {
            _app.SendOption(new GeniusTetris.Multiplayer.GeniusTetrisPlayer { ID = _toMember }, option);
        }

        /// <summary>
        /// Returns board of remote player, in fact it returns a last board who player has sent.
        /// </summary>
        /// <returns></returns>
        public byte[,] GetBoardData()
        {
            return _toMemberBoard.GetBlocksWithoutMover();
        }

        #endregion
    }
}
