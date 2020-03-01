using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Input;
using System.Xml.Serialization;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
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
        readonly TrainingService trainService;
        
        public ModelManagementViewModel(DialogService _dialogService,
                                        FileBrowserService _fileBrowserService,
                                        ConfigureService _configureService,
                                        ModelExporterService _modelExportService,
                                        TrainingService _trainService)
        {
            this.dialogService = _dialogService;
            this.fileBrowserService = _fileBrowserService;
            this.configureService = _configureService;
            this.modelExportService = _modelExportService;
            this.trainService = _trainService;



            Messenger.Default.Register<TrainMessage>(this, this.ReciveTrianMessage);


            this.RefreshTrainedModelCommand.Execute(null);
            this.RefreshPureModelCommand.Execute(null);
        }

        private void ReciveTrianMessage(TrainMessage message)
        {
            switch (message.Message)
            {
                case "Complete":
                    RefreshTrainedModelCommand.Execute(null);
                    break;
            }
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

                        this.ModelInfoContent = "";

                        this.modelExportService.DeleteFreezeUnzipFiles();
                        if (this.modelExportService.UnZipSegmentDeepModel(this.SelectedTrainModel.FullPath, this.configureService.SecurityPassword) == false)
                        {
                            this.dialogService.ShowErrorMessage("프리징 모델 정보 추출에 실패 했습니다.");
                            return;
                        }

                        this.PackageContentCollection.Clear();
                        string[] files = Directory.GetFiles(this.configureService.FreezeUnzipPath);
                        foreach(var file in files)
                        {
                            this.PackageContentCollection.Add(new ModelFile()
                            {
                                Name = Path.GetFileName(file),
                                Path = file
                            });
                        }

                        string modelInfoFile = this.configureService.FreezeUnzipPath + Path.DirectorySeparatorChar + this.configureService.ModelInfoFileName;
                        this.ModelInfoContent = File.ReadAllText(modelInfoFile);
                        
        

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
                        {
                            this.dialogService.ShowErrorMessage("저장할 파일이 설정되지 않았습니다.");
                            return;
                        }

                        if (this.PackageContentCollection.Count <= 0)
                        {
                            this.dialogService.ShowErrorMessage("프리징될 모델이 선택되지 않았습니다.");
                            return;
                        }

                        try
                        {
                            this.modelExportService.DeletePackageZipFiles();

                            string freezedModelFile = this.configureService.PackageZipPath + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(fileName) + ".frz";
                            string modelInfoFile = this.configureService.PackageZipPath + Path.DirectorySeparatorChar + this.configureService.FreezeModelInfoFileName;
                            string outputZipModelFile = Path.GetDirectoryName(fileName) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(fileName) + ".zip";


                            SegmentTrainModelInfo modelInfo = this.trainService.LoadSegmentTrainModelInfo(this.configureService.FreezeUnzipPath +
                                                                                    Path.DirectorySeparatorChar +
                                                                                    this.configureService.ModelInfoFileName);
                            string checkFilePath = this.configureService.FreezeUnzipPath + Path.DirectorySeparatorChar + modelInfo.CheckFile;

                            string metaFilePath = this.configureService.FreezeUnzipPath + Path.DirectorySeparatorChar + modelInfo.MetaFile;

                            string[] outputNodeNames = new string[3];
                            outputNodeNames[0] = modelInfo.InputNodeName.Replace(":0", "");
                            outputNodeNames[1] = modelInfo.OutputNodeName.Replace(":0", "");
                            outputNodeNames[2] = modelInfo.PhaseNodeName.Replace(":0", "");

                            if (this.trainService.Freeze_Graph(metaFilePath, checkFilePath, freezedModelFile, outputNodeNames) == false)
                            {
                                this.dialogService.ShowErrorMessage("모델 프리징에 실패했습니다.");
                                return;
                            };

                            if (this.IsGpuPackage == true)
                            {
                                File.Copy(this.configureService.TensorflowPackagePath + Path.DirectorySeparatorChar + this.configureService.TensorflowGpuDllName,
                                          this.configureService.PackageZipPath + Path.DirectorySeparatorChar + this.configureService.TensorflowDllName);
                            }
                            else
                            {
                                File.Copy(this.configureService.TensorflowPackagePath + Path.DirectorySeparatorChar + this.configureService.TensorflowCpuDllName,
                                          this.configureService.PackageZipPath + Path.DirectorySeparatorChar + this.configureService.TensorflowDllName, true);
                            }

                            FreezedSegmentModelInfo freezeInfo = new FreezedSegmentModelInfo()
                            {
                                Width = modelInfo.Width,
                                Height = modelInfo.Height,
                                IsGray = modelInfo.IsGray,
                                FreezeInputNodeName = outputNodeNames[0],
                                FreezeOutNodeName = outputNodeNames[1],
                                FreezePhaseNodeName = outputNodeNames[2],
                                LabelCount = modelInfo.MaxLabelCount
                            };
                            //파일에 출력하는 예
                            using (StreamWriter wr = new StreamWriter(modelInfoFile))
                            {
                                XmlSerializer xs = new XmlSerializer(typeof(FreezedSegmentModelInfo));
                                xs.Serialize(wr, freezeInfo);
                            }

                            if (this.modelExportService.PackageZipFreezeModel(outputZipModelFile) == false)
                            {
                                this.dialogService.ShowErrorMessage("프리징 모델 압축에 실패했습니다.");
                                return;
                            }

                            this.modelExportService.DeletePackageZipFiles();
                        }
                        catch(Exception e)
                        {
                            this.dialogService.ShowErrorMessage(e.ToString());
                        }

                        
                    });
                }

                return _ExportModelCommand;
            }
        }

        private ICommand _GpuPackageCommand = null;
        public ICommand GpuPackageCommand
        {
            get
            {
                if(_GpuPackageCommand == null)
                {
                    _GpuPackageCommand = new RelayCommand(() =>
                    {
                        this.IsGpuPackage = true;
                    });
                }

                return _GpuPackageCommand;
            }
        }


        private ICommand _CpuPackageCommand = null;
        public ICommand CpuPackageCommand
        {
            get
            {
                if(_CpuPackageCommand == null)
                {
                    _CpuPackageCommand = new RelayCommand(() =>
                    {
                        this.IsGpuPackage = false;
                    });
                }

                return _CpuPackageCommand;
            }
        }

        private bool _IsGpuPackage = false;
        public bool IsGpuPackage
        {
            get => _IsGpuPackage;
            set => Set<bool>(nameof(IsGpuPackage), ref _IsGpuPackage, value);
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


        private ObservableCollection<ModelFile> _PackageContentCollection = null;
        public ObservableCollection<ModelFile> PackageContentCollection
        {
            get
            {
                if(_PackageContentCollection == null)
                {
                    _PackageContentCollection = new ObservableCollection<ModelFile>();
                }

                return _PackageContentCollection;
            }
        }


        private string _ModelInfoContent = "";
        public string ModelInfoContent
        {
            get => _ModelInfoContent;
            set => Set<string>(nameof(ModelInfoContent), ref _ModelInfoContent, value);
        }

    }
}
