using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;

namespace GeniusTetris.Core
{
    /// <summary>
    /// for available options see <see cref="Options"/>
    /// </summary>
    public class Game : INotifyPropertyChanged, IGame
    {
        Board _Board;
        private ShapeQueue _queue = new ShapeQueue();
        private List<byte> _Options;
        private bool _started = false;
        private bool _paused = false;
        private int _timerInterval;
        private int _score;
        private int _level;
        private int _linesCompleted;
        private bool _ended;

        public event EventHandler OnGameOver;
        public event EventHandler OnOptionsChanged;
        public event EventHandler OnBoardChanged;
        public event EventHandler OnHideMyBoard;

        public Game(ITimer aTimer)
        {
            _Options = new List<byte>();
            _Board = new Board(12, 22, aTimer);
            _Board.OnLinesCompleted += new EventHandler<LinesCompletedEventArgs>(_Board_OnLinesCompleted);
            _Board.OnDropped += new EventHandler<BoardChangedEventArgs>(_Board_OnDropped);
            _Board.OnBoardFull += new EventHandler(_Board_OnBoardFull);
            _Board.OnOptions += new EventHandler<BoardOptionsCapturedEventArgs>(_Board_OnOptions);
        }

        void _Board_OnOptions(object sender, BoardOptionsCapturedEventArgs e)
        {
            int restant = 18 - _Options.Count;
            bool changed = false;
            if (restant > 0)
            {
                foreach (byte b in e.Options)
                {
                    if (restant > 0)
                    {
                        _Options.Add(b);
                        changed = true;
                        restant--;
                    }
                    else
                        break;
                }
            }
            if (changed)
                DoOptionsChanged();
        }

        void _Board_OnBoardFull(object sender, EventArgs e)
        {
            _started = false;
            _ended = true;
            if (OnGameOver != null)
                OnGameOver(this, EventArgs.Empty);
        }

        void _Board_OnDropped(object sender, BoardChangedEventArgs e)
        {
            ThrowNewShape();
        }

        void _Board_OnLinesCompleted(object sender, LinesCompletedEventArgs e)
        {
            LinesCompleted += e.CompletedLines;
            Score += (int)Math.Round(100 * e.CompletedLines * Math.Pow(
                Math.Pow(2, 1 / 3d), (e.CompletedLines - 1)));
            Level = LinesCompleted / 10 + 1;
            _timerInterval = Math.Max(55, 800 - (_level - 1) * 55);
            DoChanged("Score");
            DoChanged("LinesCompleted");
        }

        private void ThrowNewShape()
        {
            _Board.ThrowShape(_queue.PickUpShape(), _timerInterval);
        }

        private void DoChanged(string apropname)
        {
            //Debug.WriteLine("Game.DoChange");
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(apropname));
        }

        private void SetUpBoard()
        {
            _Options.Clear();
            _Board.Reset();
            DoOptionsChanged();
            LinesCompleted = 0;
            Score = 0;
            Level = 1;
            _timerInterval = 800;
        }

        /// <summary>
        /// Start game
        /// </summary>
        public void Start()
        {
            if (_started)
                return;
            _started = true;
            _ended = false;

            SetUpBoard();
            ThrowNewShape();
        }

        public void Stop()
        {
            if (_started)
            {
                _started = false;
                _ended = true;
                _Board.Stop();                
            }
        }

        public void MoveLeft()
        {
            _Board.MoveLeft();
        }

        public void MoveRight()
        {
            _Board.MoveRight();
        }

        public void RotateLeft()
        {
            _Board.RotateLeft();
        }

        public void RotateRight()
        {
            _Board.RotateRight();
        }

        public void Drop()
        {
            _Board.Drop();
        }

        public void MoveDown()
        {
            _Board.MoveDown();
        }

        public Board Board
        {
            get
            {
                return _Board;
            }
        }

        #region propriétés publiques
        public int Level
        {
            get
            {
                return _level;
            }
            private set
            {
                _level = value;
                DoChanged("Level");
            }
        }

        public int Score
        {
            get
            {
                return _score;
            }
            private set
            {
                _score = value;
                DoChanged("Score");
            }
        }

        public int LinesCompleted
        {
            get
            {
                return _linesCompleted;
            }
            private set
            {
                _linesCompleted = value;
                DoChanged("LinesCompleted");
            }
        }

        public ShapeQueue ShapeQueue
        {
            get
            {
                return _queue;
            }
        }

        public List<byte> Options
        {
            get
            {
                return _Options;
            }
        }

        public bool Started
        {
            get
            {
                return _started;
            }
        }
        #endregion



        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void SendNextOption(IGame receiver)
        {
            if (_Options.Count > 0)
            {
                if (receiver == null)
                    return;
                byte option;
                lock(_Options)
                {
                    option = _Options[0];
                    _Options.RemoveAt(0);
                }
                receiver.ExecuteOption(option, this);
                //particular case for 's' option
                if (option == 's')
                {
                    this.ExecuteOption(option, receiver);
                }
                DoOptionsChanged();
            }
        }

        private void DoOptionsChanged()
        {
            if (OnOptionsChanged != null)
                OnOptionsChanged(this, EventArgs.Empty);
        }

        #region IGame Members

        public void ExecuteOption(byte option, IGame sender)
        {
            char c = (char)option;
            switch (c)
            {
                /// les options du jeu :
                /// - a : add line
                /// - b : clear all option in board, clear special block
                /// - c : clear line
                /// - e : erase board
                /// - g : gravity
                /// - h : Hide my board to anothers during 5 secondes
                /// - l : gravity left
                /// - n : nuke field
                /// - o : block bomb 
                /// - q : quake
                /// - r : gravity right
                /// - s : switch board with another
                case 'a' :// - a : add line
                    this.Board.AddLine();
                    DoBoardChanged();
                    break;
                case 'b' :// - b : clear all option in board, clear special block
                    this.Board.ClearOptionsOnBoard();
                    DoBoardChanged();
                    break;
                case 'c' :// - c : clear line
                    this.Board.RemoveLine();
                    DoBoardChanged();
                    break;
                case 'e' :// - e : erase board
                case 'n' :
                    this.Board.EraseBoard();
                    DoBoardChanged();
                    break;
                case 'g':// - g : gravity
                    this.Board.Gravity();
                    DoBoardChanged();
                    break;
                case 'h':// - h : hide my board to anothers during 5 secondes
                    DoHideMyBoard();
                    break;
                case 'l':// - l : gravity left
                    this.Board.GravityLeft();
                    DoBoardChanged();
                    break;
                case 'o': // - o : block bomb 
                    this.Board.BlockBomb();
                    DoBoardChanged();
                    break;
                case 'q' : // - q : quake
                    this.Board.Quake();
                    DoBoardChanged();
                    break;
                case 'r': // - r : gravity right
                    this.Board.GravityRight();
                    DoBoardChanged();
                    break;
                case 's': //- s : switch board with another
                    if (sender != this)
                    {
                        byte[,] blocks = sender.GetBoardData();
                        this.Board.ChangedBoard(blocks);
                        DoBoardChanged();
                    }
                    break;
            }
        }

        private void DoHideMyBoard()
        {
            if (OnHideMyBoard != null)
                OnHideMyBoard(this, EventArgs.Empty);
        }

        private void DoBoardChanged()
        {
            if (OnBoardChanged != null)
                OnBoardChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Retreive data of board without block in move
        /// </summary>
        /// <returns></returns>
        public byte[,] GetBoardData()
        {
            return this.Board.GetBlocksWithoutMover();
        }
        #endregion
    }
}
