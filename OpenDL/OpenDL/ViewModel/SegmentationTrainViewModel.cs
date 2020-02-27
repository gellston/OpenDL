using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using OpenDL.Model;
using OpenDL.Service;

namespace OpenDL.ViewModel
{
    public class SegmentationTrainViewModel : ViewModelBase
    {

        private Thread TrainTaskThread;
        private bool IsTraining = true;
        private int CurrentStep = 0;

        private ManualResetEventSlim TrainTaskSignal = new ManualResetEventSlim(false);

        private readonly FolderBrowserService folderBrowserService;
        private readonly TrainingService trainSampleLoaderService;
        private readonly ConfigureService configureService;

        public SegmentationTrainViewModel(FolderBrowserService _folderBrowserService,
                                          TrainingService _trainSampleLoaderService,
                                          ConfigureService _configureServie)
        {

            this.folderBrowserService = _folderBrowserService;
            this.trainSampleLoaderService = _trainSampleLoaderService;
            this.configureService = _configureServie;

            this.TrainCostCollection = new ObservableCollection<LinePlotInfo>();
            this.ValidationCostCollection = new ObservableCollection<LinePlotInfo>();
            this.TrainAccuracyCollection = new ObservableCollection<LinePlotInfo>();
            this.ValidationAccuracyCollection = new ObservableCollection<LinePlotInfo>();
            this.PureSegmentModelCollection = new ObservableCollection<DeepModelPath>();

            this.TrainSampleCollection = new ObservableCollection<SegmentTrainSample>();


            this.TrainTaskThread = new Thread(new ThreadStart(TrainTaskStart));
            this.TrainTaskThread.IsBackground = true;
            this.TrainTaskThread.Start();


            // update 모델
            this.RefreshPureModelCommand.Execute(null);
        }


        ~SegmentationTrainViewModel()
        {
            this.IsTraining = false;
            this.TrainTaskSignal.Set();
            this.TrainTaskThread.Join(5000);
        }

        private void TrainTaskStart()
        {
            Random random = new Random();
            CurrentStep = 0;
            while (this.IsTraining == true)
            {
                this.TrainTaskSignal.Wait();
                if (this.IsTraining == false) break;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    double accuracy = random.NextDouble();
                    double loss = random.NextDouble();
                    this.TrainCostCollection.Add(new LinePlotInfo()
                    {
                        Step = CurrentStep,
                        Value = accuracy
                    });

                    this.ValidationCostCollection.Add(new LinePlotInfo()
                    {
                        Step = CurrentStep,
                        Value = loss
                    });

                    this.CurrentAccuracy = accuracy;
                    this.CurrentLoss = loss;
                });

