using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Timers;
using System.Diagnostics;
using System.Threading;

namespace GeniusTetris.Core
{
    /// <summary>
    /// gestion du deplacement de la piece
    /// </summary>
    class ShapeMover : IDisposable
    {
        private Shape _Current;
        private Board _Board;
        //private System.Timers.Timer _timer = new System.Timers.Timer();
        private ITimer _timer;
        private int _positionX, _positionY;
        private bool _fallen = false;

        internal event EventHandler<BoardChangedEventArgs> OnDropped;
        internal event EventHandler<BoardChangedEventArgs> OnMoved;
        internal event EventHandler OnCollisionAtStart;

        public ShapeMover(Board board, Shape tomove, ITimer aTimer)
        {
            _Board = board;
            _Current = tomove;
            _timer = aTimer;
            if (_timer != null)
                _timer.OnElapsed +=new EventHandler(_timer_Elapsed);
        }

        void _timer_Elapsed(object sender, EventArgs e)
        {
            MoveShape(Movement.Down);
        }

        public Shape Shape
        {
            get { return _Current; }
        }

        public int PositionX
        {
            get { return _positionX; }
        }

        public int PositionY
        {
            get { return _positionY; }
        }

        public int GetGhostPositionY()
        {
            int ghostPositionY = _positionY;

            while (IsCollidingWithBoard(_positionX,
                ghostPositionY - 1) == false)
            {
                --ghostPositionY;
            }

            return ghostPositionY;
        }

        public int GetGhostDifferenceInY()
        {
            return (_positionY - GetGhostPositionY());
        }

        internal void ThrowIntoBoard(int fallByStepInterval)
        {
            // calculate initial position
            _positionX = (byte)Math.Ceiling((double)(_Board.Width - this.Shape.Width) / 2);
            _positionY = (byte)(_Board.Height - this.Shape.Height);
            if (this.Shape.Height == 1)
                _positionY--;	// "line" shape always starts at the 2nd line

            if (IsCollidingWithBoard(_positionX, _positionY))
            {
                _timer.Stop();
                OnCollisionAtStart(this,EventArgs.Empty);
                return;
            }

            // display initial position
            OnMoved(this, new BoardChangedEventArgs(GetChangedBlock(_positionX, _positionY)));
            _timer.Interval = fallByStepInterval;
            _timer.Start();
        }

        private Dictionary<Point, bool> GetChangedBlock(int offsetx, int offsety)
        {
            Dictionary<Point, bool> Result = new Dictionary<Point, bool>();
            for (int i = 0; i < this.Shape.Width; i++)
            {
                for (int j = 0; j < this.Shape.Height; j++)
                {
                    if (this._Board.ValueOf(offsetx + i, offsety + j) != this.Shape[i, j])
                        Result.Add(new Point(offsetx + i, offsety + j), true);
                }
            }
            return Result;
        }

        private bool IsCollidingWithBoard(int shapePosX, int shapePosY)
        {
            // check indexes
            if (shapePosX < 0 || shapePosY < 0 ||
                shapePosX + this.Shape.Width > _Board.Width ||
                shapePosY + this.Shape.Height > _Board.Height)
                return true;

            // check overlapping
            for (byte i = 0; i < this.Shape.Width; i++)
            {
                for (byte j = 0; j < this.Shape.Height; j++)
                {
                    if (this.Shape[i, j] != 0 &&
                        _Board.ValueOf((byte)(i + shapePosX), (byte)(j + shapePosY)) != 0)
                        return true;
                }
            }
            return false;
        }
        internal void Pause()
        {
            _timer.Stop();
        }

        internal void Continue()
        {
            _timer.Start();
        }

        internal void MoveLeft()
        {
            MoveShape(Movement.Left);
        }

        internal void MoveRight()
        {
            MoveShape(Movement.Right);
        }

        internal void MoveDown()
        {
            _timer.Stop();
            MoveShape(Movement.Down);
            _timer.Start();
        }

        internal void RotateLeft()
        {
            Rotate(false);
        }

        internal void RotateRight()
        {
            Rotate(true);
        }

        internal void Drop()
        {
            MoveShape(Movement.Drop);
        }

        internal void Stop()
        {
            _timer.Stop();
            _fallen = true;
        }

