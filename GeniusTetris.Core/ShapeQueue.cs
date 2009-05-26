using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace GeniusTetris.Core
{
    public class ShapeQueue
    {
        private Queue<Shape> _queue;
        private List<string> _definedShapes = new List<string>();
        private int _maxDefinedHeight = 0;
        private Random _random = new Random();

        public event EventHandler Changed;

        public ShapeQueue()
            : this(4)
        {
        }

        public ShapeQueue(byte queueLength)
        {
            SetDefinedShapes();
            FillQueue(queueLength);
        }

        public byte Length
        {
            get { return (byte)_queue.Count; }
        }

        public int MaxDefinedHeight
        {
            get
            {
                return _maxDefinedHeight;
            }
        }

        public Shape[] GetQueueImage()
        {
            lock (_queue)
            {
                return _queue.ToArray();
            }
        }

        private void OnChanged(EventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }

        public Shape PickUpShape()
        {
            Shape shape =_queue.Dequeue();
            _queue.Enqueue(GetRandomShape());
            OnChanged(EventArgs.Empty);
            return shape;
        }

        private void FillQueue(byte queueLength)
        {
            _queue = new Queue<Shape>(queueLength);
            for (byte counter = 0; counter < queueLength; counter++)
            {
                _queue.Enqueue(GetRandomShape());
            }
        }

        private Shape GetRandomShape()
        {
            int randomShapeIndex = _random.Next(_definedShapes.Count);
            return (Shape)Assembly.GetExecutingAssembly().CreateInstance(
                _definedShapes[randomShapeIndex].ToString());
        }

        private void SetDefinedShapes()
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            Type[] definedTypes = thisAssembly.GetTypes();
            foreach (Type definedType in definedTypes)
            {
                if (definedType.IsClass && definedType.IsSealed
                    && definedType.IsSubclassOf(typeof(Shape)))
                {
                    _definedShapes.Add(definedType.FullName);
                    Shape shape =
                        (Shape)thisAssembly.CreateInstance(definedType.FullName);
                    if (shape.Height > _maxDefinedHeight)
                        _maxDefinedHeight = shape.Height;
                }
            }
        }
    }
}
