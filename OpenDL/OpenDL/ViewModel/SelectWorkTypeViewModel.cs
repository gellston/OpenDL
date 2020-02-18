using DevExpress.Utils.Design;
using DevExpress.Utils.Svg;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Native;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using OpenDL.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Text;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace OpenDL.ViewModel
{
    public class SelectWorkTypeViewModel : ViewModelBase
    {

        public SelectWorkTypeViewModel()
        {

            this.WorkTypeMenuCollection.Add(new WorkTypeMenu()
            {
                
                Icon = WpfSvgRenderer.CreateImageSource(SvgImageHelper.CreateImage(new Uri("pack://application:,,,/DevExpress.Images.v19.2;component/SvgImages/Dashboards/ChartFullStackedLine.svg")), 1d, null, null, true),
                Name = "Classification"
            });

            this.WorkTypeMenuCollection.Add(new WorkTypeMenu()
            {
                Icon = WpfSvgRenderer.CreateImageSource(SvgImageHelper.CreateImage(new Uri("pack://application:,,,/DevExpress.Images.v19.2;component/SvgImages/Dashboards/ChartBubble.svg")), 1d, null, null, true),
                Name = "Segmentation"
            });

            this.WorkTypeMenuCollection.Add(new WorkTypeMenu()
            {
                Icon = WpfSvgRenderer.CreateImageSource(SvgImageHelper.CreateImage(new Uri("pack://application:,,,/DevExpress.Images.v19.2;component/SvgImages/Dashboards/MergeCells.svg")), 1d, null, null, true),
                Name = "Object Detection"
            });

        }


        private ObservableCollection<WorkTypeMenu> _WorkTypeMenuCollection = null;
        public ObservableCollection<WorkTypeMenu> WorkTypeMenuCollection
        {

            get
            {
                if(_WorkTypeMenuCollection == null)
                {
                    _WorkTypeMenuCollection = new ObservableCollection<WorkTypeMenu>();
                }

                return _WorkTypeMenuCollection;
            }
        }

        private ICommand _SelectWorkTypeCommand = null;
        public ICommand SelectWorkTypeCommand
        {
            get
            {
                if(_SelectWorkTypeCommand == null)
                {
                    _SelectWorkTypeCommand = new RelayCommand<WorkTypeMenu>((workType) =>
                    {
                        SelectWorkTypeMessage message = new SelectWorkTypeMessage()
                        {
                            Type = workType,
                            Message = "Select"
                        };
                        Messenger.Default.Send<SelectWorkTypeMessage>(message);
                    });
                }

                return _SelectWorkTypeCommand;
            }
        }



        private ICommand _ClearWorkTypeCommand = null;
        public ICommand ClearWorkTypeCommand
        {
            get
            {
                if(_ClearWorkTypeCommand == null)
                {
                    _ClearWorkTypeCommand = new RelayCommand(() =>
                    {

                        SelectWorkTypeMessage message = new SelectWorkTypeMessage()
                        {
                            Message = "Clear"
                        };
                        Messenger.Default.Send<SelectWorkTypeMessage>(message);
                    });
                }

                return _ClearWorkTypeCommand;
            }
        }


        private ICommand _CancelWorkTypeCommand = null;
        public ICommand CancelWorkTypeCommand
        {
            get
            {
                if(_CancelWorkTypeCommand == null)
                {
                    _CancelWorkTypeCommand = new RelayCommand(() =>
                    {
                        SelectWorkTypeMessage message = new SelectWorkTypeMessage()
                        {
                            Message = "Cancel"
                        };
                        Messenger.Default.Send<SelectWorkTypeMessage>(message);
                    });
                }

                return _CancelWorkTypeCommand;
            }
        }
    }
}
