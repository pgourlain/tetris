using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GeniusTetris.Core;
using GeniusTetris.Multiplayer.SignalR.Model;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Hubs;

namespace GeniusTetris.Multiplayer.SignalR
{
    /// <summary>
    /// connecteur pour SignalR
    /// </summary>
    class TetrisMultiplayerSignalRApplication : TetrisMultiplayerApplicationBase
    {

        HubConnection _connection;
        IHubProxy _hubProxy;
        string _acceptorUri;
        string _gamingRoom;

        List<IDisposable> _mappedEvents;

        string _uriToAccept;
        volatile bool _canReceiveARequest = true;
        int _nbgameOver = 0;

        public TetrisMultiplayerSignalRApplication(ITimer timer) : base(timer)
        {
            _mappedEvents = new List<IDisposable>();
            Configuration cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string url = cfg.AppSettings.Settings["serverUrl"].Value;
            _connection = new HubConnection(url);
            _connection.Credentials = CredentialCache.DefaultNetworkCredentials;
            var proxy = WebRequest.DefaultWebProxy;
            _connection.Proxy = proxy;
            if (proxy != null)
                proxy.Credentials = CredentialCache.DefaultNetworkCredentials;
        }

        #region methods client -> Server
        public override void HideBoard()
        {
            if (IsConnected)
            {
                _hubProxy.Invoke("HideBoard", _gamingRoom, true);
            }
        }

        public override void ShowBoard()
        {
            if (IsConnected)
            {
                _hubProxy.Invoke("HideBoard", _gamingRoom, false);
            }
        }

        public override void SendBoard(byte[,] board)
        {
            _hubProxy.Invoke("SendBoard", _gamingRoom, board);
        }

        public override void SendScore(int score)
        {
            _hubProxy.Invoke("SendScore", _gamingRoom, score);
        }

        public override void SendGameOver()
        {
            _canReceiveARequest = true;
            SendScore(this.CurrentGame.Score);
            _hubProxy.Invoke("SendGameOver", _gamingRoom);
        }

        public override void SendOption(GeniusTetrisPlayer toplayer, byte option)
        {
            Debug.WriteLine("[Client]SendOption {0} to player {1}", option, toplayer.ID);
            _hubProxy.Invoke("SendOption", _gamingRoom, toplayer.ID, option);
        }

        public override void AcceptGameRequest()
        {
            CallAsync(() =>
            {
                WorkingCount++;
                WorkingMessage = "Sending that you are agree to play";
            },
                arg =>
                {
                    _hubProxy.Invoke<string>("IWouldLikeToplay", this.CurrentPlayer.ID, _uriToAccept);
                    return true;
                },
                (result, ex) =>
                {
                    WorkingCount--;
                }, string.Empty);
        }

        public override void RejectGameRequest()
        {
            _canReceiveARequest = true;
            _hubProxy.Invoke<string>("IWontPlay", this.CurrentPlayer.ID, _uriToAccept);
        }

        public override void StartMultiplayerGameWF()
        {
            if (!IsConnected)
                throw new NotSupportedException();
            _canReceiveARequest = false;
            InMultiplayer = true;

            Configuration cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string groupName = cfg.AppSettings.Settings["groupName"].Value;

            _gamingRoom = String.Format("{0}/gamingRoom/{1}", groupName, Guid.NewGuid());
            _hubProxy.Invoke("SendGameRequest", _acceptorUri, _gamingRoom);
            ConnectToGamingRoom(_gamingRoom);
        }

        private void ConnectToGamingRoom(string uriGamingRoom)
        {
            CallAsync(() =>
            {
                WorkingCount++;
                if (uriGamingRoom == null)
                    WorkingMessage = "Unable to join the gaming room";
                else
                    WorkingMessage = "Connecting to the gaming room";
                _gamingRoom = uriGamingRoom; 
                GameMembersList.Clear();
            },
                arg =>
                {
                    if (uriGamingRoom == null)
                    {
                        System.Threading.Thread.Sleep(2000);
                        _canReceiveARequest = true;
                    }
                    else
                    {
                        WaitSignalFromServer();
                    }
                    return true;
                },
                (result, ex) =>
                {
                    WorkingCount--;
                }, string.Empty);
        }

        private void WaitSignalFromServer()
        {
            CallAsync(() =>
            {
                WorkingCount++;
                WorkingMessage = "Waiting signal from server";
            },
            arg =>
            {
                InMultiplayer = true;
                _hubProxy.Invoke("PlayerReady", _gamingRoom);
                System.Threading.Thread.Sleep(30000);
                return true;
            },
            (result, ex) =>
            {
                if (WorkingCount > 0)
                    WorkingCount--;
                if (ex != null)
                {
                    throw ex;
                }
            }, string.Empty);
        }


        public override void StopMultiPlayerGameWF()
        {
            _canReceiveARequest = true;
            InMultiplayer = false;
            _hubProxy.Invoke("StopGame", _gamingRoom);
        }

