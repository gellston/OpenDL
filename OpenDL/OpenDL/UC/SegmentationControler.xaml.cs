using OpenDL.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace OpenDL.UC
{
    /// <summary>
    /// SegmentationControler.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SegmentationControler : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyRaised(string propertyname)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
            }
        }

        public void Set<T>(string _name, ref T _reference, T _value)
        {
            if(!Equals(_reference, _value))
            {
                _reference = _value;
                OnPropertyRaised(_name);
            }
        }

        public SegmentationControler()
        {
            InitializeComponent();



            this.ZoomStep = 0.5;
            this.ZoomMax = 5;
            this.ZoomMin = 0.2;
            this.Zoom = 1;
            this.PolygonCollection = null;
        }


        ~SegmentationControler()
        {

        }


        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(BitmapImage), typeof(SegmentationControler), new PropertyMetadata(OnCustomerChangedCallBack));
        public BitmapImage Image
        {
            get
            {
                return (BitmapImage)GetValue(ImageProperty);
            }

            set
            {
                SetValue(ImageProperty, value);
            }
        }

        private static void OnCustomerChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SegmentationControler control = sender as SegmentationControler;
            if (control != null)
            {
                BitmapImage image = e.NewValue as BitmapImage;

                control.CanvasWidth = image.Width;
                control.CanvasHeight = image.Height;
                control.TranslationX = 0;
                control.TranslationY = 0;
                ///control.Zoom = 1;
            }
        }


        private double _CanvasWidth = 0;
        public double CanvasWidth
        {
            get => _CanvasWidth;
            set => Set<double>(nameof(CanvasWidth), ref _CanvasWidth, value);
        }

        private double _CanvasHeight = 0;
        public double CanvasHeight
        {
            get => _CanvasHeight;
            set => Set<double>(nameof(CanvasHeight), ref _CanvasHeight, value);
        }


        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register("Zoom", typeof(double), typeof(SegmentationControler));
        public double Zoom
        {
            get
            {
                return (double)GetValue(ZoomProperty);
            }

            set
            {
                SetValue(ZoomProperty, value);
            }
        }

        public static readonly DependencyProperty ZoomMaxProperty = DependencyProperty.Register("ZoomMax", typeof(double), typeof(SegmentationControler));
        public double ZoomMax
        {
            get
            {
                return (double)GetValue(ZoomMaxProperty);
            }

            set
            {
                SetValue(ZoomMaxProperty, value);
            }
        }

        public static readonly DependencyProperty ZoomMinProperty = DependencyProperty.Register("ZoomMin", typeof(double), typeof(SegmentationControler));
        public double ZoomMin
        {
            get
            {
                return (double)GetValue(ZoomMinProperty);
            }

            set
            {
                SetValue(ZoomMinProperty, value);
            }
        }

        public static readonly DependencyProperty ZoomStepProperty = DependencyProperty.Register("ZoomStep", typeof(double), typeof(SegmentationControler));
        public double ZoomStep
        {
            get
            {
                return (double)GetValue(ZoomStepProperty);
            }

            set
            {
                SetValue(ZoomStepProperty, value);
            }
        }


        public static readonly DependencyProperty TranslationXProperty = DependencyProperty.Register("TranslationX", typeof(double), typeof(SegmentationControler));
        public double TranslationX
        {
            get
            {
                return (double)GetValue(TranslationXProperty);
            }

            set
            {
                SetValue(TranslationXProperty, value);
            }
        }


        public static readonly DependencyProperty TranslationYProperty = DependencyProperty.Register("TranslationY", typeof(double), typeof(SegmentationControler));
        public double TranslationY
        {
            get
            {
                return (double)GetValue(TranslationYProperty);
            }

            set
            {
                SetValue(TranslationYProperty, value);
            }
        }



        public static readonly DependencyProperty PolygonCollectionProperty = DependencyProperty.Register("PolygonCollection", typeof(ObservableCollection<SegmentLabelPolygon>), typeof(SegmentationControler));
        public ObservableCollection<SegmentLabelPolygon> PolygonCollection
        {
            get
            {
                return (ObservableCollection<SegmentLabelPolygon>)GetValue(PolygonCollectionProperty);
            }

            set
            {
                SetValue(PolygonCollectionProperty, value);
            }
        }


        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(SegmentLabelPolygon), typeof(SegmentationControler), new PropertyMetadata(OnSelectedItemChangedCallBack));
        public SegmentLabelPolygon SelectedItem
        {
            get
            {
                return (SegmentLabelPolygon)GetValue(SelectedItemProperty);
            }

            set
            {
                SetValue(SelectedItemProperty, value);
            }
        }
        private static void OnSelectedItemChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SegmentationControler control = sender as SegmentationControler;
            if (control != null)
            {
                control.SelectedItem = e.NewValue as SegmentLabelPolygon;
            }
        }


        public static readonly DependencyProperty TargetItemProperty = DependencyProperty.Register("TargetItem", typeof(SegmentLabelPolygon), typeof(SegmentationControler), new PropertyMetadata(OnTargetItemChangedCallBack));
        public SegmentLabelPolygon TargetItem
        {
            get
            {
                return (SegmentLabelPolygon)GetValue(TargetItemProperty);
            }

            set
            {
                SetValue(TargetItemProperty, value);
            }
        }

        private static void OnTargetItemChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SegmentationControler control = sender as SegmentationControler;
            if (control != null)
            {
                control.SelectedItem = null;
            }
        }



        private void ChildCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            var draggableControl = sender as Canvas;
            if (draggableControl.IsMouseCaptured == true)
                return;

            if (e.Delta > 0)
            {
                this.Zoom -= this.ZoomStep;
                if (this.Zoom <= this.ZoomMin)
                    this.Zoom = this.ZoomMin;
            }
            else
            {
                this.Zoom += this.ZoomStep;
                if (this.Zoom >= this.ZoomMax)
                    this.Zoom = this.ZoomMax;

            }
        }

        private Point CanvasStart;
        private Point CanvasOrigin;

        private void ChildCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pressedPoint = e.GetPosition(sender as IInputElement);

            HitTestResult hitTestResult = VisualTreeHelper.HitTest(sender as Visual, e.GetPosition(sender as IInputElement));
            if (hitTestResult == null) return;
            var element = hitTestResult.VisualHit;


            if(Keyboard.IsKeyDown(Key.LeftCtrl) == true && element.GetType() == typeof(Polygon)) {
                SegmentLabelPolygon datacontext = (element as Polygon).DataContext as SegmentLabelPolygon;
                datacontext.IsSelected = !datacontext.IsSelected;
                if (datacontext.IsSelected == true)
                    this.SelectedItem = datacontext;
                return;
            }

            if (Keyboard.IsKeyDown(Key.LeftShift) == true && element.GetType() == typeof(Canvas))
            {
                this.CanvasStart = e.GetPosition(this);
                this.CanvasOrigin = new Point(this.TranslationX, this.TranslationY);

                var draggableControl = sender as Canvas;
                draggableControl.CaptureMouse();
                return;
            }

            if (this.SelectedItem == null && this.TargetItem != null && this.Image != null && Keyboard.IsKeyDown(Key.LeftAlt) == true)
            {
                //SegmentationPolygon polygonItem = this.TargetItem.Clone() as SegmentationPolygon;

                SegmentLabelPolygon polygonItem = new SegmentLabelPolygon();
                polygonItem.X = 0;
                polygonItem.Y = 0;
                polygonItem.Width = this.Image.Width;
                polygonItem.Height = this.Image.Height;
                polygonItem.Color = this.TargetItem.Color;
                polygonItem.Name = this.TargetItem.Name;

                //polygonItem.X = 0;
                //polygonItem.Y = 0;
                //polygonItem.Width = this.Image.Width;
                //polygonItem.Height = this.Image.Height;

                this.SelectedItem = polygonItem;
                this.PolygonCollection.Add(polygonItem);
            }

            if(this.SelectedItem != null && Keyboard.IsKeyDown(Key.LeftAlt) == true)
            {
                this.SelectedItem.Points.Add(new Point()
                {
                    X = pressedPoint.X,
                    Y = pressedPoint.Y
                });

                PointCollection tempCollection = this.SelectedItem.Points;
                PointCollection newCollection = new PointCollection(tempCollection);
                this.SelectedItem.Points = newCollection;
            }
        }

        private void ChildCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            Point cursorPosition = e.GetPosition(this);
            Canvas canvas = sender as Canvas;
            if(canvas != null)
            {
                if ( canvas.IsMouseCaptured == true && canvas != null)
                {
                    Vector v = this.CanvasStart - cursorPosition;
                    this.TranslationX = CanvasOrigin.X - v.X;
                    this.TranslationY = CanvasOrigin.Y - v.Y;
                    return;
                }
            }
        }

        private void ChildCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Canvas canvas = sender as Canvas;
            canvas.ReleaseMouseCapture();
        }

        private void UserControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.SelectedItem = null;
        }
    }
}
