using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using OpenDL.Model;
using OpenDL.Service;

namespace OpenDL.ViewModel
{
    public class ModelManagementViewModel : ViewModelBase
    {
        readonly DialogService dialogService;
        readonly FileBrowserService fileBrowserService;
        readonly ConfigureService configureService;
        readonly ModelExporterService modelExportService;
        
        public ModelManagementViewModel(DialogService _dialogService,
                                        FileBrowserService _fileBrowserService,
                                        ConfigureService _configureService,
                                        ModelExporterService _modelExportService)
        {
            this.dialogService = _dialogService;
            this.fileBrowserService = _fileBrowserService;
            this.configureService = _configureService;
            this.modelExportService = _modelExportService;


        }

        private ICommand _RefreshPureModelCommand = null;
        public ICommand RefreshPureModelCommand
        {
            get
            {
                if(_RefreshPureModelCommand == null)
                {
                    _RefreshPureModelCommand = new RelayCommand(() =>
                    {
                        string[] files = Directory.GetFiles(this.configureService.SegmentationPureModelContainerPath, "*.model");
                        this.PureDeepModelCollection.Clear();
                        foreach(var file in files)
                        {
                            DeepModelPath model = new DeepModelPath()
                            {
                                ModelName = Path.GetFileName(file),
                                FullPath = file
                            };
                            this.PureDeepModelCollection.Add(model);
                        }
                    });
                }

                return _RefreshPureModelCommand;
            }
        }


        private ICommand _RefreshTrainedModelCommand = null;
        public ICommand RefreshTrainedModelCommand
        {
            get
            {
                if (_RefreshTrainedModelCommand == null)
                {
                    _RefreshTrainedModelCommand = new RelayCommand(() =>
                    {
                        string[] files = Directory.GetFiles(this.configureService.SegmentationTrainedModelContainerPath, "*.model");
                        this.TrainedDeepModelCollection.Clear();
                        foreach (var file in files)
                        {
                            DeepModelPath model = new DeepModelPath()
                            {
                                ModelName = Path.GetFileName(file),
                                FullPath = file
                            };
                            this.TrainedDeepModelCollection.Add(model);
                        }
                    });
                }

                return _RefreshTrainedModelCommand;
            }
        }


        private ICommand _UnzipModelCommand = null;
        public ICommand UnzipModelCommand
        {
            get
            {
                if(_UnzipModelCommand == null)
                {
                    _UnzipModelCommand = new RelayCommand(() =>
                    {
                        if(this.SelectedTrainModel == null)
                        {
                            this.dialogService.ShowErrorMessage("모델이 선택되지 않았습니다. ");
                            return;
                        }

                        this.modelExportService.DeleteFreezeUnzipFiles();
                        this.modelExportService.UnZipSegmentDeepModel(this.SelectedTrainModel.FullPath, this.configureService.SecurityPassword);
                        //this.modelExportService

                    });
                }

                return _UnzipModelCommand;
            }
        }

        private ICommand _ExportModelCommand = null;
        public ICommand ExportModelCommand
        {
            get
            {
                if(_ExportModelCommand == null)
                {
                    _ExportModelCommand = new RelayCommand(() =>
                    {
                        string fileName = this.fileBrowserService.SaveOneFile("Zip File (.zip)|*.zip");
                        if (fileName.Length <= 0)
                            return;

                        
                    });
                }

                return _ExportModelCommand;
            }
        }


        private ObservableCollection<DeepModelPath> _PureDeepModelCollection = null;
        public ObservableCollection<DeepModelPath> PureDeepModelCollection
        {
            get
            {
                if(_PureDeepModelCollection == null)
                {
                    _PureDeepModelCollection = new ObservableCollection<DeepModelPath>();
                }

                return _PureDeepModelCollection;
            }
        }


        private DeepModelPath _SelectedTrainedModel = null;
        public DeepModelPath SelectedTrainModel
        {
            get => _SelectedTrainedModel;
            set => Set<DeepModelPath>(nameof(SelectedTrainModel), ref _SelectedTrainedModel, value);
        }


        private ObservableCollection<DeepModelPath> _TrainedDeepModelCollection = null;
        public ObservableCollection<DeepModelPath> TrainedDeepModelCollection
        {
            get
            {
                if (_TrainedDeepModelCollection == null)
                {
                    _TrainedDeepModelCollection = new ObservableCollection<DeepModelPath>();
                }

                return _TrainedDeepModelCollection;
            }
        }

    }
}