        public override void ConnectToMesh()
        {
            WorkingCount++;
            Configuration cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            WorkingMessage = "Connecting to " + cfg.AppSettings.Settings["serverUrl"].Value;
            _hubProxy = _connection.CreateHubProxy("h");
            _acceptorUri = string.Format("RoomAcceptor/{0}/", Guid.NewGuid()); 
            string groupName = cfg.AppSettings.Settings["groupName"].Value;
            MapEvents(_hubProxy);
            _connection.Start().ContinueWith(t =>
            {
                _hubProxy.Invoke("Join", new PeerPlayer { ID = CurrentPlayer.ID, NickName = CurrentPlayer.NickName }, _acceptorUri, groupName);
                _hubProxy.Invoke<IEnumerable<PeerPlayer>>("GetPlayers", groupName).ContinueWith(UpdateMembersList);
                CallOnDispatcher(() =>
                    {
                        IsConnected = !t.IsFaulted;
                        WorkingCount--;
                    });
            });
        }

        public override void DisconnectFromMesh()
        {
            WorkingCount++;
            try
            {
                WorkingMessage = "Disconnecting from server";
                _connection.Stop();
                this.PlayersInMeshList.Clear();
            }
            finally
            {
                IsConnected = false;
                WorkingCount--;
            }
        }

        #endregion


        private void UpdateMembersList(Task<IEnumerable<PeerPlayer>> t)
        {
            if (!t.IsFaulted)
            {
                CallOnDispatcher(() =>
                {
                    this.PlayersInMeshList.Clear();
                    foreach (var p in t.Result)
                    {
                        this.PlayersInMeshList.Add(new GeniusTetrisPlayer
                        {
                            ID = p.ID,
                            NickName = p.NickName
                        });
                    }
                });
            }
        }

        private void MapEvents(IHubProxy hubProxy)
        {
            _mappedEvents.AddRange(new IDisposable[]{
                    hubProxy.On<PeerPlayer, string>("AcceptGameRequest", UserAcceptGameRequest),
                    hubProxy.On("PlayerListChanged", PlayerListChanged),
                    hubProxy.On<PeerPlayer>("Leave", PlayerLeaved),
                    hubProxy.On<PeerPlayer, string>("OnAcceptedPlayer", OnAcceptedPlayer),
                    //hubProxy.On<string>("OnAcceptToPlay", OnAcceptToPlay),
                    hubProxy.On("TooManyPlayers", TooManyPlayers),
                    hubProxy.On<string, int>("GameStarting", GameStarting),
                    hubProxy.On<Guid>("PlayerReady", PlayerReady),
                    hubProxy.On<Guid>("ImIn", ImIn),
                    //GameEvent
                    hubProxy.On<string, Guid>("SendGameOver", ReceiveGameOver),
                    hubProxy.On<string, Guid, bool>("HideBoard", ReceiveHideBoard),
                    hubProxy.On<string, Guid, byte[,]>("SendBoard", ReceiveBoard),
                    hubProxy.On<string, Guid, int>("SendScore", ReceiveSendScore),
                    hubProxy.On<string, Guid, Guid, byte>("SendOption", ReceiveSendOption),
                    //hubProxy.On<string, Guid, int>("SetTimerInterval", OnSetTimerInterval),

                }
            );
        }

        private GeniusTetrisPlayer GetPlayer(PeerPlayer player)
        {
            return this.PlayersInMeshList.FirstOrDefault(p => p.ID == player.ID);
        }

        private GeniusTetrisPlayer GetPlayer(Guid playerID)
        {
            return this.PlayersInMeshList.FirstOrDefault(p => p.ID == playerID);
        }

        #region event from server
        #region workflow event
        private void UserAcceptGameRequest(PeerPlayer player, string uri)
        {
            if (_canReceiveARequest)
            {
                _canReceiveARequest = false;
                _uriToAccept = uri;
                CallOnDispatcher(() =>
                {
                    DoGameRequestReceived(GetPlayer(player), string.Format("do you want to play with '{0}'", player.NickName));
                });
            }
        }

        private void PlayerListChanged()
        {
            Configuration cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string groupName = cfg.AppSettings.Settings["groupName"].Value;
            _hubProxy.Invoke<IEnumerable<PeerPlayer>>("GetPlayers", groupName).ContinueWith(UpdateMembersList);
        }

