using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Diagnostics;

namespace GeniusTetris.Core
{
    /// <summary>
    /// 
    /// </summary>
    public class Board : GeniusArray<byte>
    {
        ShapeMover _Mover;
        int _InUpdate;
        Dictionary<Point, bool> _Changed;
        Random _random = new Random();
        private ITimer _Timer;

        internal object lockOperationPending = new object();

        public event EventHandler<BoardChangedEventArgs> OnChanged;
        public event EventHandler<LinesCompletedEventArgs> OnLinesCompleted;
        public event EventHandler<BoardChangedEventArgs> OnDropped;
        public event EventHandler OnBoardFull;
        public event EventHandler<BoardOptionsCapturedEventArgs> OnOptions;



        public Board(int w, int h, ITimer aTimer)
            : base(w, h)
        {
            _Timer = aTimer;
            _Changed = new Dictionary<Point, bool>();
        }

        public void ThrowShape(Shape ashape, int interval)
        {
            if (_Mover != null)
            {
                _Mover.OnCollisionAtStart -= new EventHandler(_Mover_OnCollisionAtStart);
                _Mover.OnDropped -= new EventHandler<BoardChangedEventArgs>(_Mover_OnDropped);
                _Mover.OnMoved -= new EventHandler<BoardChangedEventArgs>(_Mover_OnMoved);
                _Mover.Dispose();
            }
            _Mover = new ShapeMover(this, ashape, _Timer);
            _Mover.OnCollisionAtStart += _Mover_OnCollisionAtStart;
            _Mover.OnDropped += _Mover_OnDropped;
            _Mover.OnMoved += _Mover_OnMoved;
            _Mover.ThrowIntoBoard(interval);
        }

        void _Mover_OnMoved(object sender, BoardChangedEventArgs e)
        {
            if (OnChanged != null)
                OnChanged(this, e);
        }

        void _Mover_OnDropped(object sender, BoardChangedEventArgs e)
        {
            CheckAfterDrop(true, e);
        }

        /// <summary>
        /// cette method est appelée quand une piece est tombé ou que l'option 'g'ravity est executer
        /// </summary>
        /// <param name="fromDrop">génère les événements drop si le paramètre vaut "true"</param>
        private void CheckAfterDrop(bool fromDrop, BoardChangedEventArgs e)
        {
            // prepare for deleting
            int[] linesCompleted = GetLinesCompleted();

            if (linesCompleted.Length == 0)
            {
                if (fromDrop && OnDropped != null)
                    OnDropped(this, e);
                return;
            }
            byte[,] blocks = GetBlocksWithoutMover();
            _InUpdate++;
            try
            {
                for (int i = 0; i < linesCompleted.Length; i++)
                {
                    int lineToDelete = linesCompleted[i];
                    ShiftSegmentDown(lineToDelete + 1, true);
                }
                AddOptions(linesCompleted.Length);
            }
            finally
            {
                _InUpdate--;
            }
            _Mover_OnMoved(this, new BoardChangedEventArgs(GetChangedBlock(blocks, this.GetBlocks())));

            if (OnLinesCompleted != null)
                OnLinesCompleted(this, new LinesCompletedEventArgs(linesCompleted.Length));

            if (fromDrop && OnDropped != null)
                OnDropped(this, e);
        }

        /// <summary>
        /// ajoute nbOptions dans le tableau, choisi au hazard
        /// </summary>
        /// <param name="nbOptions"></param>
        private void AddOptions(int nbOptions)
        {
            byte[] options = Options.Available;
            List<Point> blocks = new List<Point>();

            //récupération des la liste des points, succeptible d'être remplacer par une option
            for (int i = 0; i < this.Width; i++)
            {
                for (int j = 0; j < this.Height; j++)
                {
                    byte b = this.ValueOf(i, j);
                    if (b > 0 && b < 65)
                        blocks.Add(new Point(i, j));
                }
            }

            Random rnd = new Random(System.Environment.TickCount);
            while (nbOptions-- > 0)
            {
#if DEBUG 
                int index = Array.FindIndex<byte>(options, 
                    delegate(byte item) 
                        {
                            //return item == (byte)'r';
                            return item == (byte)'o'; 
                        });
#else
                int index = rnd.Next(options.Length);
#endif
                if (blocks.Count == 0)
                {
                    //no blocks at screen o replace by
                    this[rnd.Next(this.Width), 0] = options[index];
                }
                else
                {
                    int indexofblock = rnd.Next(blocks.Count);
                    Point pt = blocks[indexofblock];
                    blocks.RemoveAt(indexofblock);
                    this[pt.X, pt.Y] = options[index];
                }
            }
        }

        /// <summary>
        /// efface les options du plateau
        /// </summary>
        private void ClearOptions()
        {
            for (int i = 0; i < this.Width; i++)
            {
                for (int j = 0; j < this.Height; j++)
                {
                    byte b = this.ValueOf(i, j);
                    if (b >= 65)
                        this[i, j] = 1;
                }
            }
        }


        void _Mover_OnCollisionAtStart(object sender, EventArgs e)
        {
            if (OnBoardFull != null)
                OnBoardFull(this, EventArgs.Empty);
        }

