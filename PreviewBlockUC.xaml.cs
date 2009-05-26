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
using GeniusTetris.Core;

namespace GeniusTetris
{
    /// <summary>
    /// Interaction logic for PreviewBlockUC.xaml
    /// </summary>

    public partial class PreviewBlockUC : System.Windows.Controls.UserControl
    {
        private ShapeQueue _ShapeQueue;
        private Board _PreviewBoard;

        public PreviewBlockUC()
        {
            InitializeComponent();
            _PreviewBoard = new Board(4, 8, null);
            this.preview.Board = _PreviewBoard;
        }

        public ShapeQueue ShapeQueue
        {
            get
            {
                return _ShapeQueue;
            }
            set
            {
                if (_ShapeQueue != value)
                {
                    if (_ShapeQueue != null)
                    {
                        _ShapeQueue.Changed -= new EventHandler(_ShapeQueue_Changed);
                    }
                    _ShapeQueue = value;
                    if (_ShapeQueue != null)
                    {
                        _ShapeQueue.Changed += new EventHandler(_ShapeQueue_Changed);
                    }
                }
                
            }
        }

        void _ShapeQueue_Changed(object sender, EventArgs e)
        {
            Shape[] shapes = _ShapeQueue.GetQueueImage();
            _PreviewBoard.BeginUpdate();
            try
            {

                _PreviewBoard.Reset();
                for (int index = 0; index < 2; index++)
                {
                    Shape shape = shapes[index];
                    for (int i = 0; i < shape.Width; i++)
                    {
                        for (int j = 0; j < shape.Height; j++)
                        {
                            _PreviewBoard[i, j + (1-index) * 4+1] = shape[i, j];
                        }
                    }
                }
            }
            finally
            {
                _PreviewBoard.EndUpdate();
            }

        }

    }
}