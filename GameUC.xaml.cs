using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GeniusTetris.Core;
using System.Collections.ObjectModel;
using GeniusTetris.Services;
using GeniusTetris.Dialogs;
using System.Configuration;
using System.ComponentModel;
using System.Windows.Threading;
using GeniusTetris.Multiplayer;
using System.Linq;

namespace GeniusTetris
{
    /// <summary>
    /// Interaction logic for GameUC.xaml
    /// </summary>

    public partial class GameUC : System.Windows.Controls.UserControl
    {
        Dictionary<Guid, GeniusTetris.BoardUC> _PlayersBoard;
        Dictionary<string, GeniusTetrisPlayer> _PlayersKey;

        ITetrisMultiplayerApplication app;

        public static RoutedCommand GameAction = new RoutedCommand();

        public GameUC()
        {
            app = (ITetrisMultiplayerApplication)Activator.CreateInstance(GetConfigurationType(), new GeniusTetris.Utils.WpfTimer());
            //_Server = new GeniusP2PManager.Server();
            //_Options = new ObservableCollection<byte>();
            //_DataOptions = new DataOptions();
            //_Game = new Game(new GeniusTetris.Utils.WpfTimer());
            app.CurrentGame.OnGameOver += new EventHandler(_Game_OnGameOver);
            app.OnOptionArrived += new Action<GeniusTetris.Multiplayer.GeniusTetrisPlayer, GeniusTetris.Multiplayer.GeniusTetrisPlayer, byte>(app_OnOptionArrived);
            app.OnGameRequestAccepted += new Action<GeniusTetris.Multiplayer.GeniusTetrisPlayer>(app_OnGameRequestAccepted);
            app.OnGameRequestReceived += new Action<GeniusTetrisPlayer, string>(app_OnGameRequestReceived);
            app.OnHideBoard += new Action<GeniusTetris.Multiplayer.GeniusTetrisPlayer, bool>(app_OnHideBoard);
            app.OnEndGameEnd += new Action<GeniusTetrisPlayer, string>(app_OnEndGameEnd);
            app.OnGameOver += new Action<GeniusTetrisPlayer, string>(app_OnGameOver);
            app.OnSendBoard += new Action<GeniusTetrisPlayer, byte[,]>(app_OnSendBoard);
            app.OnStartGameNow += new Action<GeniusTetrisPlayer>(app_OnStartGameNow);
            //_Game.OnOptionsChanged += new EventHandler(_Game_OnOptionsChanged);
            //_Game.Board.OnDropped += new EventHandler<BoardChangedEventArgs>(Board_OnDropped);
            //_Game.OnBoardChanged += new EventHandler(_Game_OnBoardChanged);
            //_Game.OnHideMyBoard += new EventHandler(_Game_OnHideMyBoard);
            _PlayersBoard = new Dictionary<Guid, GeniusTetris.BoardUC>();
            _PlayersKey = new Dictionary<string, GeniusTetrisPlayer>();
            InitializeComponent();
            this.board.Board = app.CurrentGame.Board;
            //this.board1.Board = this.board2.Board = this.board3.Board = this.board4.Board = this.board5.Board = this.board6.Board = _Game.Board;
            this.previewctl.ShapeQueue = app.CurrentGame.ShapeQueue;
            Application.Current.Exit += new ExitEventHandler(Current_Exit);
        }

        private Type GetConfigurationType()
        {
            Configuration cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string sType = cfg.AppSettings.Settings["ApplicationType"].Value;

            return Type.GetType(sType, true);
        }

