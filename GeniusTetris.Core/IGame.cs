using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusTetris.Core
{
    public interface IGame
    {
        //definir les methodes accessible à distances
        void ExecuteOption(byte option, IGame sender);

        byte[,] GetBoardData();
    }
}
