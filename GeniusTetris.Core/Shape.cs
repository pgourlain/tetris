using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusTetris.Core
{
    public class Shape : GeniusArray<byte>
    {
        private bool _wasLastRotationClockwise = true;
        private RoundingRules _rounding = new RoundingRules();

        public Shape(int w, int h)
            : base(w, h)
        {
        }

        internal void RotateAroundCentre(bool rotateClockwise, out int shiftInX, out int shiftInY)
        {
            shiftInX = 0; shiftInY = 0;
            float centreXOld = GetCentreInX();
            float centreYOld = GetCentreInY();

            base.Rotate(rotateClockwise);

            float differenceInX = GetCentreInX() - centreXOld;
            float differenceInY = GetCentreInY() - centreYOld;

            bool useRoundingRules =
                Math.Abs((GetCentreInX() - GetCentreInY()) % 1f) == 0.5;
            if (useRoundingRules == false)
            {
                shiftInX = (int)differenceInX;
                shiftInY = (int)differenceInY;
            }
            else
            {	// when changing rotation direction, don't switch state
                if (rotateClockwise == _wasLastRotationClockwise)
                    _rounding.SwitchState(rotateClockwise);

                _rounding.RoundCoordinates(ref differenceInX, ref differenceInY,
                    rotateClockwise);
                shiftInX = (int)differenceInX;
                shiftInY = (int)differenceInY;
            }

            _wasLastRotationClockwise = rotateClockwise;
        }

        public float GetCentreInX()
        {
            return GetCentreCoordinate(true);
        }

        public float GetCentreInY()
        {
            return GetCentreCoordinate(false);
        }

        private float GetCentreCoordinate(bool horizontal)
        {
            byte fieldMass = 1;
            float mass = 0; float moment = 0;
            for (byte x = 0; x < this.Width; x++)
                for (byte y = 0; y < this.Height; y++)
                {
                    if (this[x, y] == 0)
                        continue;
                    mass += fieldMass;
                    moment += ((horizontal ? x : y) + 1) * fieldMass;
                }
            float coordinate = moment / mass;
            if (coordinate % 1f != 0.5)
                coordinate = (float)Math.Round(coordinate);
            return coordinate;
        }


        #region -- Rules For Rounding --

        private class RoundingRules
        {
            private byte _stateNumber = 0;
            private bool[,] _stateTable = new bool[,] {
				{ false, true }, { false, false }, { true, false }, { true, true } };

            public RoundingRules()
            {
            }

            private bool RoundXDown
            {
                get
                {
                    return _stateTable[_stateNumber, 0];
                }
            }

            private bool RoundYDown
            {
                get
                {
                    return _stateTable[_stateNumber, 1];
                }
            }

            public void SwitchState(bool rotateClockwise)
            {
                int newState = _stateNumber + (rotateClockwise ? 1 : -1);
                if (newState > 3)
                    _stateNumber = 0;
                else if (newState < 0)
                    _stateNumber = 3;
                else
                    _stateNumber = (byte)newState;
            }

            public void RoundCoordinates(ref float x, ref float y, bool rotateClockwise)
            {
                bool roundXDown = RoundXDown;
                bool roundYDown = RoundYDown;

                if (rotateClockwise == false)
                {
                    roundXDown = !roundXDown;
                    roundYDown = !roundYDown;
                }

                x = (float)(roundXDown ? Math.Floor(x) : Math.Ceiling(x));
                y = (float)(roundYDown ? Math.Floor(y) : Math.Ceiling(y));
            }
        }

        #endregion

    }
}
