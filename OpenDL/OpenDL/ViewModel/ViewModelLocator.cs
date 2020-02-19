using System;
using System.Collections.Generic;
using System.Text;
using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using OpenDL.Service;

namespace OpenDL.ViewModel
{
    public class ViewModelLocator
    {

        public ViewModelLocator()
        {

            if (ViewModelBase.IsInDesignModeStatic == true)
            {
                SimpleIoc.Default.Register<DummyMainViewModel>();
                SimpleIoc.Default.Register<DummyLabelViewModel>();
                SimpleIoc.Default.Register<ReleaseNoteViewModel>();
                SimpleIoc.Default.Register<SelectWorkTypeViewModel>();
            }
            else
            {
                SimpleIoc.Default.Register<FolderBrowserService>();
                SimpleIoc.Default.Register<MainViewModel>();
                SimpleIoc.Default.Register<LabelViewModel>();
                SimpleIoc.Default.Register<ReleaseNoteViewModel>();
                SimpleIoc.Default.Register<SelectWorkTypeViewModel>();
                SimpleIoc.Default.Register<SegmentationLabelViewModel>();



                //Pre Initialization

                SimpleIoc.Default.GetInstance<LabelViewModel>();
                SimpleIoc.Default.GetInstance<SegmentationLabelViewModel>();
            }

        }

        public ViewModelBase MainViewModel
        {
            get
            {
                if (ViewModelBase.IsInDesignModeStatic == true)
                {
                    return SimpleIoc.Default.GetInstance<DummyMainViewModel>();
                }
                else
                {
                    return SimpleIoc.Default.GetInstance<MainViewModel>();
                }

            }
        }
    }
}
