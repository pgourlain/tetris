using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;

namespace GeniusP2PManager
{
    // Chat service contract
    [ServiceContract(Namespace = "GameChat", CallbackContract = typeof(IGameChat))]
    public interface IGameChat
    {
        /// <summary>
        /// se join à un maillage
        /// </summary>
        /// <param name="member"></param>
        [OperationContract(IsOneWay = true)]
        void Join(Guid member, string displayMember);

        /// <summary>
        /// quand un joueur se join au maillage tout le monde le dit 'bonjour'
        /// </summary>
        /// <param name="member"></param>
        [OperationContract(IsOneWay = true)]
        void Hello(Guid member, string displayMember);

        /// <summary>
        /// envoyer un message à toute personnes du maillage
        /// </summary>
        /// <param name="member"></param>
        /// <param name="msg"></param>
        [OperationContract(IsOneWay = true)]
        void Chat(Guid member, string msg);

        /// <summary>
        /// quitter le maillage
        /// </summary>
        /// <param name="member"></param>
        [OperationContract(IsOneWay = true)]
        void Leave(Guid member);

        /// <summary>
        /// le serveur appel cette méthode pour commencer un jeu multijoueur
        /// </summary>
        /// <param name="member"></param>
        [OperationContract(IsOneWay = true)]
        void SendGameRequest(Guid member, string message);

        /// <summary>
        /// chaque joueur peut accepter l'invitation lancer par SendGameRequest
        /// </summary>
        /// <param name="member"></param>
        [OperationContract(IsOneWay = true)]
        void GameRequestAccepted(Guid member);

        /// <summary>
        /// chaque joueur peut refuser l'invitation lancer par SendGameRequest
        /// </summary>
        /// <param name="member"></param>
        [OperationContract(IsOneWay = true)]
        void GameRequestRejected(Guid member);

        /// <summary>
        /// demarrage effectif du jeu, le serveur execute la methode StartFromHost()
        /// </summary>
        /// <param name="member"></param>
        [OperationContract(IsOneWay = true)]
        void StartGame(Guid member);
    }
}