        /// <summary>
        /// renvoi la list des points du tableau ayant changés
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        internal Dictionary<Point, bool> GetChangedBlock(byte[,] a, byte[,] b)
        {
            Dictionary<Point, bool> Result = new Dictionary<Point, bool>();
            for (int i = 0; i <= a.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= a.GetUpperBound(1); j++)
                {
                    if (a[i, j] != b[i, j])
                        Result.Add(new Point(i, j), true);
                }
            }
            return Result;
        }

        /// <summary>
        /// récuperation des blocks du tableau, y compris les blocks de la pièce en déplacement
        /// </summary>
        /// <returns></returns>
        internal byte[,] GetBlocks()
        {
            byte[,] Result = new byte[this.Width, this.Height];
            for (int i = 0; i < this.Width; i++)
            {
                for (int j = 0; j < this.Height; j++)
                {
                    Result[i, j] = this[i, j];
                }
            }
            return Result;
        }
        /// <summary>
        /// retourne la liste des ligne completes
        /// </summary>
        /// <returns></returns>
        private int[] GetLinesCompleted()
        {
            List<int> linesCompleted = new List<int>();
            for (int line = this.Height - 1; line >= 0; line--)
            {
                bool lineCompleted = true;
                for (int column = 0; column < this.Width; column++)
                {
                    if (this[column, line] == 0)
                    {
                        lineCompleted = false;
                        break;
                    }
                }
                if (lineCompleted)
                    linesCompleted.Add(line);
            }
            return linesCompleted.ToArray();
        }

        /// <summary>
        /// descend le plteau d'un cran
        /// </summary>
        /// <param name="fromLine"></param>
        /// <param name="checkOption"></param>
        private void ShiftSegmentDown(int fromLine, bool checkOption)
        {
            if (fromLine < 1)
                return;
            List<byte> options = new List<byte>();
            lock (lockOperationPending)
            {
                BeginUpdate();
                try
                {
                    for (int line = fromLine; line < this.Height; line++)
                        for (int column = 0; column < this.Width; column++)
                        {
                            byte b = this.ValueOf(column, line - 1);
                            if (checkOption && b >= (byte)'a' && b <= (byte)'z')
                                options.Add(b);
                            this[column, line - 1] = this.ValueOf(column, line);
                            this[column, line] = 0;
                        }
                }
                finally
                {
                    EndUpdate();
                }
            }
            if (OnOptions != null && checkOption)
                OnOptions(this, new BoardOptionsCapturedEventArgs(options.ToArray()));
        }

        /// <summary>
        /// ajoute une ligne au tableau
        /// </summary>
        /// <param name="nbline"></param>
        private void ShiftSegmentUp(int nbline, byte[,] addingValues)
        {
            lock (lockOperationPending)
            {
                Debug.WriteLine(string.Format("{0} : ShiftSegmentUp", Thread.CurrentThread.ManagedThreadId));
                BeginUpdate();
                try
                {
                    for (int line = this.Height - nbline - 1; line >= 0; line--)
                    {
                        for (int column = 0; column < this.Width; column++)
                        {
                            this[column, line + nbline] = this.ValueOf(column, line);
                            if (addingValues != null && line <= addingValues.GetUpperBound(1))
                                this[column, line] = addingValues[column, line];
                            else
                                this[column, line] = 0;
                        }
                    }
                }
                finally
                {
                    EndUpdate();
                }
            }
        }

        public void BeginUpdate()
        {
            _InUpdate++;
        }

        public void EndUpdate()
        {
            _InUpdate--;
            if (_InUpdate <= 0)
            {
                _InUpdate = 0;
                DoChanged(-1, -1);
            }
        }

        /// <summary>
        /// renvoi le tableau complet, avec la piece en mouvement
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override byte this[int x, int y]
        {
            get
            {
                if (_Mover != null)
                    return (byte)(_Mover.GetBlockInShape(x, y) | base[x, y]);
                return base[x, y];
            }
            set
            {
                if (base[x, y] != value)
                {
                    base[x, y] = value;
                    DoChanged(x, y);
                }
            }
        }

        private void DoChanged(int x, int y)
        {
            if (x > -1 && y > -1)
                _Changed[new Point(x, y)] = true;
            if (_InUpdate <= 0 && _Changed.Count > 0)
            {
                if (OnChanged != null)
                    OnChanged(this, new BoardChangedEventArgs(_Changed));
                _Changed = new Dictionary<Point, bool>();
            }
        }

        public void Reset()
        {
            BeginUpdate();
            try
            {
                for (int i = 0; i < this.Width; i++)
                    for (int j = 0; j < this.Height; j++)
                    {
                        this[i, j] = 0;
                    }
            }
            finally
            {
                EndUpdate();
            }
        }

        internal byte ValueOf(int x, int y)
        {
            return base[x, y];
        }

        internal void MoveLeft()
        {
            if (_Mover != null)
                _Mover.MoveLeft();
        }

        internal void MoveRight()
        {
            if (_Mover != null)
                _Mover.MoveRight();
        }

