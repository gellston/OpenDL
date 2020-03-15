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
            SimpleIoc.Default.Register<FileBrowserService>();
            SimpleIoc.Default.Register<AugmentService>();
            SimpleIoc.Default.Register<TrainingService>();
            SimpleIoc.Default.Register<DialogService>();
            SimpleIoc.Default.Register<ModelExporterService>();


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
            SimpleIoc.Default.Register<SegmentationModelManagementViewModel>();


            // Classification ViewModel Registeration
            SimpleIoc.Default.Register<ClassificationLabelViewModel>();
            SimpleIoc.Default.Register<ClassificationAugmentViewModel>();
            SimpleIoc.Default.Register<ClassificationTrainViewModel>();
            SimpleIoc.Default.Register<ClassificationModelManagementViewModel>();



            // Anomaly Detection ViewModel
            SimpleIoc.Default.Register<AnomalyLabelViewModel>();
            SimpleIoc.Default.Register<AnomalyAugmentViewModel>();
            SimpleIoc.Default.Register<AnomalyTrainViewModel>();



            //Pre Initialization of ViewModel
            SimpleIoc.Default.GetInstance<ConfigureService>();
            SimpleIoc.Default.GetInstance<LabelViewModel>();
            SimpleIoc.Default.GetInstance<TrainViewModel>();
            SimpleIoc.Default.GetInstance<ModelManagementViewModel>();


            SimpleIoc.Default.GetInstance<SegmentationTrainViewModel>();
            SimpleIoc.Default.GetInstance<SegmentationLabelViewModel>();
            SimpleIoc.Default.GetInstance<SegmentationModelManagementViewModel>();

            SimpleIoc.Default.GetInstance<ClassificationTrainViewModel>();
            SimpleIoc.Default.GetInstance<ClassificationLabelViewModel>();
            SimpleIoc.Default.GetInstance<ClassificationModelManagementViewModel>();

            SimpleIoc.Default.GetInstance<AnomalyLabelViewModel>();
            SimpleIoc.Default.GetInstance<AnomalyTrainViewModel>();


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
