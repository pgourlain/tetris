using System;
using System.Collections.Generic;
using System.Text;
using GeniusP2PManager;

namespace GeniusTetris
{
	public class GeniusTetrisPlayer1 : BasePlayer
	{

        private int _Score;

        public int Score
        {
            get { return _Score; }
            set { _Score = value; }
        }

        private bool _GameOver;

        public bool GameOver
        {
            get { return _GameOver; }
            set { _GameOver = value; }
        }
	}
}