        void Current_Exit(object sender, ExitEventArgs e)
        {
            app.ExitGame();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {

        }

        void _Game_OnGameOver(object sender, EventArgs e)
        {
            this.board.GameOver = Visibility.Visible;
        }

        private void OnCanGameAction(object sender, CanExecuteRoutedEventArgs e)
        {
            string parameter = (string)e.Parameter;
            switch (parameter)
            {
                case "StopGame":
                    e.CanExecute = app.CurrentGame.Started;
                    break;
                case "StartServerGame":
                    e.CanExecute = false;
                    break;
                case "StopServerGame":
                    e.CanExecute = false;
                    break;
                case "StartMultiplayerGame":
                    e.CanExecute = !app.CurrentGame.Started && app.IsConnected;
                    break;
                case "ConnectToMesh":
                    //e.CanExecute = !_Game.Started && !_Server.Started;
                    e.CanExecute = !app.CurrentGame.Started && !app.IsConnected;
                    break;
                case "DisconnectToMesh" :
                    //e.CanExecute = !_Game.Started && !_Server.Started && _app != null;
                    e.CanExecute = !app.CurrentGame.Started && app.IsConnected;
                    break;
                default:
                    e.CanExecute = true;
                    break;
            }
        }

        private void OnGameAction(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Parameter != null)
            {
                string key = e.Parameter.ToString();
                if (!app.CurrentGame.Started)
                {
                    switch (key)
                    {
                        case "OnNetworkOptions":
                            ModalService.ShowModal(new OptionsUC(app.GameOptions));
                            break;
                        case "OnHelp" :
                            ModalService.ShowModal(new HelpUC());
                            break;
                        case "OnAbout" :
                            e.Handled = true;
                            OnAbout(null, null);
                            break;
                        case "ConnectToMesh":
                            this.board.PlayerName = app.GameOptions.Member;
                            app.ConnectToMesh();
                            break;
                        case "DisconnectToMesh" :
                            this.board.PlayerName = app.GameOptions.Member;
                            app.DisconnectFromMesh();
                            break;
                        case "StartMultiplayerGame" :
                            WaitForOtherPlayers();
                            break;
                        case "StartAlone":
                            ClearAllPlayersBoard();
                            this.board.GameOver = Visibility.Collapsed;
                            app.StartStandalone();
                            this.board.Focus();
                            break;
                    }
                    return;
                }
                switch (key)
                {
                    case "Drop":
                        app.CurrentGame.Drop();
                        e.Handled = true;
                        break;
                    case "Left":
                        app.CurrentGame.MoveLeft();
                        e.Handled = true;
                        break;
                    case "Right":
                        app.CurrentGame.MoveRight();
                        e.Handled = true;
                        break;
                    case "Down":
                        app.CurrentGame.MoveDown();
                        e.Handled = true;
                        break;
                    case "Up":
                        app.CurrentGame.RotateRight();
                        e.Handled = true;
                        break;
                    case "1":
                    case "2":
                    case "3":
                    case "4":
                    case "5":
                    case "6":
                    case "7":
                    case "8":
                    case "9":
                        app.CurrentGame.SendNextOption(GetReceiver(key));
                        break;
                    case "StopGame" :
                        app.CurrentGame.Stop();
                        break;
#if DEBUG
                    case "a" :
                        Game.Options.Add((byte)'r');
                        ((TetrisMultiplayerApplicationBase)app)._Game_OnOptionsChanged(Game, EventArgs.Empty);
                        break;
#endif
                }
            }
        }

        #region Multi players section

        class ScorePlayer
        {
            public int Score { get; set; }
            public int TeamScore { get; set; }
            public string NickName { get; set; }
            public bool IsYou { get; set; }
            public string TeamName { get; set; }

            internal static ScorePlayer FromPlayer(GeniusTetrisPlayer x)
            {
                return new ScorePlayer
                {
                    Score = x.Score,
                    NickName = x.NickName,
                    TeamName = GameOptions.Instance.Value.TeamName
                };
            }
        }

        private void WaitForOtherPlayers()
        {
            MessageDlgUC dlg = new MessageDlgUC(null,
                delegate
                {
                    app.StopMultiPlayerGameWF();
                    Console.WriteLine("user click on close button");
                }
                , false, false);
            dlg.Message = "Wait for players...(15 seconds)";
            ModalService.ShowModal(dlg);
            _PlayersBoard.Clear();
            _PlayersKey.Clear();
            app.StartMultiplayerGameWF();
        }

        void app_OnEndGameEnd(GeniusTetrisPlayer arg1, string arg2)
        {
            //Sorted player by their score
            CollectionViewSource sortedCollection = new CollectionViewSource();
            ObservableCollection<ScorePlayer> allplayers = new ObservableCollection<ScorePlayer>(app.GameMembersList.Select(x => ScorePlayer.FromPlayer(x)));
            app.CurrentPlayer.Score = app.CurrentGame.Score;
            var Me = ScorePlayer.FromPlayer(app.CurrentPlayer); 
            Me.IsYou = true;
            allplayers.Add(Me);
            foreach (var teamGrp in allplayers.GroupBy(x => x.TeamName))
            {
                var teamScore = teamGrp.Sum(x => x.Score);
                foreach (var player in teamGrp)
                {
                    player.TeamScore = teamScore;
                }
            }
            sortedCollection.Source = allplayers;
            var grp = new PropertyGroupDescription("TeamName");
            sortedCollection.GroupDescriptions.Add(grp);
            sortedCollection.SortDescriptions.Add(new SortDescription("Score", ListSortDirection.Descending));

            ScoresDlgUC dlg = new ScoresDlgUC();
            dlg.DataContext = sortedCollection;
            ModalService.ShowModal(dlg);
        }


        void app_OnOptionArrived(GeniusTetrisPlayer from, GeniusTetrisPlayer to, byte option)
        {
            if (_PlayersBoard.ContainsKey(from.ID))
            {
                GeniusTetris.BoardUC b = _PlayersBoard[from.ID];
                app.CurrentGame.ExecuteOption(option, new ProxyGame(app, from.ID, b.Board));
            }
        }

