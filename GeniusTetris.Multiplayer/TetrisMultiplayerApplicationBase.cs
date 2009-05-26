using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeniusTetris.Core;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.ComponentModel;

namespace GeniusTetris.Multiplayer
{
    public class TetrisMultiplayerApplicationBase : ITetrisMultiplayerApplication, INotifyPropertyChanged
    {
        protected Dispatcher Dispatcher { get; private set; }

        public TetrisMultiplayerApplicationBase(ITimer timer)
        {
            this.Dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            CurrentGame = new GeniusTetris.Core.Game(timer);
            CurrentGame.OnGameOver += new EventHandler(_Game_OnGameOver);
            CurrentGame.OnOptionsChanged += new EventHandler(_Game_OnOptionsChanged);
            CurrentGame.Board.OnDropped += new EventHandler<BoardChangedEventArgs>(Board_OnDropped);
            CurrentGame.OnBoardChanged += new EventHandler(_Game_OnBoardChanged);
            CurrentGame.OnHideMyBoard += new EventHandler(_Game_OnHideMyBoard);
            PlayersInMeshList = new System.Collections.ObjectModel.ObservableCollection<GeniusTetrisPlayer>();
            GameMembersList = new System.Collections.ObjectModel.ObservableCollection<GeniusTetrisPlayer>();
            Options = new ObservableCollection<byte>();            
            GameOptions = new GameOptions();
            GameOptions.OnOptionsChanged += new EventHandler(GameOptions_OnOptionsChanged);
            CurrentPlayer = new GeniusTetrisPlayer()
            {
                ID = Guid.NewGuid(),
                NickName = GameOptions.Member
            };
        }

        void GameOptions_OnOptionsChanged(object sender, EventArgs e)
        {
            DoGameOptionsChanged();
        }

        protected virtual void DoGameOptionsChanged()
        {
            CurrentPlayer.NickName = GameOptions.Member;
        }

        #region game events subscription
        void _Game_OnGameOver(object sender, EventArgs e)
        {
            if (InMultiplayer)
                SendGameOver();
        }

#if DEBUG
        public
#endif
        void _Game_OnOptionsChanged(object sender, EventArgs e)
        {
            if (System.Windows.Threading.Dispatcher.CurrentDispatcher != this.Dispatcher)
            {
                this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, TimeSpan.MaxValue, new EventHandler(_Game_OnOptionsChanged), sender, e);
            }
            else
            {
                //throw new Exception("The method or operation is not implemented.");
                if (CurrentGame.Options.Count > Options.Count)
                {
                    int nb = CurrentGame.Options.Count - Options.Count;
                    for (int i = CurrentGame.Options.Count - nb; i < CurrentGame.Options.Count; i++)
                    {
                        Options.Add(CurrentGame.Options[i]);
                    }
                }
                else
                {
                    int nb = Options.Count - CurrentGame.Options.Count;
                    for (int i = 0; i < nb; i++)
                    {
                        Options.RemoveAt(0);
                    }
                }
            }
        }
        void Board_OnDropped(object sender, BoardChangedEventArgs e)
        {
            if (InMultiplayer)
            {
                this.SendBoard(CurrentGame.GetBoardData());
            }
        }

        void _Game_OnBoardChanged(object sender, EventArgs e)
        {
            if (InMultiplayer)
            {
                this.SendBoard(CurrentGame.GetBoardData());
            }
        }

        void _Game_OnHideMyBoard(object sender, EventArgs e)
        {
            if (InMultiplayer)
            {
                this.HideBoard();
                System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();
                timer.Interval = TimeSpan.FromMilliseconds(10000);
                timer.Tick += delegate
                {
                    this.ShowBoard();
                    timer.Stop();
                };
                timer.Start();
            }
        }
        #endregion

        #region ITetrisMultiplayerApplication Members

        public GeniusTetrisPlayer CurrentPlayer
        {
            get;
            set;
        }

        public GeniusTetris.Core.Game CurrentGame
        {
            get;
            private set;
        }

        public virtual void ExitGame()
        {
            CurrentGame.Stop();
            DisconnectFromMesh();
        }

        public bool InMultiplayer
        {
            get;
            protected set;
        }


        private int _WorkingCount;
        public int WorkingCount
        {
            get { return _WorkingCount; }
            set
            {
                if (value != _WorkingCount)
                {
                    _WorkingCount = value;
                    NotifyPropertyChanged("WorkingCount");
                }
            }
        }

        private string _WorkingMessage;
        public string WorkingMessage
        {
            get { return _WorkingMessage; }
            set
            {
                if (value != _WorkingMessage)
                {
                    _WorkingMessage = value;
                    NotifyPropertyChanged("WorkingMessage");
                }
            }
        }

        public void StartStandalone()
        {
            InMultiplayer = false;
            CurrentGame.Start();
        }

        public virtual void HideBoard()
        {
            throw new NotImplementedException();
        }

        public virtual void ShowBoard()
        {
            throw new NotImplementedException();
        }

        public virtual void SendBoard(byte[,] board)
        {
            throw new NotImplementedException();
        }

