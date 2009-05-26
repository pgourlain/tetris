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
using System.Windows.Media.Media3D;
using System.Windows.Media.Animation;
using System.Threading;

namespace GeniusTetris._3D
{
    /// <summary>
    /// Interaction logic for View3DUC.xaml
    /// </summary>

    public partial class View3DUC : System.Windows.Controls.UserControl
    {
        public View3DUC()
        {
            InitializeComponent();
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            //GeniusTetris.Core.ShapeQueue queue = new GeniusTetris.Core.ShapeQueue();
            ////GeniusTetris.Core.Shape sh = new GeniusTetris.Core.ZShapeLeft();
            //GeniusTetris.Core.Shape sh = queue.PickUpShape();
            //ModelVisual3D model = Tetris3DModelProvider.CreateShape(sh);
            //topModel.Children.Add(model);
            StartAnimation();
        }

        public GeniusTetris.Core.Shape Shape
        {
            set
            {
                float xCenter = value.GetCentreInX();
                float yCenter = value.GetCentreInY();
                myTranslateTransform3D.OffsetX = -xCenter;
                myTranslateTransform3D.OffsetY = -yCenter;
                ModelVisual3D model = Tetris3DModelProvider.CreateShape(value);
                topModel.Children.Clear();
                topModel.Children.Add(model);
            }
        }

        delegate void StartAnimationDelegate();
        public void StartAnimation()
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                Thread.Sleep(new Random(System.Environment.TickCount).Next(1000));
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.ApplicationIdle,new StartAnimationDelegate(
                    delegate()
                    {
                        ((Storyboard)this.Resources["myStoryBoard"]).Begin(this);
                    }));

            });
        }
    }
}