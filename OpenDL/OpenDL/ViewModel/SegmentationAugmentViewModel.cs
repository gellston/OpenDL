using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using OpenDL.Model;
using OpenDL.Service;

namespace OpenDL.ViewModel
{
    public class SegmentationAugmentViewModel : ViewModelBase
    {

        readonly FolderBrowserService folderBrowserService;
        readonly LabelLoaderService labelLoaderService;
        readonly SegmentationService segmentationService;

        public SegmentationAugmentViewModel(FolderBrowserService _folderBrowserService,
                                            LabelLoaderService _labelLoaderService,
                                            SegmentationService _segmentationService)
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


        private ICommand _OpenTargetLabelPathCommand = null;
        public ICommand OpenTargetLabelPathCommand
        {
            get
            {
                if(_OpenTargetLabelPathCommand == null)
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
                if(_OpenOutputAugmentationPathCommand == null)
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
                        string[] files = folderBrowserService.ImageListFromFolder(this.TargetLabelPath);
                        var labelResult = await labelLoaderService.LoadSegmentedLabelAsync(TargetLabelPath, files);
                        ObservableCollection<SegmentLabelPolygon> polygons = labelResult.Item1;
                        ObservableCollection<SegmentLabelUnit> labelUnits = labelResult.Item2;

                        this.segmentationService.DoSegmentationAugmentation(this.OutputAugmentationPath, polygons, labelUnits, 0, 0);
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
                if(_CancelCommand == null)
                {
                    _CancelCommand = new RelayCommand(() =>
                    {
                        Messenger.Default.Send<SegmentAugmentationMessage>(new SegmentAugmentationMessage()
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
    }
}
