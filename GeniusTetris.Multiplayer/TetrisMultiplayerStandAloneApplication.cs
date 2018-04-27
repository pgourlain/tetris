using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeniusTetris.Core;

namespace GeniusTetris.Multiplayer
{
    public class TetrisMultiplayerStandAloneApplication : TetrisMultiplayerApplicationBase
    {
        public TetrisMultiplayerStandAloneApplication(ITimer timer) 
            : base(timer)
        {

        }

        public override void HideBoard()
        {
            throw new NotImplementedException();
        }

        public override void ShowBoard()
        {
            throw new NotImplementedException();
        }

        public override void SendBoard(byte[,] board)
        {
            throw new NotImplementedException();
        }

        public override void SendScore(int score)
        {
            throw new NotImplementedException();
        }

        public override void SendGameOver()
        {
            throw new NotImplementedException();
        }

        public override void SendOption(GeniusTetrisPlayer toplayer, byte option)
        {
            throw new NotImplementedException();
        }

        public override void AcceptGameRequest()
        {
            throw new NotImplementedException();
        }

        public override void RejectGameRequest()
        {
            throw new NotImplementedException();
        }

        public override void StartMultiplayerGameWF()
        {
            throw new NotImplementedException();
        }

        public override void StopMultiPlayerGameWF()
        {
            throw new NotImplementedException();
        }

        public override void ConnectToMesh()
        {
            throw new NotImplementedException();
        }

        public override void DisconnectFromMesh()
        {
            throw new NotImplementedException();
        }
    }
}
