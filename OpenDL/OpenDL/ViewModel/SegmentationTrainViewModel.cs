 using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using OpenDL.Model;
using OpenDL.Service;
using OpenCvSharp;
using System.Runtime.InteropServices;
using DevExpress.Compression;
using Tensorflow;
using static Tensorflow.Binding;
using Newtonsoft.Json;
using NumSharp;
using Size = OpenCvSharp.Size;

namespace OpenDL.ViewModel
{
    public class SegmentationTrainViewModel : ViewModelBase
    {

        private Thread TrainTaskThread;
        private bool IsThreadloop = true;
        private bool IsInterrupt = false;



        /// <summary>
        /// Train variablees
        /// </summary>
        /// 
        private SegmentLabelInfo segmentLabelInfo = null;
        private int batchSize = 0;
        private int epoch = 0;
        private int batchStep = 0;
        private int validationSampleCount = 0;
        private double learningRate = 0;
        private double validationSampleRate = 0;
        private double targetAccuracy = 0;
        private SegmentTrainModelInfo modelInfo = null;
        bool isGray = false;
        int imageWidth = 0;
        int imageHeight = 0;
        int labelOutput = 0;
        int imageChannel = 1;



        private AutoResetEvent TrainTaskSignal = new AutoResetEvent(false);



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
            this.BestSamplePreviewCollection = new ObservableCollection<SegmentPreviewItem>();
            this.WorstSamplePreviewCollection = new ObservableCollection<SegmentPreviewItem>();


            this.TrainTaskThread = new Thread(new ThreadStart(TrainTaskStart));
            this.TrainTaskThread.IsBackground = true;
            this.TrainTaskThread.Start();


            // update 모델
            this.RefreshPureModelCommand.Execute(null);
        }


        ~SegmentationTrainViewModel()
        {
            this.IsThreadloop = false;
            this.TrainTaskSignal.Set();
            this.TrainTaskThread.Join(5000);
        }

