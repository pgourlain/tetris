using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using GeniusTetris.Multiplayer.SignalR.Model;

namespace GeniusTetris.Server.SignalR
{
    public class GameRoom
    {
        const int MAXPLAYERS = 9;
        public GameRoom()
        {
            Players = new List<PeerPlayer>();
            Started = false;
            //new Timer(OnTimer, null, 15000, System.Threading.Timeout.Infinite);
            Task.Factory.StartNew(OnTimerAfterTimeOut);
        }

        private void OnTimerAfterTimeOut()
        {
            ManualResetEventSlim ev = new ManualResetEventSlim();
            ev.Wait(15000);
            OnTimer(null);
        }

        public bool Started { get; set; }

        public event EventHandler OnTimeToStartGame;

        public PeerPlayer Owner { get; set; }
        public string RoomName { get; set; }


        private void OnTimer(object state)
        {
            if (OnTimeToStartGame != null)
            {
                OnTimeToStartGame(this, EventArgs.Empty);
            }
        }

        public List<PeerPlayer> Players
        {
            get;
            private set;
        }

        public int ReadyPlayerCount { get; set; }

        public bool IsAllPlayersReady
        {
            get
            {
                //prêt quand le nombre est atteint
                var maxNumberReached = (ReadyPlayerCount == MAXPLAYERS);
                //prêt quand quand le nombre de joueurs == le nombre dans le groupe
                var allPlayersAreReady = ReadyPlayerCount >= (PlayersInGroup.Count - RefusedCount);                
                return maxNumberReached || allPlayersAreReady;
            }
        }

        /// <summary>
        /// la liste des joueurs dans le group ou ils se sont connectés
        /// </summary>
        public List<PeerPlayer> PlayersInGroup { get; internal set; }

        public int RefusedCount { get; set; }

        internal bool TryAdd(PeerPlayer player)
        {
            if (Started)
                return false;
           lock (((ICollection)this.Players).SyncRoot)
            {
                if (this.Players.Count < MAXPLAYERS)
                {
                    this.Players.Add(player);
                    return true;
                }
            }
            return false;
        }

        public int GameOverCount { get; set; }
    }
}