        private void PlayerLeaved(PeerPlayer player)
        {
            CallOnDispatcher(() =>
            {
                var foundPlayer = this.PlayersInMeshList.FirstOrDefault(p => p.ID == player.ID);
                this.PlayersInMeshList.Remove(foundPlayer);
                foundPlayer = this.GameMembersList.FirstOrDefault(p => p.ID == player.ID);
                this.GameMembersList.Remove(foundPlayer);
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player"></param>
        private void OnAcceptedPlayer(PeerPlayer player, string gamingRoom)
        {
            if (player != null && player.ID == CurrentPlayer.ID)
            {
                _gamingRoom = gamingRoom;
                ConnectToGamingRoom(gamingRoom);
            }
        }

        void PlayerReady(Guid idplayer)
        {
            Debug.WriteLine("PlayerReady " + idplayer.ToString());
            TryAddPlayer(idplayer);
            CallOnDispatcher(() =>
            {
                //when player said PlayerReady, you respond i'm in
                _hubProxy.Invoke("ImIn", _gamingRoom, idplayer);
            });
        }


        void GameStarting(string  gamingRoom, int nbPlayer)
        {
            if (gamingRoom == _gamingRoom)
            {
                Debug.WriteLine(string.Format("[Client]GameStarting '{0}' for {1}", gamingRoom, this.CurrentPlayer.ToString()));
                CallOnDispatcher(() =>
                {
                    if (this.WorkingCount > 0)
                        this.WorkingCount = 0;
                    _nbgameOver = 0;
                    this.CurrentPlayer.IsGameOver = false;
                    this.CurrentPlayer.Score = 0;
                    DoStartGameNow(null);
                });
            }
        }

        private void TryAddPlayer(Guid idplayer)
        {
            CallOnDispatcher(() =>
            {
                if (GameMembersList.FirstOrDefault(p => p.ID == idplayer) == null)
                {
                    var player = GetPlayer(idplayer);
                    player.IsGameOver = false;
                    player.Score = 0;
                    System.Diagnostics.Debug.Assert(player != null);
                    GameMembersList.Add(player);
                    DoGameRequestAccepted(player);
                }
            });
        }

        void ImIn(Guid idplayer)
        {
            if (idplayer != this.CurrentPlayer.ID)
            {
                TryAddPlayer(idplayer);
            }
        }


        void OnAcceptToPlay(string gamingRoom)
        {
            ConnectToGamingRoom(gamingRoom);                                
        }

        void TooManyPlayers()
        {

        }
        #endregion


        #region game events
        void ReceiveBoard(string gamingRoom, Guid idplayer, byte[,] board)
        {
            if (idplayer != this.CurrentPlayer.ID)
            {
                CallOnDispatcher(() =>
                    {
                        var player =GetPlayer(idplayer); 
                        DoSendBoard(player, board);
                    });
            }
        }

        void OnSetTimerInterval(Guid idplayer, double newValue)
        {
            //throw new NotImplementedException();
        }

        void ReceiveGameOver(string gamingRoom, Guid idplayer)
        {
            if (gamingRoom == _gamingRoom)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("{0} send gameover", idplayer));
                Interlocked.Increment(ref _nbgameOver);
                CallOnDispatcher(() =>
                    {
                        var player = GetPlayer(idplayer);
                        if (idplayer == this.CurrentPlayer.ID)
                            player = this.CurrentPlayer;
                        player.IsGameOver = true;
                        DoGameOver(player, string.Empty);
                        if (_nbgameOver > GameMembersList.Count)
                            DoEndGameEnd(player, "last player gameover");
                    });
            }
        }

        void ReceiveSendOption(string gamingRoom, Guid from, Guid to, byte option)
        {
            Debug.WriteLine("Receive option {0} from player {1} to player {2}", option, from, to);
            if (to == this.CurrentPlayer.ID)
            {
                CallOnDispatcher(() =>
                    {
                        DoOptionArrived(GetPlayer(from), this.CurrentPlayer, option);
                    });
            }
        }

        void ReceiveSendScore(string gamingRoom, Guid idplayer, int score)
        {
            if (idplayer != this.CurrentPlayer.ID)
            {
                CallOnDispatcher(() =>
                    {
                        var player = GetPlayer(idplayer);
                        player.Score = score;

                        DoSendScore(player, score);
                    });
            }
        }

        void ReceiveHideBoard(string gamingRoom, Guid idplayer, bool value)
        {
            if (idplayer != this.CurrentPlayer.ID)
            {
                CallOnDispatcher(() =>
                    {
                        DoHideBoard(GetPlayer(idplayer), value);
                    });
            }
        }
        #endregion
        #endregion

        private void UnMapEvents()
        {
            _mappedEvents.ForEach(x => x.Dispose());
            _mappedEvents.Clear();
        }

        #region static methods
        public static T[] From2Dto1D<T>(T[,] TwoDarray)
        {
            int k = 0;
            int upper0 = TwoDarray.GetUpperBound(0) + 1;
            int upper1 = TwoDarray.GetUpperBound(1) + 1;
            T[] oneDArray = new T[upper0 * upper1];
            for (int i = 0; i < upper0; i++)
            {
                for (int j = 0; j < upper1; j++)
                {
                    oneDArray[k] = TwoDarray[i, j];
                    k++;
                }
            }

            return oneDArray;
        }


        public static T[,] From1Dto2D<T>(T[] oneDarray, int length1, int length2)
        {
            int k = 0;
            T[,] twoDArray = new T[length1, length2];
            for (int i = 0; i < length1; i++)
            {
                for (int j = 0; j < length2; j++)
                {
                    twoDArray[i, j] = oneDarray[k];
                    k++;
                }
            }

            return twoDArray;
        }
        #endregion

    }
}