        private void MoveShape(Movement movement)
        {
            if (_fallen)
                return;
            lock (this._Board.lockOperationPending)
            {
                Debug.WriteLine(string.Format("{0} : MoveShape", Thread.CurrentThread.ManagedThreadId));
                int horizontalMove = 0;
                int verticalMove = 0;
                switch (movement)
                {
                    case Movement.Down:
                        verticalMove = -1;
                        break;
                    case Movement.Left:
                        horizontalMove = -1;
                        break;
                    case Movement.Right:
                        horizontalMove = 1;
                        break;
                    case Movement.Drop:
                        verticalMove = -GetGhostDifferenceInY();
                        break;
                }

                if (IsCollidingWithBoard(_positionX + horizontalMove,
                    _positionY + verticalMove))
                {
                    if (movement == Movement.Down)
                    {
                        // the shape lies at the bottom and is about to fall,
                        //  but there's not enough space below it -> OnFallenDown
                        _fallen = true;
                        _timer.Stop();
                        DoOnDropped(new BoardChangedEventArgs(GetChangedBlock(_positionX + horizontalMove, _positionY)));
                    }
                    return;
                }

                byte[,] blocks = this._Board.GetBlocks();
                // move
                _positionX += horizontalMove;
                _positionY += verticalMove;
                OnMoved(this, new BoardChangedEventArgs(this._Board.GetChangedBlock(blocks, this._Board.GetBlocks())));


                if (_positionY == GetGhostPositionY() &&
                    IsCollidingWithBoard(_positionX + 1, _positionY) &&
                    IsCollidingWithBoard(_positionX - 1, _positionY)
                    || movement == Movement.Drop)
                {
                    // the shape has just fallen by this move, there is no posibility
                    //  to shift it in the horizontal direction -> the board must check
                    //  if there are any completed lines.
                    _fallen = true;
                    _timer.Stop();
                    DoOnDropped(new BoardChangedEventArgs(this._Board.GetChangedBlock(blocks, this._Board.GetBlocks())));
                }
            }
        }


        private void Rotate(bool rotateClockwise)
        {
            if (_fallen)
                return;

            byte[,] blocks = this._Board.GetBlocks();
            int shiftInX, shiftInY;
            this.Shape.RotateAroundCentre(rotateClockwise,
                out shiftInX, out shiftInY);

            // try to place the rotated shape
            // keep the shape centre in the same place
            int newPositionX = _positionX - shiftInX;
            int newPositionY = _positionY - shiftInY;
            // modify the new position so that the shape won't get 
            //  out of the board
            if (newPositionX < 0)
                newPositionX = 0;
            else if (newPositionX + this.Shape.Width > _Board.Width)
                newPositionX = _Board.Width - this.Shape.Width;
            if (newPositionY + this.Shape.Height > _Board.Height)
                newPositionY = _Board.Height - this.Shape.Height;

            if (IsCollidingWithBoard(newPositionX, newPositionY) == false)
            {
                _positionX = newPositionX;
                _positionY = newPositionY;
            }
            else
            {// new position not found
                this.Shape.RotateAroundCentre(!rotateClockwise,
                    out shiftInX, out shiftInY);
                return;
            }

            OnMoved(this, new BoardChangedEventArgs(this._Board.GetChangedBlock(blocks, this._Board.GetBlocks())));
        }


        private void DoOnDropped(BoardChangedEventArgs e)
        {
            this._Board.BeginUpdate();
            try
            {
                for (int i = 0; i < this.Shape.Width; i++)
                {
                    for (int j = 0; j < this.Shape.Height; j++)
                    {
                        if (this.Shape[i, j] != 0)
                            this._Board[i + _positionX, j + _positionY] = this.Shape[i, j];
                    }
                }
                _fallen = true;
            }
            finally
            {
                this._Board.EndUpdate();
            }
            if (OnDropped != null)
                OnDropped(this, e);
        }

        private enum Movement
        {
            Left, Right, Down, Drop
        }

        internal byte GetBlockInShape(int x, int y)
        {
            if (_fallen)
                return 0;
            x -= _positionX;
            y -= _positionY;
            if (x >= 0 && x < this.Shape.Width && y >= 0 && y < this.Shape.Height)
                return this.Shape[x, y];
            return 0;
        }

        #region IDisposable Members

        public void Dispose()
        {
            _timer.OnElapsed -= _timer_Elapsed;
        }
        #endregion
    }
}
