using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeniusTetris.Core;
using Microsoft.ServiceBus;
using System.Configuration;
using GeniusTetris.Multiplayer.Cloud.Helpers;
using GeniusTetris.Multiplayer.Cloud.Interfaces;
using GeniusTetris.Multiplayer.Cloud.Services;
using System.ServiceModel;
using System.Threading;

namespace GeniusTetris.Multiplayer.Cloud
{
    using MCConnectionService = MulticastHost<PeerConnexionService, IPeerConnexionService, IPeerConnexionServiceChannel>;
    using MCGameService = MulticastHost<TetrisGameService, ITetrisGameService, ITetrisGameServiceChannel>;
    using System.Net;

    public class TetrisMultiplayerCloudApplication : TetrisMultiplayerApplicationBase, ITetrisGameService
    {
        MCConnectionService _cnx;
        ServiceHost _hostAcceptor;
        IAcceptorService _acceptor;
        Uri _acceptorUri;
        bool _ImOwnerOfGame;
        int nbPlayerReady;

        Uri _gamingRoom;
        MCGameService _gamesvc;
        ManualResetEvent _quitGame = new ManualResetEvent(true);
        Uri _uriToAccept;
        volatile bool canReceiveARequest = true;

        public TetrisMultiplayerCloudApplication(ITimer timer)
            : base(timer)
        {
        }

        #region connection managment
        bool _inexiting;
        public override void ExitGame()
        {
            _inexiting = true;
            base.ExitGame();
            _quitGame.WaitOne(10000);
        }

        public override void ConnectToMesh()
        {
            CallAsync(() =>
                {
                    WorkingCount++;
                    WorkingMessage = "Connecting to the cloud";
                },
                arg =>
                {
                    Configuration cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    string solutionName = cfg.AppSettings.Settings["CloudSolutionName"].Value;
                    Uri uri = ServiceBusEnvironment.CreateServiceUri("sb", solutionName, "GeniusTetrisRoomService/MulticastService/");
                    _cnx = new MCConnectionService(uri, "RomServiceRelayEndpoint");
                    var player = new PeerPlayer { ID = this.CurrentPlayer.ID, NickName = this.CurrentPlayer.NickName };
                    _cnx.AfterStart = sender =>
                    {
                        sender.Receiver.OnPlayerLeaved += new Action<PeerPlayer>(Receiver_OnPlayerLeaved);
                        sender.Receiver.OnPlayerListChanged += new Action(Receiver_OnPlayerListChanged);
                        sender.Receiver.OnRequestReceived += new Action<PeerPlayer, Uri>(Receiver_OnRequestReceived);
                        sender.Receiver.ClientPeerConnexion = sender.Sender;
                        sender.Receiver.CurrentPlayer = player;
                    };

                    _cnx.BeforeShutdown = sender =>
                    {
                        sender.Receiver.OnPlayerLeaved -= new Action<PeerPlayer>(Receiver_OnPlayerLeaved);
                        if (sender.Sender != null)
                            sender.Sender.Leave(player);
                    };
                    _cnx.Start();
                    CallOnDispatcher(() =>
                        {
                            WorkingMessage = "Publishing acceptor service on the cloud";
                        });

                    WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultNetworkCredentials;
                    _acceptor = new AcceptorService(this.OnAcceptPlayer);
                    _acceptorUri = uri = ServiceBusEnvironment.CreateServiceUri("sb", solutionName, string.Format("RoomAcceptor/{0}/", Guid.NewGuid()));
                    _hostAcceptor = new ServiceHost(_acceptor, uri);
                    _hostAcceptor.Open();
                    CallOnDispatcher(() =>
                    {
                        WorkingMessage = "Joining the cloud...";
                    }); 
                    _cnx.Sender.Join(player);
                    return true;
                },
                (result, ex) =>
                {
                    IsConnected = ex == null;
                    WorkingCount--;
                    if (ex != null)
                        throw ex;
                }, string.Empty);
        }