        internal void RotateRight()
        {
            if (_Mover != null)
                _Mover.RotateRight();
        }

        internal void RotateLeft()
        {
            if (_Mover != null)
                _Mover.RotateLeft();
        }

        internal void Drop()
        {
            if (_Mover != null)
                _Mover.Drop();
        }

        internal void MoveDown()
        {
            if (_Mover != null)
                _Mover.MoveDown();
        }

        #region options actions
        public void AddLine()
        {
            byte[,] values = new byte[this.Width, 1];

            int nb = _random.Next(this.Width);
            for (int i = 0; i < nb; i++)
            {
                values[_random.Next(values.Length), 0] = (byte)(_random.Next(8) + 1); //8 nombre maxi pour indiqué une couleur
            }
            this.ShiftSegmentUp(1, values);
        }

        public void RemoveLine()
        {
            this.ShiftSegmentDown(1, false);
        }

        public void ClearOptionsOnBoard()
        {
            ClearOptions();
        }
        #endregion


        internal void EraseBoard()
        {
            NukeField();
        }

        internal void BlockBomb()
        {
            BeginUpdate();
            try
            {
                for (int i = 0; i < this.Width; i++)
                    for (int j = 0; j < this.Height; j++)
                    {
                        if (this[i, j] == (byte)'o')
                        {
                            Explode(i, j, true);
                        }
                    }
            }
            finally
            {
                EndUpdate();
            }
        }

        private void Explode(int i, int j, bool center)
        {
            if (i >= 0 && i < this.Width && j >= 0 && j < this.Height)
                this[i, j] = 0;
            if (center)
            {
                Explode(i - 1, j, false);
                Explode(i - 1, j - 1, false);
                Explode(i - 1, j + 1, false);
                Explode(i, j - 1, false);
                Explode(i, j + 1, false);

                Explode(i + 1, j, false);
                Explode(i + 1, j+1, false);
                Explode(i + 1, j-1, false);
            }
        }

        internal void NukeField()
        {
            //efface tout
            BeginUpdate();
            try
            {
                Reset();
            }
            finally
            {
                EndUpdate();
            }
        }

        internal void ChangedBoard(byte[,] blocks)
        {
            SetData(blocks);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public byte[,] GetBlocksWithoutMover()
        {
            byte[,] Result = new byte[this.Width, this.Height];
            for (int i = 0; i < this.Width; i++)
            {
                for (int j = 0; j < this.Height; j++)
                {
                    Result[i, j] = this.ValueOf(i, j);
                }
            }
            return Result;
        }

        internal void Gravity()
        {
            BeginUpdate();
            try
            {
                for (int i = 0; i < this.Width; i++)
                {
                    int jDest = 0;
                    for (int j = 0; j < this.Height; j++)
                    {
                        byte b = this.ValueOf(i, j);
                        if (j != jDest && b > 0)
                            this[i, j] = 0;
                        this[i, jDest] = b;
                        if (b > 0)
                            jDest++;
                    }
                }
                CheckAfterDrop(false, null);
            }
            finally
            {
                EndUpdate();
            }
        }

        public void GravityLeft()
        {
            BeginUpdate();
            try
            {
                for (int j = 0; j < this.Height; j++)
                {
                    int iDest = 0;
                    for (int i = 0; i < this.Width; i++)
                    {
                        byte b = this.ValueOf(i, j);
                        if (i != iDest && b > 0)
                            this[i, j] = 0;
                        this[iDest, j] = b;
                        if (b > 0)
                            iDest++;
                    }
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        internal void Stop()
        {
            if (_Mover != null)
            {
                _Mover.Stop();
                OnChanged(this, new BoardChangedEventArgs(null, true));
            }
        }

        internal void GravityRight()
        {
            BeginUpdate();
            try
            {
                for (int j = 0; j < this.Height; j++)
                {
                    int iDest = this.Width - 1;
                    for (int i = this.Width-1; i >=0 ; i--)
                    {
                        byte b = this.ValueOf(i, j);
                        if (i != iDest && b > 0)
                            this[i, j] = 0;
                        this[iDest, j] = b;
                        if (b > 0)
                            iDest--;
                    }
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        internal void Quake()
        {
            BeginUpdate();
            try
            {
                List<byte> line = new List<byte>();
                for (int j = 0; j < this.Height; j++)
                {
                    int shift = _random.Next(11);
                    //i can shift in positive of negative
                    line.Clear();
                    for(int i=0; i < this.Width; i++)
                        line.Add(this.ValueOf(i,j));

                    for (int i = 0; i < shift;i++)
                        if (shift - 5 < 0)
                        {
                            line.Add(line[0]);
                            line.RemoveAt(0);
                        }
                        else
                        {
                            line.Insert(0, line[line.Count-1]);
                            line.RemoveAt(line.Count - 1);
                        }
                    for (int i = 0; i < this.Width; i++)
                        this[i,j] = line[i];
                }
            }
            finally
            {
                EndUpdate();
            }
        }

        public override void SetData(byte[,] atab)
        {
            BeginUpdate();
            try
            {
                base.SetData(atab);
            }
            finally
            {
                EndUpdate();
            }
        }
    }
}
