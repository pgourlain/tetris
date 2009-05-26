using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusTetris.Core
{
    public sealed class Square : Shape
    {
        public Square()
            : base(2, 2)
        {
            this[0, 0] = this[0, 1] = this[1, 0] = this[1, 1] = 1;
        }
    }

    public sealed class Line : Shape
    {
        public Line()
            : base(4, 1)
        {
            this[0, 0] = this[1, 0] = this[2, 0] = this[3, 0] = 2;
        }
    }

    public sealed class LShapeLeft : Shape
    {
        public LShapeLeft()
            : base(3, 2)
        {
            this[0, 0] = this[1, 0] = this[2, 0] = this[0, 1] = 3;
        }
    }

    public sealed class LShapeRight : Shape
    {
        public LShapeRight()
            : base(3, 2)
        {
            this[0, 0] = this[0, 1] = this[1, 1] = this[2, 1] = 4;
        }
    }

    public sealed class ZShapeLeft : Shape
    {
        public ZShapeLeft()
            : base(3, 2)
        {
            this[0, 1] = this[1, 1] = this[1, 0] = this[2, 0] = 5;
        }
    }

    public sealed class ZShapeRight : Shape
    {
        public ZShapeRight()
            : base(3, 2)
        {
            this[0, 0] = this[1, 0] = this[1, 1] = this[2, 1] = 6;
        }
    }

    public sealed class TShape : Shape
    {
        public TShape()
            : base(3, 2)
        {
            this[0, 0] = this[1, 0] = this[1, 1] = this[2, 0] = 7;
        }
    }

    public sealed class UShape : Shape
    {
        public UShape()
            : base(3, 2)
        {
            this[0, 0] = this[1, 0] = this[2, 0] = this[0, 1] = this[2, 1] = 8;
        }
    }

}
