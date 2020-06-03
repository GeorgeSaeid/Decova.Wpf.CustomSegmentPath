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
using System.Windows.Shapes;
using System.Windows.Markup;
using System.Runtime.CompilerServices;

namespace Decova.Wpf
{
    //[ContentProperty("Text")]
    public class CustomSegmentPath : Control
    {
        #region initialization
        //####################################################################
        static CustomSegmentPath()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomSegmentPath),
                                                     new FrameworkPropertyMetadata(typeof(CustomSegmentPath)));
        }

        public CustomSegmentPath()
        {
            this.SegmentLength = 10;
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _layoutPanel = this.GetTemplateChild("LayoutPanel") as Panel;
            if (_layoutPanel == null)
                throw new Exception("Could not find template part: LayoutPanel");
        }


        Panel _layoutPanel;
        //####################################################################
        #endregion

        #region Path
        //####################################################################
        public Geometry Path
        {
            get { return (Geometry)GetValue(TextPathProperty); }
            set { SetValue(TextPathProperty, value); }
        }

        public static readonly DependencyProperty TextPathProperty =
            DependencyProperty.Register("Path", typeof(Geometry), typeof(CustomSegmentPath),
            new FrameworkPropertyMetadata(null, new PropertyChangedCallback(OnPathPropertyChanged)));

        static void OnPathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomSegmentPath customPath = d as CustomSegmentPath;

            if (customPath == null)
                return;

            if (e.NewValue == e.OldValue || e.NewValue == null)
                return;

            customPath.Path.Transform = null;

            //Decova.Wpf.UpdateSize();
            customPath.Update();
        }
        //####################################################################
        #endregion

        #region SegmentLength
        //***********************************************************************************************	
        public static readonly DependencyProperty SegmentLengthProperty = DependencyProperty.Register("SegmentLength", typeof(double), typeof(CustomSegmentPath));
        public double SegmentLength
        {
            get
            {
                return (double)(this.GetValue(SegmentLengthProperty));
            }
            set
            {
                this.SetValue(SegmentLengthProperty, value);
            }
        }
        //***********************************************************************************************	
        #endregion

        #region IsPathVisible
        //####################################################################
        /// <summary>
        /// Set this property to True to display the Path geometry in the control
        /// </summary>
        public bool IsPathVisible
        {
            get { return (bool)GetValue(IsPathVisibleProperty); }
            set { SetValue(IsPathVisibleProperty, value); }
        }

        public static readonly DependencyProperty IsPathVisibleProperty =
            DependencyProperty.Register("IsPathVisible", typeof(bool), typeof(CustomSegmentPath),
            new PropertyMetadata(false, new PropertyChangedCallback(OnDrawPathPropertyChanged)));

        static void OnDrawPathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomSegmentPath customPath = d as CustomSegmentPath;

            if (customPath == null)
                return;

            if (e.NewValue == e.OldValue || e.NewValue == null)
                return;

            customPath.Update();
        }
        //####################################################################
        #endregion

        #region VisiblePathLine
        //####################################################################
        /// <summary>
        /// Set this property to True to display the line segments under the text (flattened path)
        /// </summary>
        public bool VisiblePathLine
        {
            get { return (bool)GetValue(VisiblePathLineProperty); }
            set { SetValue(VisiblePathLineProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DrawFlattendPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisiblePathLineProperty =
            DependencyProperty.Register("VisiblePathLine", typeof(bool), typeof(CustomSegmentPath),
            new PropertyMetadata(false, new PropertyChangedCallback(OnVisiblePathLinePropertyChanged)));

        static void OnVisiblePathLinePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomSegmentPath customPath = d as CustomSegmentPath;

            if (customPath == null)
                return;

            if (e.NewValue == e.OldValue || e.NewValue == null)
                return;

            customPath.Update();
        }
        //####################################################################
        #endregion

        #region Segment
        //***********************************************************************************************	
        public static readonly DependencyProperty SegmentProperty = DependencyProperty.Register("Segment", typeof(Geometry), typeof(CustomSegmentPath), new PropertyMetadata(Geometry.Parse("M0 0 L5 5")));
        public Geometry Segment
        {
            get
            {
                return (Geometry)(this.GetValue(SegmentProperty));
            }
            set
            {
                this.SetValue(SegmentProperty, value);
            }
        }
        //***********************************************************************************************	
        #endregion

        void Update()
        {
            if (Path == null || _layoutPanel == null) return;

            List<Point> intersectionPoints;

            PathGeometry flattenedPath = Path.GetFlattenedPathGeometry();
            intersectionPoints = flattenedPath.GetSegmentsIntersectionsWithPath(this.SegmentLength);

            _layoutPanel.Children.Clear();

            for (int i = 0; i < intersectionPoints.Count - 1; i++)
            {
                double oppositeLen = Math.Sqrt(Math.Pow(intersectionPoints[i].X + this.SegmentLength - intersectionPoints[i + 1].X, 2.0) + Math.Pow(intersectionPoints[i].Y - intersectionPoints[i + 1].Y, 2.0)) / 2.0;
                double hypLen = Math.Sqrt(Math.Pow(intersectionPoints[i].X - intersectionPoints[i + 1].X, 2.0) + Math.Pow(intersectionPoints[i].Y - intersectionPoints[i + 1].Y, 2.0));

                double slope = oppositeLen / hypLen;
                slope = Math.Max(-1.0, Math.Min(1, slope));


                #region calc angle
                //####################################################################
                double angle = 2.0 * Math.Asin(slope) * 180.0 / Math.PI;
                if ((intersectionPoints[i].X + this.SegmentLength) > intersectionPoints[i].X)
                {
                    if (intersectionPoints[i + 1].Y < intersectionPoints[i].Y)
                        angle = -angle;
                }
                else
                {
                    if (intersectionPoints[i + 1].Y > intersectionPoints[i].Y)
                        angle = -angle;
                }
                //####################################################################
                #endregion

                UIElement currTextBlock = new Path()
                {
                    Data = this.Segment,// new EllipseGeometry(new Point(3, 3), 3, 3, new TranslateTransform(-3, -3)),
                    Fill = new SolidColorBrush(Colors.Tomato),
                    Width = this.SegmentLength,//6,
                    Height = this.SegmentLength,
                    //Stroke = new SolidColorBrush(Colors.Green),
                };
                

                RotateTransform rotate = new RotateTransform(angle);
                TranslateTransform translate = new TranslateTransform(intersectionPoints[i].X - currTextBlock.DesiredSize.Width, 
                                                                      intersectionPoints[i].Y - currTextBlock.DesiredSize.Height);
                TransformGroup transformGrp = new TransformGroup();
                transformGrp.Children.Add(rotate);
                transformGrp.Children.Add(translate);
                currTextBlock.RenderTransform = transformGrp;

                _layoutPanel.Children.Add(currTextBlock);
            }

            // don't draw path if already drawing line path
            if (IsPathVisible == true && VisiblePathLine == false)
            {
                Path path = new Path();
                path.Data = Path;
                path.Stroke = Brushes.Black;
                _layoutPanel.Children.Add(path);
            }
        }
    }
}
