using OpenDL.Model;
using System;
using System.Collections.Generic;
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
    /// ClassificationPreviewer.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ClassificationPreviewer : UserControl, INotifyPropertyChanged
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


        public ClassificationPreviewer()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty PreviewItemProperty = DependencyProperty.Register("PreviewItem", typeof(ClassPreviewItem), typeof(ClassificationPreviewer), new PropertyMetadata(OnCustomerChangedCallBack));
        public ClassPreviewItem PreviewItem
        {
            get
            {
                return (ClassPreviewItem)GetValue(PreviewItemProperty);
            }

            set
            {
                SetValue(PreviewItemProperty, value);
            }
        }

        private static void OnCustomerChangedCallBack(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ClassificationPreviewer control = sender as ClassificationPreviewer;
            if (control != null)
            {
                ClassPreviewItem item = e.NewValue as ClassPreviewItem;
                control.PreviewItem = item;
                control.OnPropertyRaised("PreviewItem");
            }
        }

    }
}
