using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using GeniusTetris.Multiplayer.SignalR.Model;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace GeniusTetris.Server.SignalR
{
    [HubName("h")]
    public class GeniusTetrisGameHub : Hub
    {
        private readonly GeniusTetrisGame _game;
        List<string> _ToLeave = new List<string>();
        public GeniusTetrisGameHub() : this(GeniusTetrisGame.Instance)
        {

        }

        public GeniusTetrisGameHub(GeniusTetrisGame game)
        {
            this._game = game;
            this._game.OnTimeToStartGame += _game_OnTimeToStartGame;
        }

        void _game_OnTimeToStartGame(string gamingRoom)
        {
            GameStarting(gamingRoom, this._game.PlayerCount(gamingRoom));
        }

        public void Join(PeerPlayer player, string acceptorUri, string groupName)
        {
            Trace.WriteLine(string.Format("[Server]| '{0}' join {1}", player.ToString(), groupName));
            player.ConnectionID = Context.ConnectionId;
            player.AcceptorUri = acceptorUri;
            this._game.PlayerJoin(player, groupName);

            this.Groups.Add(player.ConnectionID, groupName);
            this.Clients.OthersInGroup(groupName).PlayerListChanged();
        }

        public IEnumerable<PeerPlayer> GetPlayers(string groupName)
        {
            var players = this._game.GetPlayers(groupName).ToArray();
            return players;
        }

        public void SendGameRequest(string acceptorUri, string gamingRoom)
        {
            var player = this._game.GetPlayer(Context.ConnectionId);
            this._game.NewRoom(gamingRoom, player);
            this.Groups.Add(player.ConnectionID, gamingRoom);

            //recherche du groupe auquel il faut envoyer la requête
            string groupName = this._game.GetGroupNameFromConnectionID(Context.ConnectionId);
            if (groupName == null)
            {
                throw new Exception(string.Format("player {0}/{1} is in any group ???", player.ID, player.NickName));
            }
            this.Clients.OthersInGroup(groupName).AcceptGameRequest(player, acceptorUri);
        }

        public void StopGame(string gamingRoom)
        {
            var player = this._game.GetPlayer(Context.ConnectionId);
            this._game.StopGame(player, gamingRoom);
        }

        public void IWouldLikeToplay(Guid playerID, string acceptorUri)
        {
            var player = this._game.GetPlayer(playerID);
            string gamingRoom;
            if (this._game.PlayerWouldLikeToPlay(player, acceptorUri, out gamingRoom))
            {
                this.Groups.Add(player.ConnectionID, gamingRoom);
                this.Clients.Client(Context.ConnectionId).OnAcceptedPlayer(player, gamingRoom);
            }
            else
                this.Clients.Client(Context.ConnectionId).TooManyPlayers();
        }

        public void IWontPlay(Guid playerID, string acceptorUri)
        {
            var player = this._game.GetPlayer(playerID);
            string gamingRoom;
            if (this._game.PlayerWontPlay(player, acceptorUri, out gamingRoom))
            {
                TryStartGame(gamingRoom);
            }
        }

        private void TryStartGame(string gamingRoom)
        {
            if (this._game.IsAllPlayersReady(gamingRoom))
            {
                GameStarting(gamingRoom, this._game.PlayerCount(gamingRoom));
            }
        }

        public void PlayerReady(string gamingRoom)
        {
            var player = this._game.GetPlayer(Context.ConnectionId);
            Debug.WriteLine(string.Format("[Server]|{0} is ready for game in {1}", player.NickName, gamingRoom));
            this.Clients.OthersInGroup(gamingRoom).PlayerReady(player.ID);

            this._game.AddPlayerReady(gamingRoom);
            TryStartGame(gamingRoom);
        }

        public void ImIn(string gamingRoom, Guid playerId)
        {
            var currentPlayer = this._game.GetPlayer(Context.ConnectionId);
            Debug.WriteLine(string.Format("[Server]|'{0}' said \"i'm in\"", currentPlayer.NickName));
            var player = this._game.GetPlayer(playerId);
            this.Clients.Client(player.ConnectionID).ImIn(currentPlayer.ID);
        }

        public void GameStarting(string gamingRoom, int nbPlayers)
        {
            if (this._game.TryStartGame(gamingRoom))
            {
                Debug.WriteLine("[Server]|starting game in room '{0}' with {1} players ", gamingRoom, nbPlayers);
                //tout le monde doit recevoir cet événement
                this.Clients.Group(gamingRoom).GameStarting(gamingRoom, nbPlayers);
            }
        }

        #region Game events
        public void HideBoard(string gamingRoom, bool value)
        {
            var playerId = this._game.GetPlayer(Context.ConnectionId);
            this.Clients.OthersInGroup(gamingRoom).HideBoard(gamingRoom, playerId.ID, value);
        }

        public void SendBoard(string gamingRoom, byte[,] board)
        {
            var playerId = this._game.GetPlayer(Context.ConnectionId);
            this.Clients.OthersInGroup(gamingRoom).SendBoard(gamingRoom, playerId.ID, board);
        }

        public void SendScore(string gamingRoom, int score)
        {
            var playerId = this._game.GetPlayer(Context.ConnectionId);
            this.Clients.OthersInGroup(gamingRoom).SendScore(gamingRoom, playerId.ID, score);
        }

        public void SendGameOver(string gamingRoom)
        {
            var player = this._game.GetPlayer(Context.ConnectionId);
            this._game.GameOverFor(gamingRoom, player);
            //tout le monde doit recevoir cet événement
            this.Clients.Group(gamingRoom).SendGameOver(gamingRoom, player.ID);
        }

        public void SendOption(string gamingRoom, Guid playerID, byte option)
        {
            Debug.WriteLine("[Server]SendOption {0} to player {1}", option.ToString(), playerID);
            var currentPlayer = this._game.GetPlayer(Context.ConnectionId);
            var toPlayer = this._game.GetPlayer(playerID);
            this.Clients.Client(toPlayer.ConnectionID).SendOption(gamingRoom, currentPlayer.ID, toPlayer.ID, option);
        }
        #endregion

        #region overrides
        public override System.Threading.Tasks.Task OnConnected()
        {
            return base.OnConnected();
        }

        public override System.Threading.Tasks.Task OnDisconnected()
        {
            _ToLeave.Add(Context.ConnectionId);
            Task.Factory.StartNew(CleanLeftPlayerAfterTimeOut);
            return base.OnDisconnected();
        }

        public override System.Threading.Tasks.Task OnReconnected()
        {
            if (_ToLeave.Remove(Context.ConnectionId))
            {
                Trace.WriteLine(Context.ConnectionId + " is reconnected");
            }
            return base.OnReconnected();
        }

        private void CleanLeftPlayerAfterTimeOut()
        {
            ManualResetEventSlim ev = new ManualResetEventSlim(false);
            ev.Wait(30000);
            CleanLeftPlayer();
        }

        private void CleanLeftPlayer()
        {
            var first = _ToLeave.FirstOrDefault();
            _ToLeave.RemoveAt(0);
            if (first != null)
            {
                var groupName = this._game.GetGroupNameFromConnectionID(first);
                var player = this._game.GetPlayer(first);
                this._game.PlayerLeave(first);
                if (!string.IsNullOrWhiteSpace(groupName))
                {
                    this.Clients.OthersInGroup(groupName).Leave(player);
                    if (player != null)
                        this.Groups.Remove(player.ConnectionID, groupName);
                }
            }
        }
        #endregion

    }
}