        public override void DisconnectFromMesh()
        {
            InMultiplayer = false;
            _quitGame.Reset();
            CallAsync(() =>
            {
                WorkingCount++;
                WorkingMessage = "Disconnecting";
            },
            arg =>
            {
                try
                {
                    _ImOwnerOfGame = false;
                    //TODO: stop consuming playing service
                    if (_gamesvc != null)
                    {
                        _gamesvc.Shutdown();
                        _gamesvc = null;
                    }
                    if (_cnx != null)
                    {
                        _cnx.Shutdown();
                        _cnx = null;
                    }
                    if (!_inexiting)
                    {
                        //because in exiting an event is used to wait end of connections
                        CallOnDispatcher(() =>
                        {
                            WorkingMessage = "Unpublishing acceptor service";
                        });
                    }
                    if (_hostAcceptor != null && _hostAcceptor.State == CommunicationState.Opened)
                    {
                        _hostAcceptor.Close();
                    }
                    _hostAcceptor = null;
                }
                finally
                {
                    _quitGame.Set();
                }
                return true;
            },
            (result, ex) =>
            {
                IsConnected = false;
                WorkingCount--;
            }, string.Empty);
        }

        void Receiver_OnPlayerListChanged()
        {
            CallOnDispatcher(() =>
            {
                this.PlayersInMeshList.Clear();
                foreach (var p in _cnx.Receiver.Players)
                {
                    this.PlayersInMeshList.Add(new GeniusTetrisPlayer
                    {
                        ID = p.ID,
                        NickName = p.NickName
                    });
                }
            });
        }

        void Receiver_OnPlayerLeaved(PeerPlayer player)
        {
            CallOnDispatcher(() =>
            {
                var foundPlayer = this.PlayersInMeshList.FirstOrDefault(p => p.ID == player.ID);
                this.PlayersInMeshList.Remove(foundPlayer);
                foundPlayer = this.GameMembersList.FirstOrDefault(p => p.ID == player.ID);
                this.GameMembersList.Remove(foundPlayer);
            });
        }
        #endregion

        #region starting game workflow
        public override void StartMultiplayerGameWF()
        {
            if (!IsConnected)
                throw new NotSupportedException();
            _ImOwnerOfGame = true;
            nbPlayerReady = 0;
            canReceiveARequest = false;
            InMultiplayer = true;
            Configuration cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string solutionName = cfg.AppSettings.Settings["CloudSolutionName"].Value;

            _gamingRoom = ServiceBusEnvironment.CreateServiceUri("sb", solutionName, string.Format("{0}/MulticastService/", Guid.NewGuid()));
            ConnectToGamingRoom(_gamingRoom);
            //_acceptor.IWouldLikeToplay(this._cnx.Receiver.CurrentPlayer);
            _cnx.Sender.SendGameRequest(this._cnx.Receiver.CurrentPlayer, _acceptorUri);
        }

        public override void StopMultiPlayerGameWF()
        {
            canReceiveARequest = true;
            _ImOwnerOfGame = false;
            InMultiplayer = true;
        }

        Uri OnAcceptPlayer(PeerPlayer player)
        {
            if (_ImOwnerOfGame && !this.CurrentGame.Started)
            {
                //todo: count the number of incomming connection to limit it to 9
                CallOnDispatcher(() =>
                {
                    System.Diagnostics.Debug.Assert(player != null);
                    var p = GetPlayer(player);
                    System.Diagnostics.Debug.Assert(p != null);
                    GameMembersList.Add(p);
                    DoGameRequestAccepted(p);
                });
                return _gamingRoom;
            }
            return null;
        }
        #endregion

        #region game as client 
        void Receiver_OnRequestReceived(PeerPlayer player, Uri uri)
        {
            if (canReceiveARequest && player.ID != this.CurrentPlayer.ID)
            {
                canReceiveARequest = false;
                _uriToAccept = uri;
                CallOnDispatcher(() =>
                {
                    DoGameRequestReceived(GetPlayer(player), string.Format("do you want to play with '{0}'", player.NickName));
                });
            }
        }

