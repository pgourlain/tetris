using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusTetris.Core
{
    public class GeniusArray<T> : ICloneable
    {
        private T[,] _Tab;

        public GeniusArray(int width, int height)
        {
            _Tab = new T[width, height];
        }

        #region protected
		protected virtual void Rotate(bool rotateClockwise)
		{
            T[,] _rotatedMatrix = new T[this.Height, this.Width];
			for (byte x = 0; x < this.Width; x++)
				for (byte y = 0; y < this.Height; y++)
				{
					if (rotateClockwise)
						_rotatedMatrix[y, x] = 
							_Tab[this.Width - x - 1, y];
					else
						_rotatedMatrix[y, x] =
                            _Tab[x, this.Height - y - 1];
				}
            _Tab = _rotatedMatrix;
		}        
        #endregion

        public virtual T this[int x, int y]
        {
            get
            {
                return _Tab[x, y];
            }
            set
            {
                _Tab[x, y] = value;
            }
        }

        public int Height
        {
            get { return (byte)_Tab.GetLength(1); }
        }

        public int Width
        {
            get { return (byte)_Tab.GetLength(0); }
        }

        public virtual void SetData(T[,] tab)
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    this[i,j] = tab[i,j];
                }
            }
        }

        #region ICloneable Members

        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}
