using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Diagnostics;

namespace Microsoft.Samples.KMoore.WPFSamples.AnimatingTilePanel
{
    public class AniTilePanel : Panel
    {

        public AniTilePanel()
        {
            CompositionTarget_RenderingHandler = new EventHandler(CompositionTarget_Rendering);
            
            //need to make sure we only run the ticker when the control is actually loaded            
            this.Loaded += new RoutedEventHandler(AniTilePanel_Loaded);
            this.Unloaded += new RoutedEventHandler(AniTilePanel_Unloaded);
        }

        void AniTilePanel_Loaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering += CompositionTarget_RenderingHandler;
        }
        void AniTilePanel_Unloaded(object sender, RoutedEventArgs e)
        {
            CompositionTarget.Rendering -= CompositionTarget_RenderingHandler;
        }

        // Measures the children
        protected override Size MeasureOverride(Size availableSize)
        {
            Size theChildSize = new Size(this.ItemWidth, this.ItemHeight);

            foreach (UIElement child in Children)
            {
                child.Measure(theChildSize);
            }

            int childrenPerRow;

            // Figure out how many children fit on each row
            if (availableSize.Width == Double.PositiveInfinity)
                childrenPerRow = this.Children.Count;
            else
                childrenPerRow = Math.Max(1, (int)Math.Floor(availableSize.Width / this.ItemWidth));

            // Calculate the width and height this results in
            double height;         
            double width;

            if (childrenPerRow < this.Children.Count)
                width = childrenPerRow * this.ItemWidth;
            else
                width = this.Children.Count * this.ItemWidth;


            if (childrenPerRow != this.Children.Count)
                height = this.ItemHeight * (Math.Floor((double)this.Children.Count / childrenPerRow) + 1);
            else
                height = this.ItemHeight;
            return new Size(width, height + InnerTopMargin);
        }

        // Arrange the children
        protected override Size ArrangeOverride(Size finalSize)
        {
            // Calculate how many children fit on each row
            int childrenPerRow = Math.Max(1, (int)Math.Floor(finalSize.Width / this.ItemWidth));

            Size theChildSize = new Size(this.ItemWidth, this.ItemHeight);
            for (int i = 0; i < this.Children.Count; i++)
            {
                UIElement child = this.Children[i];

                // Figure out where the child goes
                Point newOffset = CalcChildOffset(i, childrenPerRow, this.ItemWidth, this.ItemHeight, finalSize.Width, this.Children.Count);

                newOffset.Offset(0, InnerTopMargin);
                //set the location attached DP
                child.SetValue(ChildTargetProperty, newOffset);


                if (child.ReadLocalValue(ChildLocationProperty) == DependencyProperty.UnsetValue)
                {
                    child.SetValue(ChildLocationProperty, newOffset);
                    child.Arrange(new Rect(newOffset, theChildSize));
                }
                else
                {
                    Point currentOffset = (Point)child.GetValue(ChildLocationProperty);
                    // Position the child and set its size
                    child.Arrange(new Rect(currentOffset, theChildSize));
                }
            }
            return finalSize;
        }


