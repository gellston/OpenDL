using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public SegmentationTrainViewModel(FolderBrowserService _folderBrowserService)
        {

            this.folderBrowserService = _folderBrowserService;

            this.TrainCostCollection = new ObservableCollection<LinePlotInfo>();
            this.ValidationCostCollection = new ObservableCollection<LinePlotInfo>();
            this.TrainAccuracyCollection = new ObservableCollection<LinePlotInfo>();
            this.ValidationAccuracyCollection = new ObservableCollection<LinePlotInfo>();


            this.PropertyCollection = new ObservableCollection<BaseProperty>();

            this.PropertyCollection.Add(new IntProperty()
            {
                Name = "EPOCH SIZE",
                Value = 500
            });

            this.PropertyCollection.Add(new IntProperty()
            {
                Name = "BATCH SIZE",
                Value = 10
            });

            this.PropertyCollection.Add(new IntProperty()
            {
                Name = "IMAGE WIDTH",
                Value = 255
            });

            this.PropertyCollection.Add(new IntProperty()
            {
                Name = "IMAGE HEIGHT",
                Value = 255
            });

            this.PropertyCollection.Add(new DoubleProperty()
            {
                Name = "LEARING RATE",
                Value = 0.003
            });

            this.PropertyCollection.Add(new DoubleProperty()
            {
                Name = "VALIDATION SAMPLE RATE",
                Value = 0.15
            });

            this.PropertyCollection.Add(new DoubleProperty()
            {
                Name = "TARGET ACCURACY",
                Value = 0.90
            });


            this.TrainTaskThread = new Thread(new ThreadStart(TrainTaskStart));
            this.TrainTaskThread.IsBackground = true;
            this.TrainTaskThread.Start();

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
                    _OpenAugmentedLabelCommand = new RelayCommand(() =>
                    {
                        CurrentOpenedLabelDirectory = folderBrowserService.SelectFolder();
                    });
                }
                return _OpenAugmentedLabelCommand;
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

        private ObservableCollection<BaseProperty> _PropertyCollection = null;
        public ObservableCollection<BaseProperty> PropertyCollection
        {
            get => _PropertyCollection;
            set => Set<ObservableCollection<BaseProperty>>(nameof(PropertyCollection), ref _PropertyCollection, value);
        }
    }
}
