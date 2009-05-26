using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeniusTetris.Multiplayer
{
    public class GeniusTetrisPlayer
    {
        public int Score { get; set; }
        public Guid ID { get; set; }
        public string NickName { get; set; }
        public bool IsGameOver { get; set; }
    }
}
