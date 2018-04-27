using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using GeniusTetris.Multiplayer.SignalR.Model;

namespace GeniusTetris.Server.SignalR
{
    public class GeniusTetrisGame
    {
        private readonly static Lazy<GeniusTetrisGame> _instance = new Lazy<GeniusTetrisGame>(() => new GeniusTetrisGame());

        ConcurrentDictionary<string, List<PeerPlayer>> _players = new ConcurrentDictionary<string, List<PeerPlayer>>();
        ConcurrentDictionary<string, string> _groupsByCnx = new ConcurrentDictionary<string, string>();
        ConcurrentDictionary<string, GameRoom> _rooms = new ConcurrentDictionary<string, GameRoom>();


        public event Action<string> OnTimeToStartGame;

        public GeniusTetrisGame()
        {
            Trace.WriteLine("GeniusTetrisGame:ctor()");
        }

        public static GeniusTetrisGame Instance
        {
            get
            {
                return _instance.Value;
            }
        }

        internal void PlayerJoin(PeerPlayer player, string groupName)
        {
            List<PeerPlayer> l;
            if (!_players.ContainsKey(groupName))
                _players.TryAdd(groupName, new List<PeerPlayer>());
            if (_players.TryGetValue(groupName, out l))
            {
                lock (((ICollection)l).SyncRoot)
                {
                    l.Add(player);
                    _groupsByCnx.TryAdd(player.ConnectionID, groupName);
                }
            }
        }

        internal IEnumerable<PeerPlayer> GetPlayers(string groupName)
        {
            List<PeerPlayer> l;
            if (_players.TryGetValue(groupName, out l))
            {
                return l.ToArray();
            }
            return new PeerPlayer[0];
        }

        internal string GetGroupNameFromConnectionID(string connectionID)
        {
            string groupName = null;
            _groupsByCnx.TryGetValue(connectionID, out groupName);
            return groupName;
        }

        internal PeerPlayer GetPlayer(string connectionID)
        {
            string groupName = null;
            if (_groupsByCnx.TryGetValue(connectionID, out groupName))
            {
                List<PeerPlayer> l;
                if (_players.TryGetValue(groupName, out l))
                {
                    if (l != null)
                    {
                        return l.Where(x => x.ConnectionID == connectionID).FirstOrDefault();
                    }
                }
            }
            return null;
        }

        internal void PlayerLeave(string connectionID)
        {
            string groupName;
            if (_groupsByCnx.TryRemove(connectionID, out groupName))
            {
                List<PeerPlayer> l;
                if (_players.TryGetValue(groupName, out l))
                {
                    if (l != null)
                    {
                        var p = l.Where(x => x.ConnectionID == connectionID).FirstOrDefault();
                        l.Remove(p);
                    }
                }
            }
            //remove from all room where player is
            foreach (var room in _rooms.Values)
            {
                var players = room.Players.Where(x => x.ConnectionID == connectionID).ToArray();
                if (players != null)
                {
                    foreach (var player in players)
	                {
                        room.Players.Remove(player);
	                }
                }
            }
        }

        private List<PeerPlayer> GetPlayersFromConnexionID(string connectionID)
        {
            string groupName;
            if (_groupsByCnx.TryGetValue(connectionID, out groupName))
            {
                List<PeerPlayer> l;
                if (_players.TryGetValue(groupName, out l))
                {
                    return l;
                }
            }
            return null;
        }

        internal PeerPlayer GetPlayerFromAcceptor(string acceptorUri)
        {
            return _players.Values.SelectMany(x => x).Where(x => x.AcceptorUri == acceptorUri).FirstOrDefault();
        }

        internal PeerPlayer GetPlayer(Guid playerID)
        {
            return _players.Values.SelectMany(x => x).Where(x => x.ID == playerID).FirstOrDefault();
        }

        internal void NewRoom(string gamingRoom, PeerPlayer owner)
        {
            var hypotheticalPlayers = GetPlayersFromConnexionID(owner.ConnectionID);
            var room = new GameRoom { Owner = owner, RoomName = gamingRoom, PlayersInGroup = hypotheticalPlayers };
            room.OnTimeToStartGame += room_OnTimeToStartGame;
            room.Players.Add(owner);
            _rooms.TryAdd(gamingRoom, room);
        }

        void room_OnTimeToStartGame(object sender, EventArgs e)
        {
            GameRoom room = ((GameRoom)sender);
            room.OnTimeToStartGame -= room_OnTimeToStartGame;
            if (OnTimeToStartGame != null)
                OnTimeToStartGame(room.RoomName);
        }

        internal bool PlayerWouldLikeToPlay(PeerPlayer player, string acceptorUri, out string gamingRoom)
        {
            gamingRoom = string.Empty;
            var room = _rooms.Values.Where(x => x.Owner.AcceptorUri == acceptorUri && !x.Started).FirstOrDefault();
            if (room != null)
            {
                if (room.TryAdd(player))
                {
                    gamingRoom = room.RoomName;
                    return true;
                }
            }
            return false;
        }

        internal bool IsAllPlayersReady(string gamingRoom)
        {
            GameRoom room;
            if (_rooms.TryGetValue(gamingRoom, out room))
            {
                return room.IsAllPlayersReady;
            }
            return false;
        }

        internal int PlayerCount(string gamingRoom)
        {
            GameRoom room;
            if (_rooms.TryGetValue(gamingRoom, out room))
            {
                return room.Players.Count;
            }
            return 0;
        }

        internal void AddPlayerReady(string gamingRoom)
        {
            GameRoom room;
            if (_rooms.TryGetValue(gamingRoom, out room))
            {
                lock (room)
                {
                    room.ReadyPlayerCount++;
                }
            }
        }

        internal bool PlayerWontPlay(PeerPlayer player, string acceptorUri, out string gamingRoom)
        {
            gamingRoom = null;
            var room = _rooms.Values.Where(x => x.Owner.AcceptorUri == acceptorUri).FirstOrDefault();
            if (room != null)
            {
                lock (((ICollection)room.Players).SyncRoot)
                {
                    room.RefusedCount++;
                    gamingRoom = room.RoomName;
                    return true;
                }
            }
            return false;
        }

        internal void GameOverFor(string gamingRoom, PeerPlayer player)
        {
            GameRoom room;
            if (_rooms.TryGetValue(gamingRoom, out room))
            {
                lock (room)
                {
                    room.GameOverCount++;
                }
            }
        }

        internal bool TryStartGame(string gamingRoom)
        {
            GameRoom room;
            if (_rooms.TryGetValue(gamingRoom, out room))
            {
                lock (room)
                {
                    if (!room.Started)
                    {
                        room.Started = true;
                        return true;
                    }
                }
            }
            return false;
        }

        internal void StopGame(PeerPlayer player, string gamingRoom)
        {
            GameRoom room;
            if (_rooms.TryGetValue(gamingRoom, out room))
            {
                if (room.Owner.ID == player.ID)
                {
                    lock (room)
                    {
                        room.Started = true;
                    }
                }
                _rooms.TryRemove(gamingRoom, out room);
            }
        }
    }
}