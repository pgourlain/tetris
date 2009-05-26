using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.Configuration;
using System.ServiceModel.PeerResolvers;
using System.Threading;

namespace GeniusP2PManager
{
    /// <summary>
    /// Application de base pour la gestion d'un jeu multijoueur, l'utilisateur doit forcement dérivé de Player
    /// </summary>
    public class GeniusGameApp<Player, Participant> : IGameChat, IDisposable
        where Player : BasePlayer, new()
        where Participant : IGameChat, IClientChannel
    {
        #region fields
        private string _DisplayMember;
        private Guid _member;
        private string _meshName;
        protected Participant _Participant;
        private string endPoint;
        protected int numberOfPlayersInMesh = 0;
        protected int numberofResponses = 0;
        private int _MaxPlayers = -1; //no limit
        protected Dispatcher _Dispatcher = null;

        //For search
        private Dictionary<Guid, string> _PlayersInMesh;
        private Dictionary<Guid, Player> gameMembers;
        private ObservableCollection<string> _PlayersInMeshList;
        private ObservableCollection<Player> _PlayersInGame;

        #endregion

        /// <summary>
        /// List of player witch accepted the game request, not players in the mesh
        /// </summary>
        public Dictionary<Guid, Player> GameMembers
        {
            get 
            { 
                return gameMembers; 
            }
        }

        public IDictionary<Guid, string> PlayersInMesh
        {
            get
            {
                return _PlayersInMesh;
            }
        }

        public ObservableCollection<string> PlayersInMeshList
        {
            get
            {
                return _PlayersInMeshList;
            }
        }

        public ObservableCollection<Player> GameMembersList
        {
            get
            {
                return _PlayersInGame;
            }
        }

        /// <summary>
        /// Find a player in list
        /// </summary>
        /// <param name="aMemberID"></param>
        /// <returns></returns>
        public Player Find(string aMemberID)
        {
            //Why not a dictionnary, because two players can use the same id...
            foreach (Player p in GameMembers.Values)
            {
                if (p.DisplayName == aMemberID)
                    return p;
            }
            return null;
        }

        /// <summary>
        /// The Local player
        /// </summary>
        public Guid Member
        {
            get { return _member; }
            set { _member = value; }
        }

        public string DisplayMember
        {
            get
            {
                return _DisplayMember;
            }
            set
            {
                _DisplayMember = value;
            }
        }
        /// <summary>
        /// Name of mesh
        /// </summary>
        public string MeshName
        {
            get { return _meshName; }
            set { _meshName = value; }
        }

        public int MaxPlayers
        {
            get { return _MaxPlayers; }
            set { _MaxPlayers = value;}
        }

        public GeniusGameApp(string memberName, string meshName)
        {
            _member = Guid.NewGuid();
            //load config
            _DisplayMember = memberName;
            _meshName = meshName;
            if (string.IsNullOrEmpty(_DisplayMember))
                _DisplayMember = ConfigurationManager.AppSettings["nickname"];
            if (string.IsNullOrEmpty(_DisplayMember))
                throw new Exception("membername is mandatory, you can specify it in your config file at <appSettings><add key=\"member\" value=\"genius\" /></appSettings>");
            if (string.IsNullOrEmpty(_meshName))
                _meshName = ConfigurationManager.AppSettings["defaultMeshName"];
            if (string.IsNullOrEmpty(_meshName))
                throw new Exception("meshname is mandatory, you can specify it in your config file at <appSettings><add key=\"defaultMeshName\" value=\"GeniusMesh\" /></appSettings>");

            gameMembers = new Dictionary<Guid, Player>();
            _PlayersInMesh = new Dictionary<Guid, string>();
            _PlayersInGame = new ObservableCollection<Player>();
            _PlayersInMeshList = new ObservableCollection<string>();
            this.OnGameRequestAccepted += new EventHandler<MessageReceivedEventArgs>(GeniusGameApp_RaiseGameResponse);
            this.OnGameRequestRejected += new EventHandler<MessageReceivedEventArgs>(GeniusGameApp_RaiseGameResponse);
        }

        public GeniusGameApp(Dispatcher d, string memberName, string meshName)
            : this(memberName, meshName)
        {
            this._Dispatcher = d;
        }

        #region events for local player like a proxy from the mesh
        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;

        public event EventHandler<GameRequestReceivedEventArgs> OnGameRequestReceived;

        public event EventHandler<MessageReceivedEventArgs> OnGameRequestAccepted;

        public event EventHandler<MessageReceivedEventArgs> OnGameRequestRejected;

        public event EventHandler<MessageReceivedEventArgs> GameRequestResponsesReceived;

        public event EventHandler<MessageReceivedEventArgs> StartGameNow;

        public event EventHandler<MessageReceivedEventArgs> PlayerLeaveEvent;

        public event EventHandler<MessageReceivedEventArgs> PlayerJoinEvent;
        #endregion

        #region IGameChat implementation 
        /// <summary>
        /// A player join the mesh
        /// </summary>
        /// <param name="member"></param>
        void IGameChat.Join(Guid member, string displayMember)
        {
            numberOfPlayersInMesh++;
            Console.WriteLine("[{0} joined]", member);
            if (!_PlayersInMesh.ContainsKey(member))
            {
                _PlayersInMesh.Add(member, displayMember);
                _PlayersInMeshList.Add(displayMember);
            }
            if (Member != member)
            {
                CallOnDispatcher(delegate()
                {
                    if (PlayerJoinEvent != null)
                    {
                        MessageReceivedEventArgs e = new MessageReceivedEventArgs(displayMember, string.Format("'{0}' has join the mesh", displayMember));
                        PlayerJoinEvent(this, e);
                    }
                    _Participant.Hello(this.Member, this._DisplayMember);
                });
            }
        }

        void IGameChat.Hello(Guid member, string displayMember)
        {
            if (this.Member != member)
            {
                if (!_PlayersInMesh.ContainsKey(member))
                {
                    _PlayersInMesh.Add(member, displayMember);
                    _PlayersInMeshList.Add(displayMember);
                }
            }
        }

        void IGameChat.Chat(Guid member, string msg)
        {
            if (member != this._member)
            {
                string displayMember = _PlayersInMesh[member];
                MessageReceivedEventArgs e = new MessageReceivedEventArgs(displayMember, msg);

                if (this._Dispatcher != null)
                {
                    this._Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                    {
                        OnMessageReceived(this, e);
                    });
                }
                else
                {
                    OnMessageReceived(this, e);
                }

                Console.WriteLine("[{0}] {1}", member, msg);
            }
        }

