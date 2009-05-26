using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusTetris.Core
{
    public interface IGame
    {
        //definir les methodes accessible � distances
        void ExecuteOption(byte option, IGame sender);

        byte[,] GetBoardData();
    }
}
