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
using System.Windows.Media;
using System.Text.RegularExpressions;
using GalaSoft.MvvmLight.Messaging;
using System.Drawing;
using System.Windows.Media.Imaging;

namespace OpenDL.ViewModel
{
    public class ClassificationTrainViewModel : ViewModelBase
    {

        private Thread TrainTaskThread;
        private bool IsThreadloop = true;
        private bool IsInterrupt = false;


        /// <summary>
        /// Train variablees
        /// </summary>
        /// 
        private ClassLabelInfo classLabelInfo = null;
        private int batchSize = 0;
        private int epoch = 0;
        private int batchStep = 0;
        private int validationSampleCount = 0;
        private double learningRate = 0;
        private double validationSampleRate = 0;
        private double targetAccuracy = 0;
        private ClassTrainModelInfo modelInfo = null;
        private bool isGray = false;
        private int imageWidth = 0;
        private int imageHeight = 0;
        private int labelOutput = 0;
        private int trainLabelOutput = 0;
        private int imageChannel = 1;
        private string preModelOutputPath = "";
        private string modelName = "";



        private AutoResetEvent TrainTaskSignal = new AutoResetEvent(false);



        private readonly FileBrowserService folderBrowserService;
        private readonly TrainingService trainSampleLoaderService;
        private readonly ConfigureService configureService;
        private readonly DialogService dialogService;

        public ClassificationTrainViewModel(FileBrowserService _folderBrowserService,
                                          TrainingService _trainSampleLoaderService,
                                          ConfigureService _configureServie,
                                          DialogService _dialogService)
        {

            this.folderBrowserService = _folderBrowserService;
            this.trainSampleLoaderService = _trainSampleLoaderService;
            this.configureService = _configureServie;
            this.dialogService = _dialogService;

            this.TrainCostCollection = new ObservableCollection<LinePlotInfo>();
            this.ValidationCostCollection = new ObservableCollection<LinePlotInfo>();
            this.TrainAccuracyCollection = new ObservableCollection<LinePlotInfo>();
            this.ValidationAccuracyCollection = new ObservableCollection<LinePlotInfo>();
            this.PureClassModelCollection = new ObservableCollection<DeepModelPath>();
            this.TrainSampleCollection = new ObservableCollection<ClassTrainSample>();
            //this.BestSamplePreviewCollection = new ObservableCollection<SegmentPreviewItem>();
            //this.WorstSamplePreviewCollection = new ObservableCollection<SegmentPreviewItem>();


            this.TrainTaskThread = new Thread(new ThreadStart(TrainTaskStart));
            this.TrainTaskThread.IsBackground = true;
            this.TrainTaskThread.Start();


            // update 모델
            this.RefreshPureModelCommand.Execute(null);
        }


        ~ClassificationTrainViewModel()
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
                    var saver = tf.train.import_meta_graph(configureService.ClassificationTrainedModelUnzipPath + Path.DirectorySeparatorChar + this.modelInfo.MetaFile);
                    var sess = tf.Session(graph);

                    saver.restore(sess, configureService.ClassificationTrainedModelUnzipPath + Path.DirectorySeparatorChar + this.modelInfo.CheckFile);


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

                    List<ClassTrainSample> trainSample = this.TrainSampleCollection.ToList<ClassTrainSample>();
                    List<ClassTrainSample> validationSample = new List<ClassTrainSample>();
                    validationSample.AddRange(trainSample.GetRange(0, this.validationSampleCount));