        void IGameChat.StartGame(Guid member)
        {
            CallOnDispatcher(delegate()
            {
                if (StartGameNow != null)
                {
                    MessageReceivedEventArgs e = new MessageReceivedEventArgs(null, _DisplayMember);
                    StartGameNow(this, e);
                }

            });
        }

        void IGameChat.Leave(Guid member)
        {
            numberOfPlayersInMesh--;
            Console.WriteLine("[{0} left]", member);

            Player playerToRemove = default(Player);
            string displayMember = string.Empty;
            if (_PlayersInMesh.ContainsKey(member))
            {
                displayMember = _PlayersInMesh[member];
                _PlayersInMesh.Remove(member);
                _PlayersInMeshList.Remove(displayMember);
            }

            CallOnDispatcher(delegate()
            {
                if (gameMembers.ContainsKey(member))
                {
                    playerToRemove = gameMembers[member];
                    gameMembers.Remove(member);
                    _PlayersInGame.Remove(playerToRemove);
                }
                if (PlayerLeaveEvent != null)
                {
                    MessageReceivedEventArgs e = new MessageReceivedEventArgs(displayMember, string.Format("'{0}' leave the mesh", displayMember));
                    PlayerLeaveEvent(this, e);
                }
            });
        }

        void IGameChat.SendGameRequest(Guid member, string message)
        {
            numberofResponses = 0;
            gameMembers.Clear();
            _PlayersInGame.Clear();
            if (this.Member != member)
            {
                CallOnDispatcher(delegate()
                {
                    if (OnGameRequestReceived != null)
                    {
                        string displayMember = _PlayersInMesh[member];
                        GameRequestReceivedEventArgs arg = new GameRequestReceivedEventArgs(displayMember, message);
                        OnGameRequestReceived(this, arg);
                    }
                });
            }
        }

        void IGameChat.GameRequestAccepted(Guid member)
        {
            if (_MaxPlayers <= 0 || (_MaxPlayers < numberofResponses))
            {
                CallOnDispatcher(delegate()
                {
                    Player newPlayer = new Player();
                    newPlayer.DisplayName = _PlayersInMesh[member];
                    newPlayer.ID = member;
                    gameMembers.Add(member, newPlayer);
                    _PlayersInGame.Add(newPlayer);
                    if (this._member != member)
                    {
                        if (OnGameRequestAccepted != null)
                        {
                            MessageReceivedEventArgs e = new MessageReceivedEventArgs(newPlayer.DisplayName, newPlayer.DisplayName);
                            OnGameRequestAccepted(this, e);
                        }
                    }
                });
            }
        }

        void IGameChat.GameRequestRejected(Guid member)
        {
            if (this._member != member)
            {
                CallOnDispatcher(delegate()
                {
                    if (OnGameRequestRejected != null)
                    {
                        MessageReceivedEventArgs e = new MessageReceivedEventArgs(_PlayersInMesh[member], "");
                        OnGameRequestRejected(this, e);
                    }
                });
            }
        }
        #endregion

        #region CallOnDispatcher

        protected delegate void CallOnDispatcherDelegate();
        
