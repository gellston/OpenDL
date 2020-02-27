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
            // Service Registeration
            SimpleIoc.Default.Register<ConfigureService>();
            SimpleIoc.Default.Register<LabelingService>();
            SimpleIoc.Default.Register<FolderBrowserService>();
            SimpleIoc.Default.Register<AugmentService>();
            SimpleIoc.Default.Register<TrainingService>();


            // Most Top ViewModel Registeration
            SimpleIoc.Default.Register<MainViewModel>();
            SimpleIoc.Default.Register<LabelViewModel>();
            SimpleIoc.Default.Register<TrainViewModel>();
            SimpleIoc.Default.Register<ReleaseNoteViewModel>();
            SimpleIoc.Default.Register<SelectWorkTypeViewModel>();
            SimpleIoc.Default.Register<ModelManagementViewModel>();


            // Segmentation ViewModel Registeration
            SimpleIoc.Default.Register<SegmentationLabelViewModel>();
            SimpleIoc.Default.Register<SegmentationAugmentViewModel>();
            SimpleIoc.Default.Register<SegmentationTrainViewModel>();


            // Classification ViewModel Registeration
            SimpleIoc.Default.Register<ClassificationLabelViewModel>();
            SimpleIoc.Default.Register<ClassificationAugmentViewModel>();
            SimpleIoc.Default.Register<ClassificationTrainViewModel>();



            //Pre Initialization of ViewModel
            SimpleIoc.Default.GetInstance<ConfigureService>();
            SimpleIoc.Default.GetInstance<LabelViewModel>();
            SimpleIoc.Default.GetInstance<TrainViewModel>();
            SimpleIoc.Default.GetInstance<SegmentationTrainViewModel>();
            SimpleIoc.Default.GetInstance<SegmentationLabelViewModel>();
            SimpleIoc.Default.GetInstance<ClassificationTrainViewModel>();
            SimpleIoc.Default.GetInstance<ClassificationLabelViewModel>();

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
