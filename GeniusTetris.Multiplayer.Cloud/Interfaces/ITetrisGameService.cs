using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace GeniusTetris.Multiplayer.Cloud.Interfaces
{
    [ServiceContract(Namespace = Constants.ContractNamespace)]
    public interface ITetrisGameService
    {
        [OperationContract(IsOneWay = true)]
        void SendBoard(Guid idplayer, byte[] board, int lengthDimension0, int lengthDimension1);

        [OperationContract(IsOneWay = true)]
        void SetTimerInterval(Guid idplayer, double newValue);

        [OperationContract(IsOneWay = true)]
        void GameOver(Guid idplayer);

        [OperationContract(IsOneWay = true)]
        void SendOption(Guid from, Guid to, byte option);

        [OperationContract(IsOneWay = true)]
        void SendScore(Guid idplayer, int score);

        [OperationContract(IsOneWay = true)]
        void HideBoard(Guid idplayer, bool value);

        [OperationContract(IsOneWay = true)]
        void GameStarting(Guid idCoordinator, int nbPlayer);

        [OperationContract(IsOneWay = true)]
        void PlayerReady(Guid idplayer);

        [OperationContract(IsOneWay = true)]
        void ImIn(Guid idplayer);
    }

    /// <summary>
    /// interface used to send on multicast
    /// </summary>
    public interface ITetrisGameServiceChannel : ITetrisGameService, IClientChannel
    {
    }
}
