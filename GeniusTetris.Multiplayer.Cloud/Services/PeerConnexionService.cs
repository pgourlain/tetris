using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Diagnostics;
using GeniusTetris.Multiplayer.Cloud.Interfaces;

namespace GeniusTetris.Multiplayer.Cloud.Services
{
    [ServiceBehavior(Namespace = Constants.ContractNamespace, InstanceContextMode = InstanceContextMode.Single)]
    public class PeerConnexionService : IPeerConnexionService
    {
        HashSet<PeerPlayer> _players = new HashSet<PeerPlayer>();

        #region IPeerConnexion Members

        public void Join(PeerPlayer player)
        {
            if (IsPeerReady)
            {
                if (player != CurrentPlayer)
                {
                    Debug.WriteLine(string.Format("{0} joins", player));
                    //other than me joins
                    AddPlayer(player);
                    ClientPeerConnexion.Hello(CurrentPlayer.ToPlayer());
                }
            }
        }

        public void Leave(PeerPlayer player)
        {
            if (IsPeerReady)
            {
                Debug.WriteLine(string.Format("{0} leaves", player));
                if (player != CurrentPlayer)
                {
                    RemovePlayer(player);
                    OnPlayerLeaves(player);
                }
            }
        }

        public void Hello(PeerPlayer player)
        {
            if (IsPeerReady)
            {
                if (player != CurrentPlayer)
                {
                    Debug.WriteLine(string.Format("{0} say hello", player));
                    AddPlayer(player);
                }
            }
        }

        public void Ping(PeerPlayer from, PeerPlayer to)
        {
            Debug.WriteLine(string.Format("{0} ping {1}", from, to));
            if (IsPeerReady)
            {
                if (to == CurrentPlayer)
                {
                    ClientPeerConnexion.Pong(CurrentPlayer.ToPlayer(), from);
                }
            }
        }

        public void Pong(PeerPlayer from, PeerPlayer to)
        {
            Debug.WriteLine(string.Format("{0} pong {1}", from, to));
        }

        #endregion

        protected virtual bool IsPeerReady
        {
            get
            {
                return CurrentPlayer != null && ClientPeerConnexion != null;
            }
        }

        public PeerPlayer CurrentPlayer { get; set; }
        public IPeerConnexionService ClientPeerConnexion { get; set; }


        private void AddPlayer(PeerPlayer player)
        {
            lock (_players)
            {
                if (!_players.Contains(player))
                {
                    _players.Add(player);
                    System.Threading.Monitor.Pulse(_players);
                    PlayerListChanged();
                }
            }
        }

        private void RemovePlayer(PeerPlayer player)
        {
            lock (_players)
            {
                if (_players.Contains(player))
                {
                    _players.Remove(player);
                    System.Threading.Monitor.Pulse(_players);
                    PlayerListChanged();
                }
            }
        }

        protected virtual void PlayerListChanged()
        {
            if (OnPlayerListChanged != null)
                OnPlayerListChanged();
        }

        protected virtual void OnPlayerLeaves(PeerPlayer player)
        {
            if (OnPlayerLeaved != null)
                OnPlayerLeaved(player);
        }

        public IEnumerable<PeerPlayer> Players
        {
            get
            {
                lock (_players)
                {
                    return _players.ToList();
                }
            }
        }

        public event Action<PeerPlayer> OnPlayerLeaved;
        public event Action OnPlayerListChanged;
        public event Action<PeerPlayer, Uri> OnRequestReceived;

        #region IPeerConnexionService Members


        public void SendGameRequest(PeerPlayer player, Uri acceptor)
        {
            if (OnRequestReceived != null)
            {
                OnRequestReceived(player, acceptor);
            }
        }

        #endregion
    }
}
