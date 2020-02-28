using OpenDL.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
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
    /// SegmentationPreviewer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class SegmentationPreviewer : UserControl
    {
        public SegmentationPreviewer()
        {
            InitializeComponent();
            
        }

        public static readonly DependencyProperty ImagesProperty = DependencyProperty.Register("Images", typeof(ObservableCollection<SegmentPreviewItem>), typeof(SegmentationPreviewer), new PropertyMetadata(OnCustomerChangedCallBack));
        public ObservableCollection<SegmentPreviewItem> Images
        {
            get
            {
                return (ObservableCollection<SegmentPreviewItem>)GetValue(ImagesProperty);
            }

            set
            {
                SetValue(ImagesProperty, value);
            }
        }

        private static void OnCustomerChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            SegmentationPreviewer control = sender as SegmentationPreviewer;
            if (control != null)
            {
                ObservableCollection<SegmentPreviewItem> collection = e.NewValue as ObservableCollection<SegmentPreviewItem>;
                control.Images = collection;


            }
        }

    }
}