        private void TrainTaskStart()
        {
            Random random = new Random();
            while (this.IsThreadloop == true)
            {
                this.TrainTaskSignal.WaitOne();
                if (this.IsThreadloop == false) break;

                this.IsTraining = true;
                /// Process Here!!
                /// 

                this.CurrentStatus = "학습 중";
                /// Tensorflow Processing Here; 
                /// 

                try
                {

                    var graph = tf.Graph().as_default();
                    var saver = tf.train.import_meta_graph(configureService.SegmentationTrainedModelUnzipPath + Path.DirectorySeparatorChar + this.modelInfo.MetaFile);
                    var sess = tf.Session(graph);

                    saver.restore(sess, configureService.SegmentationTrainedModelUnzipPath + Path.DirectorySeparatorChar + this.modelInfo.CheckFile);


                    Tensor X = graph.get_tensor_by_name(this.modelInfo.InputNodeName);
                    Tensor Y = graph.get_tensor_by_name(this.modelInfo.TargetNodeName);
                    Tensor Output = graph.get_tensor_by_name(this.modelInfo.OutputNodeName);
                    Tensor Phase = graph.get_tensor_by_name(this.modelInfo.PhaseNodeName);
                    Tensor Cost = graph.get_tensor_by_name(this.modelInfo.CostNodeName);
                    Tensor Accuracy = graph.get_tensor_by_name(this.modelInfo.AccuracyNodeName);
                    Tensor LearningRateTensor = graph.get_tensor_by_name(this.modelInfo.LearningRateOperationName);
                    Operation global_init = sess.graph.get_operation_by_name(this.modelInfo.GlobalVariableInitializerName);
                    Operation Optimizer_op = graph.get_operation_by_name(this.modelInfo.TrainOperationName);


                    sess.run(global_init);

                    List<SegmentTrainSample> trainSample = this.TrainSampleCollection.ToList<SegmentTrainSample>();
                    List<SegmentTrainSample> validationSample = new List<SegmentTrainSample>();
                    validationSample.AddRange(trainSample.GetRange(0, this.validationSampleCount));

                    for (int epoch = 0; epoch < this.epoch && IsInterrupt != true; epoch++)
                    {

                        /// Train sample
                        double totalTrainAccuracy = 0;
                        double totalTrainCost = 0;
                        TrainingService.Shuffle<SegmentTrainSample>(trainSample);
                        for (int batchStep = 0; batchStep < this.batchStep && IsInterrupt != true; batchStep++)
                        {
                            int collectionIndex = batchStep * this.batchSize;
                            var batch = this.trainSampleLoaderService.LoadBatch(trainSample,
                                                                                collectionIndex,
                                                                                this.batchSize,
                                                                                isGray,
                                                                                imageWidth,
                                                                                imageHeight,
                                                                                labelOutput);

                            var trainImage = batch.Item1;
                            var trainLabel = batch.Item2;

                            sess.run(new[] { Optimizer_op }, new FeedItem(X, trainImage), new FeedItem(Y, trainLabel), new FeedItem(Phase, true), new FeedItem(LearningRateTensor, learningRate));
                            var costNDArray = sess.run(new[] { Cost }, new FeedItem(X, trainImage), new FeedItem(Y, trainLabel), new FeedItem(Phase, false));
                            var accNDArray = sess.run(new[] { Accuracy }, new FeedItem(X, trainImage), new FeedItem(Y, trainLabel), new FeedItem(Phase, false));

                            var costDoubleArray = double.Parse(costNDArray[0].ToString());
                            var accDoubleArray = double.Parse(accNDArray[0].ToString());
                            totalTrainCost += (costDoubleArray);
                            totalTrainAccuracy += (accDoubleArray);

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                this.CurrentBatchStep = string.Format("{0} / {1}", batchStep, this.batchStep);
                            });

                            Thread.Sleep(10);
                        }
                        totalTrainCost /= this.batchStep;
                        totalTrainAccuracy /= this.batchStep;
                        /// Train sample



                        /// Validation Sample
                        double totalValidationAccuracy = 0;
                        double totalValidationCost = 0;

                        double bestScore = 0;
                        double worstScore = 100;
                        int bestScoreIndex = 0;
                        int worstScoreIndex = 0;
                        for (int index = 0; index < this.validationSampleCount && IsInterrupt != true; index++)
                        {
                            var batch = this.trainSampleLoaderService.LoadBatch(validationSample,
                                                    index,
                                                    1,
                                                    isGray,
                                                    imageWidth,
                                                    imageHeight,
                                                    labelOutput);

                            var validationImage = batch.Item1;
                            var validationLabel = batch.Item2;

                            

                            var costNDArray = sess.run(new[] { Cost }, new FeedItem(X, validationImage), new FeedItem(Y, validationLabel), new FeedItem(Phase, false));
                            var accNDArray = sess.run(new[] { Accuracy }, new FeedItem(X, validationImage), new FeedItem(Y, validationLabel), new FeedItem(Phase, false));

                            var costDoubleArray = double.Parse(costNDArray[0].ToString());
                            var accDoubleArray = double.Parse(accNDArray[0].ToString());


                            if (bestScore < accDoubleArray)
                            {
                                bestScore = accDoubleArray;
                                bestScoreIndex = index;
                            }

                            if(worstScore > accDoubleArray)
                            {
                                worstScore = accDoubleArray;
                                worstScoreIndex = index;
                            }

                            totalValidationAccuracy += (accDoubleArray);
                            totalValidationCost += (costDoubleArray);
                        }
                        totalValidationAccuracy /= this.validationSampleCount;
                        totalValidationCost /= this.validationSampleCount;
                        /// Validation Sample



                        // Best , Worst Image 추출
                        var worstNDArray = this.trainSampleLoaderService.LoadBatch(validationSample,
                                                    worstScoreIndex,
                                                    1,
                                                    isGray,
                                                    imageWidth,
                                                    imageHeight,
                                                    labelOutput);

                        var bestNDArray = this.trainSampleLoaderService.LoadBatch(validationSample,
                                                    bestScoreIndex,
                                                    1,
                                                    isGray,
                                                    imageWidth,
                                                    imageHeight,
                                                    labelOutput);


                        

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            this.TrainCostCollection.Add(new LinePlotInfo()
                            {
                                Step = epoch,
                                Value = totalTrainCost
                            });

                            this.TrainAccuracyCollection.Add(new LinePlotInfo()
                            {
                                Step = epoch,
                                Value = totalTrainAccuracy
                            });

                            this.ValidationCostCollection.Add(new LinePlotInfo()
                            {
                                Step = epoch,
                                Value = totalValidationCost
                            });

                            this.ValidationAccuracyCollection.Add(new LinePlotInfo()
                            {
                                Step = epoch,
                                Value = totalValidationAccuracy
                            });

                            this.CurrentAccuracy = totalValidationAccuracy;
                            this.CurrentLoss = totalValidationCost;


                            // 현재 이미지 출력 worst 이미지 
                            var worstSourceImage = this.trainSampleLoaderService.NDArrayToMat(worstNDArray.Item1["0,:,:,:"],
                                      this.imageChannel,
                                      this.imageWidth,
                                      this.imageHeight);

                            this.WorstSamplePreviewCollection.Clear();
                            this.WorstSamplePreviewCollection.Add(new SegmentPreviewItem()
                            {
                                Image = this.trainSampleLoaderService.ConvertToBitmapSource(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(worstSourceImage)),
                                Alpha = 1
                            });
                            
                            // 마스크이키지 만들기
                            for (int index = 0; index < this.segmentLabelInfo.Labels.Count; index++)
                            {
                                var label = this.segmentLabelInfo.Labels[index];
                                Mat mask = new Mat(new Size(this.imageWidth, this.imageHeight), MatType.CV_8UC3, new Scalar(label.Color.R, label.Color.G, label.Color.B));
                                var worstSourceMaskImage = this.trainSampleLoaderService.NDArrayToMat(worstNDArray.Item2["0,:,:," + index],
                                                                                                      1,
                                                                                                      this.imageWidth,
                                                                                                      this.imageHeight);
                                worstSourceMaskImage = worstSourceMaskImage.Threshold(0.5, 1, ThresholdTypes.Binary);
                                Mat result = new Mat();
                                OpenCvSharp.Cv2.BitwiseAnd(mask, worstSourceMaskImage.CvtColor(ColorConversionCodes.GRAY2BGR), result);
                                this.WorstSamplePreviewCollection.Add(new SegmentPreviewItem()
                                {
                                    Image = this.trainSampleLoaderService.ConvertToBitmapSource(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(result)),
                                    Alpha = 0.5
                                });
                            }



                            // 현재 이미지 출력 best 이미지 
                            var bestSourceImage = this.trainSampleLoaderService.NDArrayToMat(bestNDArray.Item1["0,:,:,:"],
                                      this.imageChannel,
                                      this.imageWidth,
                                      this.imageHeight);

                            this.BestSamplePreviewCollection.Clear();
                            this.BestSamplePreviewCollection.Add(new SegmentPreviewItem()
                            {
                                Image = this.trainSampleLoaderService.ConvertToBitmapSource(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(bestSourceImage)),
                                Alpha = 1
                            });

                            // 마스크이키지 만들기
                            for (int index = 0; index < this.segmentLabelInfo.Labels.Count; index++)
                            {
                                var label = this.segmentLabelInfo.Labels[index];
                                Mat mask = new Mat(new Size(this.imageWidth, this.imageHeight), MatType.CV_8UC3, new Scalar(label.Color.R, label.Color.G, label.Color.B));
                                var bestSourceMaskImage = this.trainSampleLoaderService.NDArrayToMat(bestNDArray.Item2["0,:,:," + index],
                                                                                                      1,
                                                                                                      this.imageWidth,
                                                                                                      this.imageHeight);
                                bestSourceMaskImage = bestSourceMaskImage.Threshold(0.5, 1, ThresholdTypes.Binary);
                                Mat result = new Mat();
                                OpenCvSharp.Cv2.BitwiseAnd(mask, bestSourceMaskImage.CvtColor(ColorConversionCodes.GRAY2BGR), result);
                                this.BestSamplePreviewCollection.Add(new SegmentPreviewItem()
                                {
                                    Image = this.trainSampleLoaderService.ConvertToBitmapSource(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(result)),
                                    Alpha = 0.5
                                });
                            }
                        });

