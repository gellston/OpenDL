using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using OpenDL.Model;
using OpenDL.Service;

namespace OpenDL.ViewModel
{
    public class ClassificationAugmentViewModel : ViewModelBase
    {

        readonly FileBrowserService folderBrowserService;
        readonly LabelingService labelLoaderService;
        readonly AugmentService segmentationService;

        private bool IsAugmentating = false;

        public ClassificationAugmentViewModel(FileBrowserService _folderBrowserService,
                                            LabelingService _labelLoaderService,
                                            AugmentService _segmentationService)
        {
            this.folderBrowserService = _folderBrowserService;
            this.labelLoaderService = _labelLoaderService;
            this.segmentationService = _segmentationService;
        }

        private string _TargetLabelPath = "";
        public string TargetLabelPath
        {
            get => _TargetLabelPath;
            set => Set<string>(nameof(TargetLabelPath), ref _TargetLabelPath, value);
        }


        private string _OutputAugmentationPath = "";
        public string OutputAugmentationPath
        {
            get => _OutputAugmentationPath;
            set => Set<string>(nameof(OutputAugmentationPath), ref _OutputAugmentationPath, value);
        }

        private string _CurrentStatusMessage = "";
        public string CurrentStatusMessage
        {
            get => _CurrentStatusMessage;
            set => Set<string>(nameof(CurrentStatusMessage), ref _CurrentStatusMessage, value);
        }

        private ICommand _OpenTargetLabelPathCommand = null;
        public ICommand OpenTargetLabelPathCommand
        {
            get
            {
                if (_OpenTargetLabelPathCommand == null)
                {
                    _OpenTargetLabelPathCommand = new RelayCommand(() =>
                    {
                        this.TargetLabelPath = this.folderBrowserService.SelectFolder();
                    });
                }

                return _OpenTargetLabelPathCommand;
            }
        }


        private ICommand _OpenOutputAugmentationPathCommand = null;
        public ICommand OpenOutputAugmentationPathCommand
        {
            get
            {
                if (_OpenOutputAugmentationPathCommand == null)
                {
                    _OpenOutputAugmentationPathCommand = new RelayCommand(() =>
                    {
                        this.OutputAugmentationPath = this.folderBrowserService.SelectFolder();
                    });
                }

                return _OpenOutputAugmentationPathCommand;
            }
        }


        private ICommand _DoAugmentationCommand = null;
        public ICommand DoAugmentationCommand
        {
            get
            {
                if (_DoAugmentationCommand == null)
                {
                    _DoAugmentationCommand = new RelayCommand(async () =>
                    {

                        if (this.IsAugmentating == true) return;

                        this.IsAugmentating = true;

                        this.CurrentStatusMessage = "준비";

                        string[] files = folderBrowserService.ImageListFromFolder(this.TargetLabelPath);
                        var labelResult = await labelLoaderService.LoadClassLabelAsync(TargetLabelPath, files);
                        ObservableCollection<ClassificationLabelBox> boxes = labelResult.Item1;
                        ObservableCollection<ClassLabelUnit> labelUnits = labelResult.Item2;


                        int imageWidth = ((IntProperty)this.PropertyCollection.Where(x => x.Name == "IMAGE WIDTH").FirstOrDefault()).Value;
                        int imageHeight = ((IntProperty)this.PropertyCollection.Where(x => x.Name == "IMAGE HEIGHT").FirstOrDefault()).Value;
                        bool grayScale = ((BoolProperty)this.PropertyCollection.Where(x => x.Name == "GRAY SCALE").FirstOrDefault()).Value;

                        await this.segmentationService.DoClassificationAugmentationAsync(this.OutputAugmentationPath, boxes, labelUnits, imageWidth, imageHeight, grayScale,
                                                                                       (currentCount, maxCount) =>
                                                                                       {
                                                                                           Application.Current.Dispatcher.Invoke(() =>
                                                                                           {
                                                                                               this.CurrentMaxCount = maxCount;
                                                                                               this.CurrentProcessedCount = currentCount;
                                                                                               this.CurrentStatusMessage = string.Format("{0} / {1}", CurrentProcessedCount, CurrentMaxCount);
                                                                                           });
                                                                                       });
                        this.IsAugmentating = false;
                        this.CurrentStatusMessage = "완료";
                    });
                }

                return _DoAugmentationCommand;
            }
        }


        private ICommand _CancelCommand = null;
        public ICommand CancelCommand
        {
            get
            {
                if (_CancelCommand == null)
                {
                    _CancelCommand = new RelayCommand(() =>
                    {
                        Messenger.Default.Send<AugmentationMessage>(new AugmentationMessage()
                        {
                            Message = "Cancel"
                        });
                    });
                }

                return _CancelCommand;
            }
        }

        private int _CurrentProcessedCount = 0;
        public int CurrentProcessedCount
        {
            get => _CurrentProcessedCount;
            set => Set<int>(nameof(CurrentProcessedCount), ref _CurrentProcessedCount, value);
        }

        private int _CurrentMaxCount = 0;
        public int CurrentMaxCount
        {
            get => _CurrentMaxCount;
            set => Set<int>(nameof(CurrentMaxCount), ref _CurrentMaxCount, value);
        }

        private ObservableCollection<BaseProperty> _PropertyCollection = null;
        public ObservableCollection<BaseProperty> PropertyCollection
        {
            get
            {
                if (_PropertyCollection == null)
                {
                    _PropertyCollection = new ObservableCollection<BaseProperty>();

                    this.PropertyCollection.Add(new IntProperty() { Name = "IMAGE WIDTH", Value = 100 });
                    this.PropertyCollection.Add(new IntProperty() { Name = "IMAGE HEIGHT", Value = 100 });
                    this.PropertyCollection.Add(new BoolProperty() { Name = "GRAY SCALE", Value = true });
                }
                return _PropertyCollection;
            }
        }
    }
}