                    for (int epoch = 0; epoch < this.epoch && IsInterrupt != true; epoch++)
                    {

                        /// Train sample
                        double totalTrainAccuracy = 0;
                        double totalTrainCost = 0;
                        TrainingService.Shuffle<ClassTrainSample>(trainSample);
                        for (int batchStep = 0; batchStep < this.batchStep && IsInterrupt != true; batchStep++)
                        {
                            int collectionIndex = batchStep * this.batchSize;
                            var batch = this.trainSampleLoaderService.LoadBatch(trainSample,
                                                                                collectionIndex,
                                                                                this.batchSize,
                                                                                isGray,
                                                                                imageWidth,
                                                                                imageHeight,
                                                                                trainLabelOutput);

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
                        //int bestScoreIndex = 0;
                        //int worstScoreIndex = 0;
                        string bestScoreFileName = "";
                        string worstScoreFileName = "";
                        string bestScoreLabelName = "";
                        string worstScoreLabelName = "";
                        for (int index = 0; index < this.validationSampleCount && IsInterrupt != true; index++)
                        {
                            var batch = this.trainSampleLoaderService.LoadBatch(validationSample,
                                                    index,
                                                    1,
                                                    isGray,
                                                    imageWidth,
                                                    imageHeight,
                                                    trainLabelOutput);

                            var validationImage = batch.Item1;
                            var validationLabel = batch.Item2;



                            var costNDArray = sess.run(new[] { Cost }, new FeedItem(X, validationImage), new FeedItem(Y, validationLabel), new FeedItem(Phase, false));
                            var accNDArray = sess.run(new[] { Accuracy }, new FeedItem(X, validationImage), new FeedItem(Y, validationLabel), new FeedItem(Phase, false));

                            var classResult = sess.run(new[] { Output }, new FeedItem(X, validationImage), new FeedItem(Phase, false));

                            int labelIndex = validationSample[index].LabelIndex;
                            var classScore = double.Parse(classResult[0][0,labelIndex].ToString());



                            var costDoubleArray = double.Parse(costNDArray[0].ToString());
                            var accDoubleArray = double.Parse(accNDArray[0].ToString());



                            if (bestScore < classScore)
                            {
                                bestScore = classScore;
                                bestScoreFileName = validationSample[index].InputImagePath;
                                bestScoreLabelName = this.classLabelInfo.Labels[labelIndex].Name;
                            }

                            if (worstScore > classScore)
                            {
                                worstScore = classScore;
                                worstScoreFileName = validationSample[index].InputImagePath;
                                worstScoreLabelName = this.classLabelInfo.Labels[labelIndex].Name;
                            }

                            totalValidationAccuracy += (accDoubleArray);
                            totalValidationCost += (costDoubleArray);
                        }
                        totalValidationAccuracy /= this.validationSampleCount;
                        totalValidationCost /= this.validationSampleCount;
                        /// Validation Sample



                        //// Best , Worst Image 추출



                        // UI 업데이트                                                                                                                    
                        Application.Current.Dispatcher.Invoke(() =>
                        {

                            try
                            {
                                //그래프 및 스코어 업데이트
                                this.BestSamplePreviewScore = bestScore;
                                this.WorstSamplePreviewScore = worstScore;

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

                                this.CurrentValidationAccuracy = totalValidationAccuracy;
                                this.CurrentLoss = totalValidationCost;

                                this.CurrentTrainAccuracy = totalTrainAccuracy;

                                // 이미지 업데이트

                                Mat bestMatImage = new Mat(bestScoreFileName);
                                Bitmap bestBitmapImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(bestMatImage);
                                BitmapSource bestBitmapSourceImage = this.trainSampleLoaderService.ConvertToBitmapSource(bestBitmapImage);

                                Mat worstMatImage = new Mat(worstScoreFileName);
                                Bitmap worstBitmapImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(worstMatImage);
                                BitmapSource worstBitmapSourceImage = this.trainSampleLoaderService.ConvertToBitmapSource(worstBitmapImage);

                                ClassPreviewItem bestItem = new ClassPreviewItem()
                                {
                                    Name = bestScoreLabelName,
                                    Score = bestScore,
                                    Image = bestBitmapSourceImage
                                };

                                ClassPreviewItem worstItem = new ClassPreviewItem()
                                {
                                    Name = worstScoreLabelName,
                                    Score = worstScore,
                                    Image = worstBitmapSourceImage
                                };

                                this.BestSamplePreview = bestItem;
                                this.WorstSamplePreview = worstItem;
                            }
                            catch(Exception e)
                            {

                            }

                        });

                        // 학습 완료.
                        if (this.CurrentValidationAccuracy >= this.targetAccuracy && this.CurrentTrainAccuracy >= this.targetAccuracy)
                        {
                            saver.save(sess, configureService.ClassificationTrainedModelUnzipPath + Path.DirectorySeparatorChar + this.modelInfo.CheckFile, write_meta_graph: false);
                            sess.close();
                            if (this.trainSampleLoaderService.ZipDeepModel(this.configureService.ClassificationTrainedModelUnzipPath,
                                                                       this.preModelOutputPath,
                                                                       this.configureService.SecurityPassword) == false)
                            {
                                this.dialogService.ShowErrorMessage("모델 병합에 실패했습니다.");
                            }
                            this.trainSampleLoaderService.DeleteUnzipFiles();


                            TrainMessage message = new TrainMessage()
                            {
                                Message = "Complete"
                            };
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Messenger.Default.Send<TrainMessage>(message);
                            });

                            this.dialogService.ShowConfirmMessage("학습이 종료되었습니다.");

                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    this.dialogService.ShowErrorMessage(e.ToString());
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
                if (_TrainStartCommand == null)
                {
                    _TrainStartCommand = new RelayCommand(() =>
                    {

                        if (this.classLabelInfo == null)
                        {
                            this.dialogService.ShowErrorMessage("라벨 정보가 없습니다.");
                            return;
                        }
                        if (this.SelectedClassificationModel == null)
                        {
                            this.dialogService.ShowErrorMessage("모델이 선택되지 않았습니다.");
                            return;
                        }

                        this.CurrentStatus = "학습 준비중";

                        // 그래프 및 UI초기화
                        this.BestSamplePreview = null;
                        this.WorstSamplePreview = null;
                        this.WorstSamplePreviewScore = 0;
                        this.BestSamplePreviewScore = 0;
                        this.TrainCostCollection.Clear();
                        this.TrainAccuracyCollection.Clear();
                        this.ValidationCostCollection.Clear();
                        this.ValidationAccuracyCollection.Clear();

                        // 사용자 설정값들 불러오기
                        this.modelName = ((StringProperty)this.PropertyCollection.Where(x => x.Name == "MODEL NAME").FirstOrDefault()).Value + ".model";
                        this.epoch = ((IntProperty)this.PropertyCollection.Where(x => x.Name == "EPOCH SIZE").FirstOrDefault()).Value;
                        this.batchSize = ((IntProperty)this.PropertyCollection.Where(x => x.Name == "BATCH SIZE").FirstOrDefault()).Value;
                        this.learningRate = ((DoubleProperty)this.PropertyCollection.Where(x => x.Name == "LEARING RATE").FirstOrDefault()).Value;
                        this.validationSampleRate = ((DoubleProperty)this.PropertyCollection.Where(x => x.Name == "VALIDATION SAMPLE RATE").FirstOrDefault()).Value;
                        this.targetAccuracy = ((DoubleProperty)this.PropertyCollection.Where(x => x.Name == "TARGET ACCURACY").FirstOrDefault()).Value;
                        this.validationSampleCount = (int)(this.TrainSampleCollection.Count * this.validationSampleRate);
                        this.batchStep = (int)((this.TrainSampleCollection.Count - validationSampleCount) / this.batchSize);

                        // 라벨정보에서 필요한 정보 추출
                        this.isGray = this.classLabelInfo.IsGray;
                        this.imageWidth = this.classLabelInfo.ImageWidth;
                        this.imageHeight = this.classLabelInfo.ImageHeight;
                        this.labelOutput = this.classLabelInfo.LabelSize;
                        this.imageChannel = this.isGray == true ? 1 : 3;

                        // 모델 압축해재에 필요한 정보 가져오기
                        string targetModelPath = this.SelectedClassificationModel.FullPath;
                        string unzipPath = configureService.ClassificationTrainedModelUnzipPath;
                        string password = configureService.SecurityPassword;

                        // 모델 압축 해제 전 폴더 파일들 삭제
                        trainSampleLoaderService.DeleteUnzipFiles();

                        // 모델 압축 해제 
                        if (trainSampleLoaderService.UnZipDeepModel(targetModelPath, configureService.ClassificationTrainedModelUnzipPath, configureService.SecurityPassword) == false)
                        {
                            this.dialogService.ShowErrorMessage("모델을 추출하는데 실패했습니다.");
                            return;
                        }

                        // 모델 로드
                        this.modelInfo = trainSampleLoaderService.LoadClassTrainModelInfo(unzipPath + Path.DirectorySeparatorChar + configureService.ModelInfoFileName);
                        this.trainLabelOutput = this.modelInfo.MaxLabelCount;

                        // 타겟 모델 경로
                        this.preModelOutputPath = configureService.ClassificationTrainedModelContainerPath + Path.DirectorySeparatorChar + modelName;


                        /// Label 정보 및 모델 정보 확인 
                        /// 
                        
                        if (this.modelInfo == null)
                        {
                            this.dialogService.ShowErrorMessage("Classification 모델 로드에 실패했습니다");
                            return;
                        }

                        if (File.Exists(this.preModelOutputPath) == true)
                        {
                            this.dialogService.ShowErrorMessage("이미 존재하는 학습된 모델입니다.");
                            return;
                        }

                        Regex rex = new Regex(this.configureService.SpecialCharacter);
                        if (this.modelName.Length == 0 || rex.IsMatch(this.modelName) == true || this.modelName.Equals(".model") == true)
                        {
                            this.dialogService.ShowErrorMessage("올바르지 않은 모델명입니다.");
                            return;
                        }

                        if (this.batchStep <= 0)
                        {
                            this.dialogService.ShowErrorMessage("학습 샘플이 충분하지 않습니다. 배치사이즈와 검증샘플 비율을 조절하십시오.");
                            return;
                        }

                        if (this.imageWidth != this.modelInfo.Width)
                        {
                            this.dialogService.ShowErrorMessage("이미지 너비가 맞지 않습니다.");
                            return;
                        }
                        if (this.imageHeight != this.modelInfo.Height)
                        {
                            this.dialogService.ShowErrorMessage("이미지 높이 맞지 않습니다.");
                            return;
                        }
                        if (this.classLabelInfo.IsGray != this.modelInfo.IsGray)
                        {
                            this.dialogService.ShowErrorMessage("이미지 색상 정보가 맞지 않습니다.");
                            return;
                        }
                        if (this.trainLabelOutput < this.labelOutput)
                        {
                            this.dialogService.ShowErrorMessage("라벨 갯수가 맞지 않습니다.");
                            return;
                        }

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
                if (_TrainStopCommand == null)
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
                if (_TrainPauseCommand == null)
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
                if (_OpenAugmentedLabelCommand == null)
                {
                    _OpenAugmentedLabelCommand = new RelayCommand(async () =>
                    {
                        CurrentOpenedLabelDirectory = folderBrowserService.SelectFolder();
                        if (CurrentOpenedLabelDirectory.Length <= 0)
                        {
                            this.dialogService.ShowErrorMessage("경로가 선택되지 않았습니다.");
                            return;
                        }

                        var result = await this.trainSampleLoaderService.LoadClassSamplesAsync(CurrentOpenedLabelDirectory);
                        this.TrainSampleCollection = null;
                        this.TrainSampleCollection = result.Item1;
                        this.classLabelInfo = null;
                        this.classLabelInfo = result.Item2;

                        if (this.classLabelInfo.LabelSize == 0)
                        {
                            this.dialogService.ShowErrorMessage("올바르지 않은 라벨 정보입니다.");
                            return;
                        }

                        ((BoolProperty)this.PropertyCollection.Where(x => x.Name == "GRAY").FirstOrDefault()).Value = this.classLabelInfo.IsGray;
                        ((IntProperty)this.PropertyCollection.Where(x => x.Name == "IMAGE WIDTH").FirstOrDefault()).Value = this.classLabelInfo.ImageWidth;
                        ((IntProperty)this.PropertyCollection.Where(x => x.Name == "IMAGE HEIGHT").FirstOrDefault()).Value = this.classLabelInfo.ImageHeight;

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
                if (_RefreshPureModelCommand == null)
                {
                    _RefreshPureModelCommand = new RelayCommand(() =>
                    {
                        this.PureClassModelCollection.Clear();
                        string path = configureService.ClassificationPureModelContainerPath;
                        string[] models = Directory.GetFiles(path);
                        foreach (var model in models)
                        {
                            this.PureClassModelCollection.Add(new DeepModelPath()
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


        private double _CurrentValidationAccuracy = 0;
        public double CurrentValidationAccuracy
        {
            set => Set<double>(nameof(CurrentValidationAccuracy), ref _CurrentValidationAccuracy, value);
            get => _CurrentValidationAccuracy;
        }

        private double _CurrentTrainAccuracy = 0;
        public double CurrentTrainAccuracy
        {
            set => Set<double>(nameof(CurrentTrainAccuracy), ref _CurrentTrainAccuracy, value);
            get => _CurrentTrainAccuracy;
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

        private ObservableCollection<ClassTrainSample> _TrainSamplesCollection = null;
        public ObservableCollection<ClassTrainSample> TrainSampleCollection
        {
            get => _TrainSamplesCollection;
            set => Set<ObservableCollection<ClassTrainSample>>(nameof(TrainSampleCollection), ref _TrainSamplesCollection, value);
        }


        private ObservableCollection<DeepModelPath> _PureClassModelCollection = null;
        public ObservableCollection<DeepModelPath> PureClassModelCollection
        {
            get => _PureClassModelCollection;
            set => Set<ObservableCollection<DeepModelPath>>(nameof(PureClassModelCollection), ref _PureClassModelCollection, value);
        }


        private ClassPreviewItem _BestSamplePreview = null;
        public ClassPreviewItem BestSamplePreview
        {
            get => _BestSamplePreview;
            set => Set<ClassPreviewItem>(nameof(BestSamplePreview), ref _BestSamplePreview, value);
        }

        private ClassPreviewItem _WorstSamplePreview = null;
        public ClassPreviewItem WorstSamplePreview
        {
            get => _WorstSamplePreview;
            set => Set<ClassPreviewItem>(nameof(WorstSamplePreview), ref _WorstSamplePreview, value);
        }

        private double _BestSamplePreviewScore = 0;
        public double BestSamplePreviewScore
        {
            get => _BestSamplePreviewScore;
            set => Set<double>(nameof(BestSamplePreviewScore), ref _BestSamplePreviewScore, value);
        }

        private double _WorstSamplePreviewScore = 0;
        public double WorstSamplePreviewScore
        {
            get => _WorstSamplePreviewScore;
            set => Set<double>(nameof(WorstSamplePreviewScore), ref _WorstSamplePreviewScore, value);
        }



        private DeepModelPath _SelectedClassificationModel = null;
        public DeepModelPath SelectedClassificationModel
        {
            get => _SelectedClassificationModel;
            set => Set<DeepModelPath>(nameof(SelectedClassificationModel), ref _SelectedClassificationModel, value);
        }




        private ObservableCollection<BaseProperty> _PropertyCollection = null;
        public ObservableCollection<BaseProperty> PropertyCollection
        {
            get
            {
                if (_PropertyCollection == null)
                {
                    _PropertyCollection = new ObservableCollection<BaseProperty>();

                    this.PropertyCollection.Add(new StringProperty() { Name = "MODEL NAME", Value = "AnonymousModel", IsReadOnly = false });
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