        void app_OnHideBoard(GeniusTetrisPlayer player, bool hide)
        {
            if (_PlayersBoard.ContainsKey(player.ID))
            {
                GeniusTetris.BoardUC b = _PlayersBoard[player.ID];
                b.Invisibility = hide ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        void app_OnGameRequestAccepted(GeniusTetris.Multiplayer.GeniusTetrisPlayer player)
        {
            //Assign player to a board
            int ieme = _PlayersBoard.Count + 1;
            GeniusTetris.BoardUC b;
            //find ieme board, last me be 8, after exception will throw
            b = this.FindName(string.Format("board{0}", ieme)) as GeniusTetris.BoardUC;
            //change the player name at the top of board
            b.Player = player;
            //adds link between playerId and his local board to a dictionnary
            _PlayersBoard.Add(player.ID, b);
            //hide player
            b.GameOver = Visibility.Collapsed;
            b.Invisibility = Visibility.Collapsed;
            b.OffLine = Visibility.Collapsed;
            //TODO: hide all not used board
            //add a link between a key "2" -> "8" to a playerId
            _PlayersKey.Add((ieme + 1).ToString(), player);
        }

        void app_OnStartGameNow(GeniusTetrisPlayer obj)
        {
            ModalService.CloseModal();
            this.board.GameOver = Visibility.Collapsed;
            app.CurrentGame.Start();
            this.board.Focus();
        }

        void app_OnSendBoard(GeniusTetrisPlayer player, byte[,] data)
        {
            //throw new NotImplementedException();
            if (_PlayersBoard.ContainsKey(player.ID))
            {
                GeniusTetris.BoardUC b = _PlayersBoard[player.ID];
                if (b.Board == null)
                    b.Board = new Board(12, 22, null);
                b.Board.SetData(data);
            }
        }

        void app_OnGameOver(GeniusTetrisPlayer player, string arg2)
        {
            if (_PlayersBoard.ContainsKey(player.ID))
            {
                _PlayersBoard[player.ID].GameOver = Visibility.Visible;
            }
        }
        void app_OnGameRequestReceived(GeniusTetrisPlayer player, string message)
        {
            _PlayersBoard.Clear();
            _PlayersKey.Clear();
            ClearAllPlayersBoard();
            MessageDlgUC dlg = new MessageDlgUC(delegate
            {
                app.AcceptGameRequest();
            },
            delegate
            {
                app.RejectGameRequest();
            },
            true,
            true);
            dlg.Message = message;
            ModalService.ShowModal(dlg);
        }

        private void ClearAllPlayersBoard()
        {
            if (this.board.Board != null)
                this.board.Board.SetData(new byte[12, 22]);
            foreach (GeniusTetris.BoardUC b in Boards)
            {
                if (b.Board != null)
                    b.Board.SetData(new byte[12,22]);
                b.OffLine = Visibility.Visible;
                b.GameOver = Visibility.Collapsed;
                b.Invisibility = Visibility.Collapsed;
            }
        }

        private IEnumerable<GeniusTetris.BoardUC> Boards
        {
            get
            {
                for (int i = 1; i < 9; i++)
                {
                    GeniusTetris.BoardUC b = this.FindName(string.Format("board{0}", i)) as GeniusTetris.BoardUC;
                    if (b != null)
                        yield return b;
                }
                yield break;
            }
        }

        /// <summary>
        /// Retreie a receiver for option, only "1" is authorized when a player plays alone
        /// </summary>
        /// <param name="key"></param>
        /// <returns>a local playe on "1" key, and a proxy player on others</returns>
        private IGame GetReceiver(string key)
        {
            if (key == "1")
                return app.CurrentGame;
            if (!app.InMultiplayer)
                return null;
            GeniusTetrisPlayer player = FindPlayerKey(key);
            if (player != null && !player.IsGameOver)
            {
                GeniusTetris.BoardUC b = _PlayersBoard[player.ID];
                return new ProxyGame(this.app, player.ID, b.Board);
            }
            return null;
        }

        /// <summary>
        /// Get Player from a key
        /// </summary>
        /// <param name="key">key must be in "2" -> "8"</param>
        /// <returns></returns>
        private GeniusTetrisPlayer FindPlayerKey(string key)
        {
            return _PlayersKey[key];
        }
        
        #endregion

        void OnAbout(object sender, RoutedEventArgs e)
        {
            AboutUC uc = new AboutUC();
            ModalService.ShowModal(uc);
        }

        public Game Game
        {
            get
            {
                return app.CurrentGame;
            }
        }

        public ObservableCollection<byte> Options
        {
            get
            {
                return app.Options;
            }
        }

        public object CurrentApplication
        {
            get
            {
                return app;
            }
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            this.board.Focus();
        }
    }
}