        /// <summary>
        /// send to the server that i accept his game
        /// </summary>
        public override void AcceptGameRequest()
        {
            CallAsync(() =>
                {
                    WorkingCount++;
                    WorkingMessage = "Sending that you are agree to play";
                },
                arg =>
                {
                    ChannelFactory<IAcceptorServiceChannel> factory = new ChannelFactory<IAcceptorServiceChannel>("AcceptorRelayEndpoint", new EndpointAddress(_uriToAccept));
                    var client = factory.CreateChannel();
                    client.Open();
                    try
                    {
                        var uriGamingRoom = client.IWouldLikeToplay(this._cnx.Receiver.CurrentPlayer);
                        System.Diagnostics.Debug.Write("uri of gaming room : ");
                        System.Diagnostics.Debug.WriteLine(uriGamingRoom);
                        ConnectToGamingRoom(uriGamingRoom);
                    }
                    finally
                    {
                        client.Close();
                    }
                    factory.Close();
                    return true;
                },
                (result, ex) =>
                {
                    WorkingCount--;
                }, string.Empty);
        }

        private void ConnectToGamingRoom(Uri uriGamingRoom)
        {
            CallAsync(() =>
                {
                    WorkingCount++;
                    if (uriGamingRoom == null)
                        WorkingMessage = "Unable to join the gaming room";
                    else
                        WorkingMessage = "Connecting to the gaming room";
                    GameMembersList.Clear();
                },
                arg =>
                {
                    if (uriGamingRoom == null)
                    {
                        System.Threading.Thread.Sleep(2000);
                        canReceiveARequest = true;
                    }
                    else
                    {
                        _gamesvc = new MCGameService(uriGamingRoom, "GameTetrisRelayEndpoint");
                        _gamesvc.AfterStart = sender =>
                            {
                                sender.Receiver.Inner = this;
                            };
                        _gamesvc.Start();
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
                _gamesvc.Sender.PlayerReady(this.CurrentPlayer.ID);
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

        public override void SendBoard(byte[,] board)
        {
            if (IsConnected && _gamesvc != null)
            {
                //Send my board on a separate thread
                System.Threading.ThreadPool.QueueUserWorkItem(delegate
                {                    
                    _gamesvc.Sender.SendBoard(this.CurrentPlayer.ID, From2Dto1D<byte>(board),
                                            board.GetUpperBound(0) + 1,
                                            board.GetUpperBound(1) + 1);
                });
            }
        }

        public override void SendGameOver()
        {
            if (IsConnected && _gamesvc != null)
            {
                _gamesvc.Sender.SendScore(this.CurrentPlayer.ID, this.CurrentGame.Score);
                System.Diagnostics.Debug.WriteLine("sending gameover");
                _gamesvc.Sender.GameOver(this.CurrentPlayer.ID);
            }
            canReceiveARequest = true;
            _ImOwnerOfGame = false;
        }

        public override void HideBoard()
        {
            if (IsConnected && _gamesvc != null)
            {
                _gamesvc.Sender.HideBoard(this.CurrentPlayer.ID, true);
            }
        }

        public override void ShowBoard()
        {
            if (IsConnected && _gamesvc != null)
            {
                _gamesvc.Sender.HideBoard(this.CurrentPlayer.ID, false);
            }
        }

        public override void SendOption(GeniusTetrisPlayer toplayer, byte option)
        {
            if (IsConnected && _gamesvc != null)
            {
                _gamesvc.Sender.SendOption(this.CurrentPlayer.ID, toplayer.ID, option);
            }
        }
        #endregion

        public override void RejectGameRequest()
        {
            canReceiveARequest = true;
        }

        private GeniusTetrisPlayer GetPlayer(PeerPlayer player)
        {
            return this.PlayersInMeshList.FirstOrDefault(p => p.ID == player.ID);
        }

        private GeniusTetrisPlayer GetPlayer(Guid playerID)
        {
            return this.PlayersInMeshList.FirstOrDefault(p => p.ID == playerID);
        }

        #region ITetrisGameService Members (reception of all messages)

        void ITetrisGameService.SendBoard(Guid idplayer, byte[] board, int lengthDimension0, int lengthDimension1)
        {
            if (idplayer != this.CurrentPlayer.ID)
            {
                var board2D = From1Dto2D<byte>(board, lengthDimension0, lengthDimension1);
                CallOnDispatcher(() =>
                    {
                        var player =GetPlayer(idplayer); 
                        DoSendBoard(player, board2D);
                    });
            }
        }

        void ITetrisGameService.SetTimerInterval(Guid idplayer, double newValue)
        {
            //throw new NotImplementedException();
        }

        int nbgameOver = 0;
        void ITetrisGameService.GameOver(Guid idplayer)
        {
            System.Diagnostics.Debug.WriteLine(string.Format("{0} send gameover", idplayer));
            Interlocked.Increment(ref nbgameOver);
            CallOnDispatcher(() =>
                {
                    var player = GetPlayer(idplayer);
                    if (idplayer == this.CurrentPlayer.ID)
                        player = this.CurrentPlayer;
                    player.IsGameOver = true;
                    DoGameOver(player, string.Empty);
                    if (nbgameOver > GameMembersList.Count)
                        DoEndGameEnd(player, "last player gameover");
                });
        }

        void ITetrisGameService.SendOption(Guid from, Guid to, byte option)
        {
            if (to == this.CurrentPlayer.ID)
            {
                CallOnDispatcher(() =>
                    {
                        DoOptionArrived(GetPlayer(from), this.CurrentPlayer, option);
                    });
            }
        }

        void ITetrisGameService.SendScore(Guid idplayer, int score)
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

        void ITetrisGameService.HideBoard(Guid idplayer, bool value)
        {
            if (idplayer != this.CurrentPlayer.ID)
            {
                CallOnDispatcher(() =>
                    {
                        DoHideBoard(GetPlayer(idplayer), value);
                    });
            }
        }

        void ITetrisGameService.GameStarting(Guid idCoordinator, int nbPlayer)
        {
            CallOnDispatcher(() =>
                {
                    if (this.WorkingCount > 0)
                        this.WorkingCount = 0;
                    nbgameOver = 0;
                    this.CurrentPlayer.IsGameOver = false;
                    this.CurrentPlayer.Score = 0;
                    DoStartGameNow(null);
                });
        }

        void ITetrisGameService.PlayerReady(Guid idplayer)
        {
            if (idplayer != this.CurrentPlayer.ID)
            {
                if (!_ImOwnerOfGame)
                {
                    TryAddPlayer(idplayer);
                    CallOnDispatcher(() =>
                        {
                            //when player said PlayerReady, you respond i'm in
                            _gamesvc.Sender.ImIn(this.CurrentPlayer.ID);
                        });
                }
                else
                {
                    CallOnDispatcher(() =>
                        {
                            //when player said PlayerReady, you respond i'm in
                            _gamesvc.Sender.ImIn(this.CurrentPlayer.ID);
                            if (Interlocked.Increment(ref nbPlayerReady) == GameMembersList.Count)
                            {
                                _gamesvc.Sender.GameStarting(this.CurrentPlayer.ID, GameMembersList.Count);
                            }
                        });
                }
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

        void ITetrisGameService.ImIn(Guid idplayer)
        {
            if (!_ImOwnerOfGame && idplayer != this.CurrentPlayer.ID)
            {
                TryAddPlayer(idplayer);
            }
        }

        #endregion

        #region static methods
        public static T[] From2Dto1D<T>(T[,] TwoDarray)
        {
            int k = 0;
            int upper0 = TwoDarray.GetUpperBound(0)+1;
            int upper1 = TwoDarray.GetUpperBound(1)+1;
            T[] oneDArray = new T[upper0*upper1];
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