                        if (this.CurrentAccuracy >= this.targetAccuracy)
                            break;
                    }
                }
                catch(Exception e)
                {
                    System.Console.WriteLine("test");
                }

                this.CurrentStatus = "학습 종료";
                this.IsInterrupt = false;
                this.IsTraining = false;

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

                        if (this.segmentLabelInfo == null) return;
                        if (this.SelectedSegmentModel == null) return;

                        this.CurrentStatus = "학습 준비중";

                        // 사용자 설정값들 불러오기
                        this.epoch = ((IntProperty)this.PropertyCollection.Where(x => x.Name == "EPOCH SIZE").FirstOrDefault()).Value;
                        this.batchSize = ((IntProperty)this.PropertyCollection.Where(x => x.Name == "BATCH SIZE").FirstOrDefault()).Value;
                        this.learningRate = ((DoubleProperty)this.PropertyCollection.Where(x => x.Name == "LEARING RATE").FirstOrDefault()).Value;
                        this.validationSampleRate = ((DoubleProperty)this.PropertyCollection.Where(x => x.Name == "VALIDATION SAMPLE RATE").FirstOrDefault()).Value;
                        this.targetAccuracy = ((DoubleProperty)this.PropertyCollection.Where(x => x.Name == "TARGET ACCURACY").FirstOrDefault()).Value;
                        this.validationSampleCount = (int)(this.TrainSampleCollection.Count * this.validationSampleRate);
                        this.batchStep = (int)((this.TrainSampleCollection.Count - validationSampleCount) / this.batchSize);

                        // 라벨정보에서 필요한 정보 추출
                        this.isGray = this.segmentLabelInfo.IsGray;
                        this.imageWidth = this.segmentLabelInfo.ImageWidth;
                        this.imageHeight = this.segmentLabelInfo.ImageHeight;
                        this.labelOutput = this.segmentLabelInfo.LabelSize;
                        this.imageChannel = this.isGray == true ? 1 : 3;

                        // 모델 압축해재에 필요한 정보 가져오기
                        string targetModelPath = this.SelectedSegmentModel.FullPath;
                        string unzipPath = configureService.SegmentationTrainedModelUnzipPath;
                        string password = configureService.SecurityPassword;


                        // 모델 압축 해제 전 폴더 파일들 삭제
                        trainSampleLoaderService.DeleteUnzipFiles();

                        // 모델 압축 해제 
                        try
                        {
                            using (ZipArchive archive = ZipArchive.Read(targetModelPath))
                            {
                                archive.Password = password;

                                foreach (ZipItem item in archive)
                                {
                                    item.Extract(unzipPath);
                                }
                            }

                        }
                        catch(Exception e)
                        {
                            return;
                        }


                        // 압축 해제된 파일에서 모델 정보 불러오기
                        this.modelInfo = null;
                        try
                        {
                            using (StreamReader reader = new StreamReader(configureService.SegmentationTrainedModelUnzipPath + Path.DirectorySeparatorChar + configureService.ModelInfoFileName, Encoding.UTF8))
                            {
                                string labelContent = reader.ReadToEnd();
                                this.modelInfo = (SegmentTrainModelInfo)JsonConvert.DeserializeObject(labelContent, typeof(SegmentTrainModelInfo));
                            }

                        }
                        catch (Exception e)
                        {
                            return;
                        }


                        /// Label 정보 및 모델 정보 확인 
                        /// 
                        if (this.modelInfo == null) return;
                        if (this.imageWidth != this.modelInfo.Width) return;
                        if (this.imageHeight != this.modelInfo.Height) return;
                        if (this.segmentLabelInfo.IsGray != this.modelInfo.IsGray) return;
                        if (this.segmentLabelInfo.LabelSize != this.modelInfo.MaxLabelCount) return;
                       
                        this.TrainTaskSignal.Set();
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

                        if (this.IsTraining == false) return;
                        if (this.IsInterrupt == true) return;

                        if (this.IsTraining == true)
                            this.IsInterrupt = true;

                        this.TrainTaskSignal.Reset();
                        this.TrainCostCollection.Clear();
                        this.ValidationCostCollection.Clear();
                        this.TrainAccuracyCollection.Clear();
                        this.ValidationAccuracyCollection.Clear();
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
                        if (CurrentOpenedLabelDirectory.Length <= 0)
                        {
                            return;
                        }

                        var result = await this.trainSampleLoaderService.LoadSegmentSamplesAsync(CurrentOpenedLabelDirectory);
                        this.TrainSampleCollection = null;
                        this.TrainSampleCollection = result.Item1;
                        this.segmentLabelInfo = null;
                        this.segmentLabelInfo = result.Item2;

                        ((BoolProperty)this.PropertyCollection.Where(x => x.Name == "GRAY").FirstOrDefault()).Value = this.segmentLabelInfo.IsGray;
                        ((IntProperty)this.PropertyCollection.Where(x => x.Name == "IMAGE WIDTH").FirstOrDefault()).Value = this.segmentLabelInfo.ImageWidth;
                        ((IntProperty)this.PropertyCollection.Where(x => x.Name == "IMAGE HEIGHT").FirstOrDefault()).Value = this.segmentLabelInfo.ImageHeight;

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


        private string _CurrentStatus = "대기";
        public string CurrentStatus
        {
            get => _CurrentStatus;
            set => Set<string>(nameof(CurrentStatus), ref _CurrentStatus, value);
        }


        private bool _IsTraining = false;
        public bool IsTraining
        {
            get => _IsTraining;
            set => Set<bool>(nameof(IsTraining), ref _IsTraining, value);
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


        private ObservableCollection<SegmentPreviewItem> _BestSamplePreviewCollection = null;
        public ObservableCollection<SegmentPreviewItem> BestSamplePreviewCollection
        {
            get => _BestSamplePreviewCollection;
            set => Set<ObservableCollection<SegmentPreviewItem>>(nameof(BestSamplePreviewCollection), ref _BestSamplePreviewCollection, value);
        }

        private ObservableCollection<SegmentPreviewItem> _WorstSamplePreviewCollection = null;
        public ObservableCollection<SegmentPreviewItem> WorstSamplePreviewCollection
        {
            get => _WorstSamplePreviewCollection;
            set => Set<ObservableCollection<SegmentPreviewItem>>(nameof(WorstSamplePreviewCollection), ref _WorstSamplePreviewCollection, value);
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

                    this.PropertyCollection.Add(new StringProperty() { Name = "MODEL NAME", Value = "AnonymousModel" , IsReadOnly = false});
                    this.PropertyCollection.Add(new IntProperty() { Name = "IMAGE WIDTH", Value = 255, IsReadOnly = true });
                    this.PropertyCollection.Add(new IntProperty() { Name = "IMAGE HEIGHT", Value = 255, IsReadOnly = true });
                    this.PropertyCollection.Add(new BoolProperty() { Name = "GRAY", Value = false, IsReadOnly = true });

                    this.PropertyCollection.Add(new IntProperty() { Name = "EPOCH SIZE", Value = 500, IsReadOnly = false });
                    this.PropertyCollection.Add(new IntProperty() { Name = "BATCH SIZE", Value = 10, IsReadOnly = false });
                    this.PropertyCollection.Add(new DoubleProperty() { Name = "LEARING RATE", Value = 0.003, IsReadOnly = false });
                    this.PropertyCollection.Add(new DoubleProperty() { Name = "VALIDATION SAMPLE RATE", Value = 0.15, IsReadOnly = false });
                    this.PropertyCollection.Add(new DoubleProperty() { Name = "TARGET ACCURACY", Value = 0.90, IsReadOnly = false });
                }

                return _PropertyCollection;
            }
        }
    }
}
