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
    public partial class BoxLabelControler : UserControl, INotifyPropertyChanged
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
            if (!Equals(_reference, _value))
            {
                _reference = _value;
                OnPropertyRaised(_name);
            }
        }

        public BoxLabelControler()
        {
            InitializeComponent();



            this.ZoomStep = 0.5;
            this.ZoomMax = 5;
            this.ZoomMin = 0.2;
            this.Zoom = 1;
            //this.PolygonCollection = null;
        }


        ~BoxLabelControler()
        {

        }


        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(BitmapImage), typeof(BoxLabelControler), new PropertyMetadata(OnCustomerChangedCallBack));
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
            BoxLabelControler control = sender as BoxLabelControler;
            if (control != null)
            {
                BitmapImage image = e.NewValue as BitmapImage;
                if (image == null) return;

                control.CanvasWidth = image.Width;
                control.CanvasHeight = image.Height;
                control.Zoom = (image.Width > image.Height ? (control.ActualWidth / image.Width) : (control.ActualHeight / image.Height));
                control.ZoomMax = control.Zoom * 20;
                control.ZoomMin = control.Zoom / 20;
                control.ZoomStep = (control.ZoomMax - control.ZoomMin) / 40;


                //control.TranslationX = 0;
                //control.TranslationY = 0;

                control.OutScrollViewer.UpdateLayout();

                //double horizontalOffset = (Math.Abs(control.ActualWidth - image.Width) * control.Zoom) / 2;
                //double verticalOffset = (Math.Abs(control.ActualHeight - image.Height) * control.Zoom) / 2;

                //control.OutScrollViewer.ScrollToHorizontalOffset(horizontalOffset);
                //control.OutScrollViewer.ScrollToVerticalOffset(verticalOffset);



                control.OutScrollViewer.ScrollToVerticalOffset(control.OutScrollViewer.ScrollableHeight / 2);
                control.OutScrollViewer.ScrollToHorizontalOffset(control.OutScrollViewer.ScrollableWidth / 2);
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


        public static readonly DependencyProperty ZoomProperty = DependencyProperty.Register("Zoom", typeof(double), typeof(BoxLabelControler));
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

        public static readonly DependencyProperty ZoomMaxProperty = DependencyProperty.Register("ZoomMax", typeof(double), typeof(BoxLabelControler));
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

        public static readonly DependencyProperty ZoomMinProperty = DependencyProperty.Register("ZoomMin", typeof(double), typeof(BoxLabelControler));
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

        public static readonly DependencyProperty ZoomStepProperty = DependencyProperty.Register("ZoomStep", typeof(double), typeof(BoxLabelControler));
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


        public static readonly DependencyProperty TranslationXProperty = DependencyProperty.Register("TranslationX", typeof(double), typeof(BoxLabelControler));
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


        public static readonly DependencyProperty TranslationYProperty = DependencyProperty.Register("TranslationY", typeof(double), typeof(BoxLabelControler));
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



        public static readonly DependencyProperty PolygonCollectionProperty = DependencyProperty.Register("BoxCollection", typeof(ObservableCollection<ClassificationLabelBox>), typeof(BoxLabelControler));
        public ObservableCollection<ClassificationLabelBox> BoxCollection
        {
            get
            {
                return (ObservableCollection<ClassificationLabelBox>)GetValue(PolygonCollectionProperty);
            }

            set
            {
                SetValue(PolygonCollectionProperty, value);
            }
        }


        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register("SelectedItem", typeof(ClassificationLabelBox), typeof(BoxLabelControler), new PropertyMetadata(OnSelectedItemChangedCallBack));
        public ClassificationLabelBox SelectedItem
        {
            get
            {
                return (ClassificationLabelBox)GetValue(SelectedItemProperty);
            }

            set
            {
                SetValue(SelectedItemProperty, value);
            }
        }
        private static void OnSelectedItemChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            BoxLabelControler control = sender as BoxLabelControler;
            if (control != null)
            {
                control.SelectedItem = e.NewValue as ClassificationLabelBox;
            }
        }


        public static readonly DependencyProperty TargetItemProperty = DependencyProperty.Register("TargetItem", typeof(ClassificationLabelBox), typeof(BoxLabelControler), new PropertyMetadata(OnTargetItemChangedCallBack));
        public ClassificationLabelBox TargetItem
        {
            get
            {
                return (ClassificationLabelBox)GetValue(TargetItemProperty);
            }

            set
            {
                SetValue(TargetItemProperty, value);
            }
        }

        private static void OnTargetItemChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            BoxLabelControler control = sender as BoxLabelControler;
            if (control != null)
            {
                control.SelectedItem = null;
            }
        }




        private Point CanvasStart;
        private Point CanvasOrigin;
        private bool IsRectSelected = false;
        private bool IsCanvasCaptured = false;
        private void ChildCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point pressedPoint = e.GetPosition(sender as IInputElement);

            HitTestResult hitTestResult = VisualTreeHelper.HitTest(sender as Visual, e.GetPosition(sender as IInputElement));
            if (hitTestResult == null) return;
            var element = hitTestResult.VisualHit;


            if (Keyboard.IsKeyDown(Key.LeftCtrl) == true && element.GetType() == typeof(Rectangle))
            {
                ClassificationLabelBox datacontext = (element as Rectangle).DataContext as ClassificationLabelBox;
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
                IsCanvasCaptured = true;
                return;
            }

            if (Keyboard.IsKeyDown(Key.LeftShift) == true && element.GetType() == typeof(Rectangle))
            {
                ClassificationLabelBox datacontext = (element as Rectangle).DataContext as ClassificationLabelBox;
                datacontext.IsSelected = true;

                this.IsRectSelected = true;
                this.SelectedItem = datacontext;

                var draggableControl = sender as Canvas;
                draggableControl.CaptureMouse();
                return;
            }

            if (this.SelectedItem == null && this.TargetItem != null && this.Image != null && Keyboard.IsKeyDown(Key.LeftAlt) == true)
            {

                ClassificationLabelBox boxItem = new ClassificationLabelBox();
                boxItem.X = pressedPoint.X;
                boxItem.Y = pressedPoint.Y;
                boxItem.Width = 10;
                boxItem.Height = 10;
                boxItem.Color = this.TargetItem.Color;
                boxItem.Name = this.TargetItem.Name;


                this.SelectedItem = boxItem;
                this.BoxCollection.Add(boxItem);
                var draggableControl = sender as Canvas;
                draggableControl.CaptureMouse();
            }

        }

        private void ChildCanvas_MouseMove(object sender, MouseEventArgs e)
        {


            Point cursorPosition = e.GetPosition(this);
            Canvas canvas = sender as Canvas;
            if (canvas != null)
            {
                if (canvas.IsMouseCaptured == true && canvas != null && Keyboard.IsKeyDown(Key.LeftShift) && this.IsRectSelected == false)
                {
                    Vector v = this.CanvasStart - cursorPosition;
                    this.TranslationX = CanvasOrigin.X - v.X;
                    this.TranslationY = CanvasOrigin.Y - v.Y;
                    return;
                }

                if (canvas.IsMouseCaptured == true && canvas != null && Keyboard.IsKeyDown(Key.LeftAlt) && this.SelectedItem != null)
                {

                    Point canvasCursorPosition = e.GetPosition(canvas);

                    this.SelectedItem.Width = canvasCursorPosition.X - this.SelectedItem.X;
                    this.SelectedItem.Height = canvasCursorPosition.Y - this.SelectedItem.Y;
                    System.Console.WriteLine("Box Test x: " + this.SelectedItem.X);
                    System.Console.WriteLine("Box Test y: " + this.SelectedItem.Y);
                    System.Console.WriteLine("Box Test Width: " + this.SelectedItem.Width);
                    System.Console.WriteLine("Box Test Height: " + this.SelectedItem.Height);
                    return;
                }

                if(this.IsRectSelected == true && Keyboard.IsKeyDown(Key.LeftShift))
                {

                    Point canvasCursorPosition = e.GetPosition(canvas);

                    this.SelectedItem.X = canvasCursorPosition.X - this.SelectedItem.Width / 2;
                    this.SelectedItem.Y = canvasCursorPosition.Y - this.SelectedItem.Height / 2;

                    return;
                }
            }
        }

        private void ChildCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Canvas canvas = sender as Canvas;
            canvas.ReleaseMouseCapture();
            this.SelectedItem = null;
            this.IsRectSelected = false;
            this.IsCanvasCaptured = false;
        }

        private void UserControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.SelectedItem = null;
            this.IsRectSelected = false;
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if(this.SelectedItem != null && Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.C))
            {
                ClassificationLabelBox label = new ClassificationLabelBox()
                {
                    X = this.SelectedItem.X + 10,
                    Y = this.SelectedItem.Y + 10,
                    Width = this.SelectedItem.Width,
                    Height = this.SelectedItem.Height,
                    Name = this.SelectedItem.Name,
                    Color = this.SelectedItem.Color,
                    IsSelected = true
                };
                this.SelectedItem.IsSelected = false;
                this.SelectedItem = label;
                this.BoxCollection.Add(this.SelectedItem);
            }
        }

        private void OutScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {

            e.Handled = true;

            //var draggableControl = sender as Canvas;
            if (this.IsMouseCaptured == true)
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
            OutScrollViewer.ScrollToVerticalOffset(OutScrollViewer.ScrollableHeight / 2);
            OutScrollViewer.ScrollToHorizontalOffset(OutScrollViewer.ScrollableWidth / 2);
        }
    }
}