        /// <summary>
        /// call delegate provided in parameter on dispatcher if it's not null
        /// </summary>
        /// <param name="aDelegate"></param>
        protected void CallOnDispatcher(CallOnDispatcherDelegate aDelegate)
        {
            if (this._Dispatcher != null)
            {
                this._Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate()
                {
                    aDelegate();
                });
            }
            else
                aDelegate();
        }
        #endregion

        void GeniusGameApp_RaiseGameResponse(object sender, MessageReceivedEventArgs e)
        {
            numberofResponses++;

            if ((numberofResponses == MaxPlayers) || (numberofResponses == numberOfPlayersInMesh-1))
            {
                CallOnDispatcher(delegate()
                {
                    if (GameRequestResponsesReceived != null)
                    {
                        GameRequestResponsesReceived(sender, e);
                    }
                });
            }
        }

        // PeerNode event handlers
        public static void OnOnline(object sender, EventArgs e)
        {
            Console.WriteLine("**  Online");
        }

        public static void OnOffline(object sender, EventArgs e)
        {
            Console.WriteLine("**  Offline");
        }

        #region public method for local player
        public void StartChat(string peerAddress, string portNumber)
        {
            // Construct InstanceContext to handle messages on callback interface. 
            // An instance of ChatApp is created and passed to the InstanceContext.

            InstanceContext instanceContext = new InstanceContext(this);

            endPoint = "net.p2p://" + _meshName + "/servicemodelsamples/Peer";

            NetPeerTcpBinding binding = new NetPeerTcpBinding();
            binding.Security.Mode = SecurityMode.None;
            if (string.IsNullOrEmpty(peerAddress))
                peerAddress = ConfigurationManager.AppSettings["peerResolverAddress"];
            if (string.IsNullOrEmpty(peerAddress))
                throw new Exception("peerAddress is mandatory, you can specify it in your config file at <appSettings><add key=\"peerResolverAddress\" value=\"http://localhost/xxxxxx\" /></appSettings>");
            binding.Resolver.Custom.Resolver = new CustomPeerResolver(peerAddress);
            binding.Resolver.Mode = PeerResolverMode.Custom;
            binding.Resolver.Custom.Address = new EndpointAddress(peerAddress);
            binding.Resolver.Custom.Binding = new NetTcpBinding();

            binding.OpenTimeout = TimeSpan.FromMinutes(3);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(3);
            binding.SendTimeout = TimeSpan.FromMinutes(3);
            if (string.IsNullOrEmpty(portNumber))
                portNumber = ConfigurationManager.AppSettings["portNumber"];
            if (string.IsNullOrEmpty(portNumber))
                throw new Exception("portNumber is mandatory, you can specify it in your config file at <appSettings><add key=\"portNumber\" value=\"8081\" /></appSettings>");
            binding.Port = int.Parse(portNumber);

            // Create the participant with the given endpoint configuration
            // Each participant opens a duplex channel to the mesh
            // participant is an instance of the chat application that has opened a channel to the mesh
            DuplexChannelFactory<Participant> factory = new DuplexChannelFactory<Participant>(instanceContext, binding, new EndpointAddress(endPoint));
            _Participant = factory.CreateChannel();

            // Retrieve the PeerNode associated with the participant and register for online/offline events
            // PeerNode represents a node in the mesh. Mesh is the named collection of connected nodes.
            IOnlineStatus ostat = _Participant.GetProperty<IOnlineStatus>();
            ostat.Online += new EventHandler(OnOnline);
            ostat.Offline += new EventHandler(OnOffline);

            _Participant.Open();
            // Announce self to other participants
            _Participant.Join(_member, _DisplayMember);
        }

        /// <summary>
        /// demarrage du jeu pour le server en asynchrone
        /// </summary>
        public void StartGameFromHost()
        {
            ThreadPool.QueueUserWorkItem((WaitCallback)delegate(object notUser)
            {
                //to add me in list of all others players
                AcceptGameRequest();
                _Participant.StartGame(this._member);
            });
        }

        /// <summary>
        /// Send a message on the mesh
        /// </summary>
        /// <param name="message"></param>
        public void ChannelChat(string message)
        {
            _Participant.Chat(_member, message);
        }

        /// <summary>
        /// leave the mesh
        /// </summary>
        public void StopChat()
        {
            if (_Participant != null)
            {
                if (_Participant.State == CommunicationState.Opened)
                {
                    _Participant.Leave(this._member);

                    _Participant.Close();
                }
                _Participant = default(Participant);
            }
        }

        /// <summary>
        /// Send a "accept"
        /// </summary>
        public void AcceptGameRequest()
        {
            _Participant.GameRequestAccepted(this._member);
        }

        /// <summary>
        /// Send a "reject"
        /// </summary>
        public void RejectGameRequest()
        {
            _Participant.GameRequestRejected(this._member);
        }

        /// <summary>
        /// Send a request to mesh, all players in the mesh can respond to this resquest by "Yes" or "No"
        /// </summary>
        /// <param name="message"></param>
        public void SendGameRequest(string message)
        {
            numberofResponses = 0;
            _Participant.SendGameRequest(Member, message);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (_Participant != null)
                StopChat();
        }

        #endregion
    }
}