        public virtual void SendScore(int score)
        {
            throw new NotImplementedException();
        }

        public virtual void SendGameOver()
        {
            throw new NotImplementedException();
        }

        public virtual void SendOption(GeniusTetrisPlayer toplayer, byte option)
        {
            throw new NotImplementedException();
        }

        public virtual void AcceptGameRequest()
        {
            throw new NotImplementedException();
        }

        public virtual void RejectGameRequest()
        {
            throw new NotImplementedException();
        }

        public virtual void StartMultiplayerGameWF()
        {
            throw new NotImplementedException();
        }

        public virtual void StopMultiPlayerGameWF()
        {
            throw new NotImplementedException();
        }

        public virtual void ConnectToMesh()
        {
            throw new NotImplementedException();
        }

        public virtual void DisconnectFromMesh()
        {
            throw new NotImplementedException();
        }


        #region app events 
        public event Action<GeniusTetrisPlayer, byte[,]> OnSendBoard;
        protected void DoSendBoard(GeniusTetrisPlayer player, byte[,] board)
        {
            if (OnSendBoard != null)
                OnSendBoard(player, board);
        }
        public event Action<GeniusTetrisPlayer, double> OnTimerChanged;
        protected void DoTimerChanged(GeniusTetrisPlayer player, double value)
        {
            if (OnTimerChanged != null)
                OnTimerChanged(player, value);
        }

        public event Action<GeniusTetrisPlayer, string> OnGameOver;
        protected void DoGameOver(GeniusTetrisPlayer player, string message)
        {
            if (OnGameOver != null)
                OnGameOver(player, message);
        }
        public event Action<GeniusTetrisPlayer, GeniusTetrisPlayer, byte> OnOptionArrived;
        protected void DoOptionArrived(GeniusTetrisPlayer from, GeniusTetrisPlayer to, byte option)
        {
            if (OnOptionArrived != null)
                OnOptionArrived(from, to, option);
        }

        public event Action<GeniusTetrisPlayer, int> OnSendScore;
        protected void DoSendScore(GeniusTetrisPlayer player, int score)
        {
            if (OnSendScore != null)
                OnSendScore(player, score);
        }

        public event Action<GeniusTetrisPlayer, string> OnEndGameEnd;
        protected void DoEndGameEnd(GeniusTetrisPlayer player, string message)
        {
            if (OnEndGameEnd != null)
                OnEndGameEnd(player, message);
        }

        public event Action<GeniusTetrisPlayer, bool> OnHideBoard;
        protected void DoHideBoard(GeniusTetrisPlayer player, bool hide)
        {
            if (OnHideBoard != null)
                OnHideBoard(player, hide);
        }

        public event Action<GeniusTetrisPlayer> OnStartGameNow;
        protected void DoStartGameNow(GeniusTetrisPlayer player)
        {
            if (OnStartGameNow != null)
                OnStartGameNow(player);
        }

        public event Action<GeniusTetrisPlayer> OnGameRequestAccepted;
        protected void DoGameRequestAccepted(GeniusTetrisPlayer player)
        {
            if (OnGameRequestAccepted != null)
                OnGameRequestAccepted(player);
        }
        public event Action<GeniusTetrisPlayer, string> OnGameRequestReceived;
        protected void DoGameRequestReceived(GeniusTetrisPlayer player, string message)
        {
            if (OnGameRequestReceived != null)
                OnGameRequestReceived(player, message);
        }

        #endregion

        public System.Collections.ObjectModel.ObservableCollection<GeniusTetrisPlayer> PlayersInMeshList
        {
            get;
            private set;
        }

        public System.Collections.ObjectModel.ObservableCollection<GeniusTetrisPlayer> GameMembersList
        {
            get;
            private set;
        }

        public ObservableCollection<byte> Options { get; private set; }

        public GameOptions GameOptions { get; private set; }

        public bool IsConnected
        {
            get;
            protected set;
        }

        #endregion

        #region INotifyPropertyChanged Members
        protected void NotifyPropertyChanged(string propname)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propname));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        protected void CallOnDispatcher(Action a)
        {
            if (System.Windows.Threading.Dispatcher.CurrentDispatcher != this.Dispatcher)
            {
                this.Dispatcher.BeginInvoke(a);
            }
            else
                a();
        }

        protected void CallAsync<TArg, TResult>(Action before, Func<TArg, TResult> action, Action<TResult, Exception> executed, TArg arg)
        {
            if (before != null)
            {
                CallOnDispatcher(() =>
                    {
                        before();
                    });
            }
            System.Threading.ThreadPool.QueueUserWorkItem(state =>
                {
                    Exception ResultException = null;
                    TResult result = default(TResult);
                    try
                    {
                        result = action((TArg)state);
                    }
                    catch (Exception ex)
                    {
                        ResultException = ex;
                    }
                    if (executed != null)
                    {
                        CallOnDispatcher(() =>
                            {
                                executed(result, ResultException);
                            });
                    }
                }, arg);
        }
    }
}
