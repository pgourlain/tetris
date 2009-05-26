using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace GeniusTetris.Multiplayer.Cloud.Interfaces
{
    [ServiceContract(Namespace = Constants.ContractNamespace)]
    public interface IPeerConnexionService
    {
        #region connection managment
        /// <summary>
        /// used to join, others player should respond with hello 
        /// </summary>
        /// <param name="id">unique id of the member</param>
        /// <param name="nickName">friendly user name</param>
        [OperationContract(IsOneWay = true)]
        void Join(PeerPlayer player);

        /// <summary>
        /// used to leave the room
        /// </summary>
        /// <param name="id"></param>
        [OperationContract(IsOneWay = true)]
        void Leave(PeerPlayer player);

        /// <summary>
        /// the new player receive one "Hello" from all other players
        /// </summary>
        /// <param name="id"></param>
        /// <param name="displayMember"></param>
        [OperationContract(IsOneWay = true)]
        void Hello(PeerPlayer player);

        /// <summary>
        /// used to send a ping to a specific player
        /// </summary>
        /// <param name="player"></param>
        [OperationContract(IsOneWay = true)]
        void Ping(PeerPlayer from, PeerPlayer to);

        /// <summary>
        /// used to respond from ping
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        [OperationContract(IsOneWay = true)]
        void Pong(PeerPlayer from, PeerPlayer to);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="acceptor">uri of the acceptor service</param>
        [OperationContract(IsOneWay = true)]
        void SendGameRequest(PeerPlayer player, Uri acceptor);
        #endregion
    }

    /// <summary>
    /// interface used to send on multicast
    /// </summary>
    public interface IPeerConnexionServiceChannel : IPeerConnexionService, IClientChannel
    {
    }
}