        #region public properties
        public double ItemWidth
        {
            get { return (double)GetValue(ItemWidthProperty); }
            set { SetValue(ItemWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemWidthProperty =
            DependencyProperty.Register("ItemWidth", typeof(double), typeof(AniTilePanel),
            new FrameworkPropertyMetadata((double)50, FrameworkPropertyMetadataOptions.AffectsMeasure));

        public double ItemHeight
        {
            get { return (double)GetValue(ItemHeightProperty); }
            set { SetValue(ItemHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register("ItemHeight", typeof(double), typeof(AniTilePanel),
            new FrameworkPropertyMetadata((double)50, FrameworkPropertyMetadataOptions.AffectsMeasure));


        public static readonly DependencyProperty DampeningProperty = DependencyProperty.Register(
            "Dampening", typeof(double), typeof(AniTilePanel),
            new FrameworkPropertyMetadata((double).8));

        public double Dampening
        {
            get
            {
                return (double)GetValue(DampeningProperty);
            }
            set
            {
                SetValue(DampeningProperty, value);
            }
        }

        public static readonly DependencyProperty AttractionProperty = DependencyProperty.Register(
            "Attraction", typeof(double), typeof(AniTilePanel),
            new FrameworkPropertyMetadata((double)2));

        public double Attraction
        {
            get
            {
                return (double)GetValue(AttractionProperty);
            }
            set
            {
                SetValue(AttractionProperty, value);
            }
        }

        public static readonly DependencyProperty InnerTopMarginProperty = DependencyProperty.Register(
    "InnerTopMargin", typeof(double), typeof(AniTilePanel),
    new FrameworkPropertyMetadata((double)0));

        public double InnerTopMargin
        {
            get
            {
                return (double)GetValue(InnerTopMarginProperty);
            }
            set
            {
                SetValue(InnerTopMarginProperty, value);
            }
        }
        #endregion

        #region private attached properties

        private static readonly DependencyProperty ChildLocationProperty
            = DependencyProperty.RegisterAttached("ChildLocation", typeof(Point), typeof(AniTilePanel)
            , new FrameworkPropertyMetadata(new Point(), FrameworkPropertyMetadataOptions.AffectsParentArrange));

        public static void SetChildLocation(UIElement element, Point point)
        {
            element.SetValue(ChildLocationProperty, point);
        }
        public static Point GetChildLocation(UIElement element)
        {
            return (Point)element.GetValue(ChildLocationProperty);
        }

        private static readonly DependencyProperty ChildTargetProperty
            = DependencyProperty.RegisterAttached("ChildTarget", typeof(Point), typeof(AniTilePanel));

        public static void SetChildTarget(UIElement element, Point point)
        {
            element.SetValue(ChildTargetProperty, point);
        }
        public static Point GetChildTarget(UIElement element)
        {
            return (Point)element.GetValue(ChildTargetProperty);
        }

        private static readonly DependencyProperty VelocityProperty
            = DependencyProperty.RegisterAttached("Velocity", typeof(Vector), typeof(AniTilePanel));

        public static void SetVelocity(UIElement element, Vector Vector)
        {
            element.SetValue(VelocityProperty, Vector);
        }
        public static Vector GetVelocity(UIElement element)
        {
            return (Vector)element.GetValue(VelocityProperty);
        }

        #endregion

        #region private methods

        void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            long nowTick = DateTime.Now.Ticks;
            long diff = nowTick - _lastTick;
            _lastTick = nowTick;

            double seconds = SecondsFromTicks(diff);

            double dampening = this.Dampening;
            double attractionFactor = this.Attraction;

            foreach (UIElement child in Children)
            {
                updateElement(child, seconds, dampening, attractionFactor);
            }
        }


        static void updateElement(UIElement element, double seconds, double dampening, double attractionFactor)
        {
            Point current = (Point)element.GetValue(ChildLocationProperty);
            Point target = (Point)element.GetValue(ChildTargetProperty);
            Vector velocity = (Vector)element.GetValue(VelocityProperty);

            Vector diff = target - current;

            //_count++;

            if (diff.Length > Diff || velocity.Length > Diff)
            {
                //_goodCount++;


                velocity.X *= dampening;
                velocity.Y *= dampening;

                velocity += diff;

                Vector delta = velocity * seconds * attractionFactor;

                //velocity shouldn't be greater than...maxVelocity?
                double maxVelocity = 100;
                delta *= (delta.Length > maxVelocity) ? (maxVelocity / delta.Length) : 1;

                current += delta;

                element.SetValue(ChildLocationProperty, current);
                element.SetValue(VelocityProperty, velocity);
            }
        }

        // Given a child index, child size and children per row, figure out where the child goes
        private static Point CalcChildOffset(int index, int childrenPerRow, double itemWidth, double itemHeight, double panelWidth, int totalChildren)
        {
            double fudge = 0;
            if (totalChildren > childrenPerRow)
            {
                fudge = (panelWidth - childrenPerRow * itemWidth) / childrenPerRow;
                Debug.Assert(fudge >= 0);
            }

            int row = index / childrenPerRow;
            int column = index % childrenPerRow;
            return new Point(.5 * fudge + column * (itemWidth + fudge), row * itemHeight);
        }


        private static double SecondsFromTicks(long diff)
        {
            double seconds = diff / (double)10000000; //1 tick = 100-nanoseconds, so 10,000,000
            return seconds;
        }
        #endregion

        private readonly EventHandler CompositionTarget_RenderingHandler;
        private long _lastTick = long.MinValue;
        private const double Diff = 0.1;

    }
}