                CurrentStep++;
                Thread.Sleep(10);
            }
        }

        

        private ICommand _TrainStartCommand = null;
        public ICommand TrainStartCommand
        {
            get
            {
                if(_TrainStartCommand == null)
                {
                    _TrainStartCommand = new RelayCommand(() =>
                    {

                        if(this.TrainTaskSignal.IsSet == false)
                        {
                            this.TrainTaskSignal.Set();
                        }
                            
                    });
                }

                return _TrainStartCommand;
            }
        }


        private ICommand _TrainStopCommand = null;
        public ICommand TrainStopCommand
        {
            get
            {
                if(_TrainStopCommand == null)
                {
                    _TrainStopCommand = new RelayCommand(() =>
                    {

                        this.TrainTaskSignal.Reset();
                        this.TrainCostCollection.Clear();
                        this.ValidationCostCollection.Clear();
                        this.TrainAccuracyCollection.Clear();
                        this.ValidationAccuracyCollection.Clear();
                        this.CurrentStep = 0;
                    });
                }

                return _TrainStopCommand;
            }
        }


        private ICommand _TrainPauseCommand = null;
        public ICommand TrainPauseCommand
        {
            get
            {
                if(_TrainPauseCommand == null)
                {
                    _TrainPauseCommand = new RelayCommand(() =>
                    {
                        this.TrainTaskSignal.Reset();
                    });
                }

                return _TrainPauseCommand;
            }
        }

        private ICommand _OpenAugmentedLabelCommand = null;
        public ICommand OpenAugmentedLabelCommand
        {
            get
            {
                if(_OpenAugmentedLabelCommand == null)
                {
                    _OpenAugmentedLabelCommand = new RelayCommand(async () =>
                    {
                        CurrentOpenedLabelDirectory = folderBrowserService.SelectFolder();
                        this.TrainSampleCollection = await this.trainSampleLoaderService.LoadSegmentSamplesAsync(CurrentOpenedLabelDirectory);
                    });
                }
                return _OpenAugmentedLabelCommand;
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
                        string path = configureService.SegmentationPureModelContainerPath;
                        string [] models = Directory.GetFiles(path);
                        foreach(var model in models)
                        {
                            this.PureSegmentModelCollection.Add(new DeepModelPath()
                            {
                                FullPath = model,
                                ModelName = Path.GetFileName(model)
                            });
                        }
                    });
                }

                return _RefreshPureModelCommand;
            }
        }

        private string _CurrentOpenedLabelDirectory = "";
        public string CurrentOpenedLabelDirectory
        {
            get => _CurrentOpenedLabelDirectory;
            set => Set<string>(nameof(CurrentOpenedLabelDirectory), ref _CurrentOpenedLabelDirectory, value);
        }

        private string _CurrentBatchStep = "0 / 0";
        public string CurrentBatchStep
        {
            get => _CurrentBatchStep;
            set => Set<string>(nameof(CurrentBatchStep), ref _CurrentBatchStep, value);
        }


        private double _CurrentAccuracy = 0;
        public double CurrentAccuracy
        {
            set => Set<double>(nameof(CurrentAccuracy), ref _CurrentAccuracy, value);
            get => _CurrentAccuracy;
        }

        private double _CurrentLoss = 0;
        public double CurrentLoss
        {
            set => Set<double>(nameof(CurrentLoss), ref _CurrentLoss, value);
            get => _CurrentLoss;
        }


        private ObservableCollection<LinePlotInfo> _TrainCostCollection = null;
        public ObservableCollection<LinePlotInfo> TrainCostCollection
        {
            get => _TrainCostCollection;
            set => Set<ObservableCollection<LinePlotInfo>>(nameof(TrainCostCollection), ref _TrainCostCollection, value);
        }

        private ObservableCollection<LinePlotInfo> _ValidationCostCollection = null;
        public ObservableCollection<LinePlotInfo> ValidationCostCollection
        {
            get => _ValidationCostCollection;
            set => Set<ObservableCollection<LinePlotInfo>>(nameof(ValidationCostCollection), ref _ValidationCostCollection, value);
        }

        private ObservableCollection<LinePlotInfo> _TrainAccuracyCollection = null;
        public ObservableCollection<LinePlotInfo> TrainAccuracyCollection
        {
            get => _TrainAccuracyCollection;
            set => Set<ObservableCollection<LinePlotInfo>>(nameof(TrainAccuracyCollection), ref _TrainAccuracyCollection, value);
        }

        private ObservableCollection<LinePlotInfo> _ValidationAccuracyCollection = null;
        public ObservableCollection<LinePlotInfo> ValidationAccuracyCollection
        {
            get => _ValidationAccuracyCollection;
            set => Set<ObservableCollection<LinePlotInfo>>(nameof(ValidationAccuracyCollection), ref _ValidationAccuracyCollection, value);
        }

        private ObservableCollection<SegmentTrainSample> _TrainSamplesCollection = null;
        public ObservableCollection<SegmentTrainSample> TrainSampleCollection
        {
            get => _TrainSamplesCollection;
            set => Set<ObservableCollection<SegmentTrainSample>>(nameof(TrainSampleCollection), ref _TrainSamplesCollection, value);
        }


        private ObservableCollection<DeepModelPath> _PureSegmentModelCollection = null;
        public ObservableCollection<DeepModelPath> PureSegmentModelCollection
        {
            get => _PureSegmentModelCollection;
            set => Set<ObservableCollection<DeepModelPath>>(nameof(PureSegmentModelCollection), ref _PureSegmentModelCollection, value);
        }


        private DeepModelPath _SelectedSegmentModel = null;
        public DeepModelPath SelectedSegmentModel
        {
            get => _SelectedSegmentModel;
            set => Set<DeepModelPath>(nameof(SelectedSegmentModel), ref _SelectedSegmentModel, value);
        }




        private ObservableCollection<BaseProperty> _PropertyCollection = null;
        public ObservableCollection<BaseProperty> PropertyCollection
        {
            get
            {
                if(_PropertyCollection == null)
                {
                    _PropertyCollection = new ObservableCollection<BaseProperty>();

                    this.PropertyCollection.Add(new StringProperty() { Name = "MODEL NAME", Value = "AnonymousModel" });
                    this.PropertyCollection.Add(new IntProperty() { Name = "EPOCH SIZE", Value = 500 });
                    this.PropertyCollection.Add(new IntProperty() { Name = "BATCH SIZE", Value = 10 });
                    this.PropertyCollection.Add(new IntProperty() { Name = "IMAGE WIDTH", Value = 255 });
                    this.PropertyCollection.Add(new IntProperty() { Name = "IMAGE HEIGHT", Value = 255 });
                    this.PropertyCollection.Add(new DoubleProperty() { Name = "LEARING RATE", Value = 0.003 });
                    this.PropertyCollection.Add(new DoubleProperty() { Name = "VALIDATION SAMPLE RATE", Value = 0.15 });
                    this.PropertyCollection.Add(new DoubleProperty() { Name = "TARGET ACCURACY", Value = 0.90 });
                }

                return _PropertyCollection;
            }
        }
    }
}
