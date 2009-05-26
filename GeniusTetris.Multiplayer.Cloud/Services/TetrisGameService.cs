using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeniusTetris.Multiplayer.Cloud.Interfaces;
using System.ServiceModel;

namespace GeniusTetris.Multiplayer.Cloud.Services
{
    [ServiceBehavior(InstanceContextMode=InstanceContextMode.Single)]
    public class TetrisGameService : ITetrisGameService
    {
        public ITetrisGameService Inner { get; set; }
        #region ITetrisGameService Members

        public void SendBoard(Guid idplayer, byte[] board, int lengthDimension0, int lengthDimension1)
        {
            if (Inner != null)
            {
                Inner.SendBoard(idplayer, board, lengthDimension0, lengthDimension1);
            }
        }

        public void SetTimerInterval(Guid idplayer, double newValue)
        {
            if (Inner != null)
            {
                Inner.SetTimerInterval(idplayer, newValue);
            }
        }

        public void GameOver(Guid idplayer)
        {
            if (Inner != null)
            {
                Inner.GameOver(idplayer);
            }
        }

        public void SendOption(Guid from, Guid to, byte option)
        {
            if (Inner != null)
            {
                Inner.SendOption(from,to,option);
            }
        }

        public void SendScore(Guid idplayer, int score)
        {
            if (Inner != null)
            {
                Inner.SendScore(idplayer, score);
            }
        }

        public void HideBoard(Guid idplayer, bool value)
        {
            if (Inner != null)
            {
                Inner.HideBoard(idplayer, value);
            }
        }

        public void GameStarting(Guid idCoordinator, int nbPlayer)
        {
            if (Inner != null)
            {
                Inner.GameStarting(idCoordinator, nbPlayer);
            }
        }

        public void PlayerReady(Guid idplayer)
        {
            if (Inner != null)
            {
                Inner.PlayerReady(idplayer);
            }
        }
        
        public void ImIn(Guid idplayer)
        {
            if (Inner != null)
            {
                Inner.ImIn(idplayer);
            }
        }
        #endregion
    }
}
