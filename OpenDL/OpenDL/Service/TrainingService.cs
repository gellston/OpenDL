using Newtonsoft.Json;
using OpenDL.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using NumSharp;
using OpenCvSharp;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Media;
using System.Windows.Interop;
using Size = OpenCvSharp.Size;
using DevExpress.Compression;
using Tensorflow;
using static Tensorflow.Binding;
using Google.Protobuf;

namespace OpenDL.Service
{
    public class TrainingService
    {
        readonly ConfigureService configService;

        public TrainingService(ConfigureService _configService)
        {
            this.configService = _configService;

            
        }


        private static Random rng = new Random();
        public static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }


        public bool Freeze_Graph(string _meta_file_path, string _check_point_path, string _graph_output_path, string[] __output_nodes)
        {

            var graph4import = tf.Graph().as_default();
            var saver = tf.train.import_meta_graph(_meta_file_path);
            var sess = tf.Session(graph4import);

            try
            {
                saver.restore(sess, _check_point_path);
                var graph = sess.graph;
                var output_graph_def = tf.graph_util.convert_variables_to_constants(sess, graph.as_graph_def(), __output_nodes);
                File.WriteAllBytes(_graph_output_path, output_graph_def.ToByteArray());
                sess.close();
            }
            catch (Exception e)
            {
                System.Console.WriteLine("test");
                sess.close();
                return false;
            }


            return true;
        }

        public (NDArray, NDArray) LoadBatch(List<SegmentTrainSample> _sampleCollection, 
                                                                                     int _index, 
                                                                                     int _batchSize,
                                                                                     bool _isGray,
                                                                                     int _imageWidth,
                                                                                     int _imageHeight,
                                                                                     int _labelOutput)
        {

            int channel = 1;
            ImreadModes readMode = ImreadModes.Grayscale;
            if (_isGray == true)
            {
                channel = 1;
                readMode = ImreadModes.Grayscale;
            }
            else {
                channel = 3;
                readMode = ImreadModes.Color;
            }

            NDArray inputImageBatch = np.zeros((_batchSize, _imageHeight, _imageWidth, channel));
            NDArray inputLabelBatch = np.zeros((_batchSize, _imageHeight, _imageWidth, _labelOutput));
            int inputImageSize = _imageHeight * _imageHeight * channel;
            int labelImageSize = _imageHeight * _imageHeight * 1;

            for (int index =0; index < _batchSize; index++)
            {
                /// Source ND Image make
                int sampleIndex = index + _index;
                SegmentTrainSample sample = _sampleCollection[sampleIndex];
                string inputImagePath = sample.InputImagePath;
                Mat sourceImage = new Mat(inputImagePath, readMode);

                byte[] byteImage = new byte[inputImageSize];
                Marshal.Copy(sourceImage.Data, byteImage, 0, inputImageSize);
                NDArray sourceNDImage = np.array<byte>(byteImage, true);
                sourceNDImage = sourceNDImage.reshape((_imageHeight, _imageWidth, channel));
                inputImageBatch[index] = sourceNDImage;
                /// Source ND Image make
                /// 

                /// Label ND Image Make
                /// 
                //NDArray oneLabel = np.zeros((_imageHeight, _imageWidth, _labelOutput));
                List<NDArray> labelList = new List<NDArray>();
                for (int labelIndex = 0; labelIndex < _labelOutput; labelIndex++)
                {
                    string labelPath = sample.OutputLabelCollection[labelIndex];

                    Mat labelImage = new Mat(labelPath, ImreadModes.Grayscale);
                    Mat normalized = labelImage.Threshold(125, 255, ThresholdTypes.Binary);
                    normalized = normalized / 255;
                    byte[] byteLabelImage = new byte[labelImageSize];
                    Marshal.Copy(normalized.Data, byteLabelImage, 0, labelImageSize);
                    NDArray labelNDImage = np.array<byte>(byteLabelImage, true);
                    labelNDImage = labelNDImage.reshape((_imageHeight, _imageWidth, 1));
                    labelList.Add(labelNDImage);
                    //oneLabel[labelIndex] = labelNDImage;
                }
                NDArray oneLabel = np.concatenate(labelList.ToArray(), 2);
                inputLabelBatch[index] = oneLabel;
                /// Label ND Image Make
                /// 
            }


            return (inputImageBatch, inputLabelBatch);
        }



        public NDArray LoadBatch(List<AnomalTrainSample> _sampleCollection,
                                                                                     int _index,
                                                                                     int _batchSize,
                                                                                     bool _isGray,
                                                                                     int _imageWidth,
                                                                                     int _imageHeight)
        {

            int channel = 1;
            ImreadModes readMode = ImreadModes.Grayscale;
            if (_isGray == true)
            {
                channel = 1;
                readMode = ImreadModes.Grayscale;
            }
            else
            {
                channel = 3;
                readMode = ImreadModes.Color;
            }

            NDArray inputImageBatch = np.zeros((_batchSize, _imageHeight, _imageWidth, channel));
     
            int inputImageSize = _imageHeight * _imageHeight * channel;
            //int labelImageSize = _imageHeight * _imageHeight * 1;

            for (int index = 0; index < _batchSize; index++)
            {
                /// Source ND Image make
                int sampleIndex = index + _index;
                AnomalTrainSample sample = _sampleCollection[sampleIndex];
                string inputImagePath = sample.InputImagePath;
                Mat sourceImage = new Mat(inputImagePath, readMode);

                byte[] byteImage = new byte[inputImageSize];
                Marshal.Copy(sourceImage.Data, byteImage, 0, inputImageSize);
                NDArray sourceNDImage = np.array<byte>(byteImage, true);
                sourceNDImage = sourceNDImage.reshape((_imageHeight, _imageWidth, channel));
                inputImageBatch[index] = sourceNDImage;
                /// Source ND Image make
                /// 

            }


            return inputImageBatch;
        }


        public (NDArray, NDArray) LoadBatch(List<ClassTrainSample> _sampleCollection,
                                                                             int _index,
                                                                             int _batchSize,
                                                                             bool _isGray,
                                                                             int _imageWidth,
                                                                             int _imageHeight,
                                                                             int _labelOutput)
        {

            int channel = 1;
            ImreadModes readMode = ImreadModes.Grayscale;
            if (_isGray == true)
            {
                channel = 1;
                readMode = ImreadModes.Grayscale;
            }
            else
            {
                channel = 3;
                readMode = ImreadModes.Color;
            }

            NDArray inputImageBatch = np.zeros((_batchSize, _imageHeight, _imageWidth, channel));
            NDArray inputLabelBatch = np.zeros((_batchSize, _labelOutput));
            int inputImageSize = _imageHeight * _imageHeight * channel;
            //int labelImageSize = _imageHeight * _imageHeight * 1;

            for (int index = 0; index < _batchSize; index++)
            {
                /// Source ND Image make
                int sampleIndex = index + _index;
                ClassTrainSample sample = _sampleCollection[sampleIndex];
                string inputImagePath = sample.InputImagePath;
                Mat sourceImage = new Mat(inputImagePath, readMode);

                byte[] byteImage = new byte[inputImageSize];
                Marshal.Copy(sourceImage.Data, byteImage, 0, inputImageSize);
                NDArray sourceNDImage = np.array<byte>(byteImage, true);
                sourceNDImage = sourceNDImage.reshape((_imageHeight, _imageWidth, channel));
                inputImageBatch[index] = sourceNDImage;
                /// Source ND Image make
                /// 

                /// Label ND Image Make
                /// 
                //NDArray oneLabel = np.zeros((_imageHeight, _imageWidth, _labelOutput));
                //List<NDArray> labelList = new List<NDArray>();
                //for (int labelIndex = 0; labelIndex < _labelOutput; labelIndex++)
                //{
                //    string labelPath = sample.OutputLabelCollection[labelIndex];

                //    Mat labelImage = new Mat(labelPath, ImreadModes.Grayscale);
                //    Mat normalized = labelImage.Threshold(125, 255, ThresholdTypes.Binary);
                //    normalized = normalized / 255;
                //    byte[] byteLabelImage = new byte[labelImageSize];
                //    Marshal.Copy(normalized.Data, byteLabelImage, 0, labelImageSize);
                //    NDArray labelNDImage = np.array<byte>(byteLabelImage, true);
                //    labelNDImage = labelNDImage.reshape((_imageHeight, _imageWidth, 1));
                //    labelList.Add(labelNDImage);
                //    //oneLabel[labelIndex] = labelNDImage;
                //}
                //NDArray oneLabel = np.concatenate(labelList.ToArray(), 2);
                inputLabelBatch[index, sample.LabelIndex] = 1.0;
                /// Label ND Image Make
                /// 
            }


            return (inputImageBatch, inputLabelBatch);
        }

        public void DeleteUnzipFiles()
        {
            string[] files = Directory.GetFiles(configService.SegmentationTrainedModelUnzipPath);
            foreach (var file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                FileInfo fileInfo = new FileInfo(file);
                fileInfo.IsReadOnly = false;
                File.Delete(file);
            }
     
        }


        public OpenCvSharp.Mat NDArrayToMat(NDArray input, int channel, int imageHeight, int imageWidth)
        {

            MatType type = MatType.CV_8UC1;
            if (channel == 3)
                type = MatType.CV_8UC3;

            byte[] ndByte = input.astype(NumSharp.NPTypeCode.Byte).ToByteArray();
            var cvImage = new OpenCvSharp.Mat(imageHeight, imageWidth, type);
            int length = imageHeight * imageWidth * channel; // or src.Height * src.Step;
            Marshal.Copy(ndByte, 0, cvImage.Data, length);

            return cvImage;
        }

        public OpenCvSharp.Mat AlphaBlendColorMat(OpenCvSharp.Mat input, System.Windows.Media.Color _color)
        {
            Mat alphaMap = new Mat(new Size(input.Width, input.Height), MatType.CV_8UC4, new Scalar(0, 0, 0, 0));
            for (int i = 0; i < alphaMap.Rows; i++)
            {
                for (int j = 0; j < alphaMap.Cols; j++)
                {
                    var target = alphaMap.At<Vec4b>(i, j);
                    byte pixelValue = input.At<byte>(i, j);

                    if(pixelValue == 1)
                    {
                        target.Item0 = (byte)(_color.B);
                        target.Item1 = (byte)(_color.G);
                        target.Item2 = (byte)(_color.R);
                        target.Item3 = 255;
                        

                    }
                    else
                    {
                        target.Item0 = 0;
                        target.Item1 = 0;
                        target.Item2 = 0;
                        target.Item3 = 0;

                    }
                    alphaMap.Set<Vec4b>(i, j, target);
                }
            }

            return alphaMap;
        }

        public ObservableCollection<SegmentPreviewItem> ExtractSetmentImagesFromNDArray(SegmentLabelInfo segmentLabelInfo,
                                                                                        NDArray source,
                                                                                        NDArray output,
                                                                                        int imageWidth,
                                                                                        int imageHeight,
                                                                                        int imageChannel)
        {
            ObservableCollection<SegmentPreviewItem> _collection = new ObservableCollection<SegmentPreviewItem>();


            // 현재 이미지 출력 best 이미지 
            var bestSourceImage = this.NDArrayToMat(source["0,:,:,:"],
                      imageChannel,
                      imageWidth,
                      imageHeight);

            BitmapSource sourceImage = this.ConvertToBitmapSource(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(bestSourceImage));
            sourceImage.Freeze();
            _collection.Add(new SegmentPreviewItem()
            {
                Image = sourceImage,
                Alpha = 1,
            });

         
            for (int index = 0; index < segmentLabelInfo.Labels.Count; index++)
            {
                var label = segmentLabelInfo.Labels[index];
                var bestSourceMaskImage = this.NDArrayToMat(output[0, Slice.All, Slice.All, index],
                                                                                      1,
                                                                                      imageWidth,
                                                                                      imageHeight);
                bestSourceMaskImage = bestSourceMaskImage.Threshold(0.5, 1, ThresholdTypes.Binary);
                Mat alphaColor = this.AlphaBlendColorMat(bestSourceMaskImage, label.Color);
                BitmapSource maskImage = this.ConvertToBitmapSource(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(alphaColor));
                _collection.Add(new SegmentPreviewItem()
                {
                    Image = maskImage,
                    Alpha = 0.8

                });
            }

            return _collection;
        }


        public ClassPreviewItem ExtractClassImagesFromNDArray(ClassLabelInfo classLabelInfo,
                                                                                        NDArray source,
                                                                                        NDArray output,
                                                                                        int imageWidth,
                                                                                        int imageHeight,
                                                                                        int imageChannel)
        {
            //ObservableCollection<ClassPreviewItem> _collection = new ObservableCollection<ClassPreviewItem>();


            // 현재 이미지 출력 best 이미지 
            var bestSourceImage = this.NDArrayToMat(source["0,:,:,:"],
                      imageChannel,
                      imageWidth,
                      imageHeight);

            BitmapSource sourceImage = this.ConvertToBitmapSource(OpenCvSharp.Extensions.BitmapConverter.ToBitmap(bestSourceImage));
            sourceImage.Freeze();

            ClassPreviewItem previewItem = new ClassPreviewItem()
            {
                Image = sourceImage
            };

            double maxScore = 0;
            int maxIndex = 0;
            for (int index = 0; index < classLabelInfo.Labels.Count; index++)
            {
                var label = classLabelInfo.Labels[index];

                
                double score = double.Parse(output[0, index].ToString());
                if (maxScore < score)
                {
                    maxScore = score;
                    maxIndex = index;
                }
            }

            previewItem.Score = maxScore;
            previewItem.Name = classLabelInfo.Labels[maxIndex].Name;

            return previewItem;
        }

        public BitmapSource ConvertToBitmapSource(Bitmap bitmap)
        {
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
                                                                             IntPtr.Zero,
                                                                             Int32Rect.Empty,
                                                                             BitmapSizeOptions.FromEmptyOptions());

            return bitmapSource;
        }


        public bool UnZipDeepModel(string targetfilePath, string outputPath, string password)
        {

            // 모델 압축 해제 
            try
            {
                using (ZipArchive archive = ZipArchive.Read(targetfilePath))
                {
                    archive.Password = password;

                    foreach (ZipItem item in archive)
                    {
                        item.Extract(outputPath);
                    }
                }

            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }

        public bool ZipDeepModel(string targetFolderPath, string outputFilePath, string password)
        {

            try
            {
                string[] sourceFiles = Directory.GetFiles(targetFolderPath);
                EncryptionType encryptionType = EncryptionType.PkZip;
                using (ZipArchive archive = new ZipArchive())
                {
                    archive.Password = password;
                    foreach (string file in sourceFiles)
                    {
                        archive.AddFile(file, "/");
                    }
                    archive.Save(outputFilePath);
                }
            }
            catch(Exception e)
            {
                return false;
            }



            return true;
        }

        public SegmentTrainModelInfo LoadSegmentTrainModelInfo(string modelInfoFile)
        {
            SegmentTrainModelInfo info = new SegmentTrainModelInfo();


            // 압축 해제된 파일에서 모델 정보 불러오기
            try
            {
                using (StreamReader reader = new StreamReader(modelInfoFile, Encoding.UTF8))
                {
                    string labelContent = reader.ReadToEnd();
                    info = (SegmentTrainModelInfo)JsonConvert.DeserializeObject(labelContent, typeof(SegmentTrainModelInfo));
                }

            }
            catch (Exception e)
            {
                return null;
            }


            return info;
        }


        public ClassTrainModelInfo LoadClassTrainModelInfo(string modelInfoFile)
        {
            ClassTrainModelInfo info = new ClassTrainModelInfo();


            // 압축 해제된 파일에서 모델 정보 불러오기
            try
            {
                using (StreamReader reader = new StreamReader(modelInfoFile, Encoding.UTF8))
                {
                    string labelContent = reader.ReadToEnd();
                    info = (ClassTrainModelInfo)JsonConvert.DeserializeObject(labelContent, typeof(ClassTrainModelInfo));
                }

            }
            catch (Exception e)
            {
                return null;
            }


            return info;
        }

        public AnomalTrainModelInfo LoadAnomalTrainModelInfo(string modelInfoFile)
        {
            AnomalTrainModelInfo info = new AnomalTrainModelInfo();


            // 압축 해제된 파일에서 모델 정보 불러오기
            try
            {
                using (StreamReader reader = new StreamReader(modelInfoFile, Encoding.UTF8))
                {
                    string labelContent = reader.ReadToEnd();
                    info = (AnomalTrainModelInfo)JsonConvert.DeserializeObject(labelContent, typeof(AnomalTrainModelInfo));
                }

            }
            catch (Exception e)
            {
                return null;
            }


            return info;
        }

        public async Task<(ObservableCollection<SegmentTrainSample>, SegmentLabelInfo)> LoadSegmentSamplesAsync(string _folder)
        {

            ObservableCollection<SegmentTrainSample> segmenTrainSampleCollection = new ObservableCollection<SegmentTrainSample>();
            SegmentLabelInfo labelInfo = new SegmentLabelInfo();

            try
            {
                using (StreamReader reader = new StreamReader(_folder + Path.DirectorySeparatorChar + this.configService.LabelInfoFileName, Encoding.UTF8))
                {
                    string labelContent = reader.ReadToEnd();
                    labelInfo = (SegmentLabelInfo)JsonConvert.DeserializeObject(labelContent, typeof(SegmentLabelInfo));
                }
            }
            catch (Exception e)
            {

            }

            int imageWidth = labelInfo.ImageHeight;
            int imageHeight = labelInfo.ImageWidth;
            int labelSize = labelInfo.LabelSize;


            string[] directories = Directory.GetDirectories(_folder);

            try
            {
                await Task.Run(async () =>
                {
                    foreach (var directory in directories)
                    {
                        SegmentTrainSample info = new SegmentTrainSample()
                        {
                            ImageWidth = labelInfo.ImageWidth,
                            ImageHeight = labelInfo.ImageHeight,
                            IsGray = labelInfo.IsGray,
                            Name = directory
                        };

                        info.InputImagePath = directory + Path.DirectorySeparatorChar + "source.jpg";

                        for (int index = 0; index < labelSize; index++)
                        {
                            info.OutputLabelCollection.Add(directory + Path.DirectorySeparatorChar + index + ".jpg");
                        }

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            segmenTrainSampleCollection.Add(info);
                        });

                        await Task.Delay(10);
                    }
                });
            }
            catch(Exception e)
            {

            }
            

            return (segmenTrainSampleCollection, labelInfo);
        }


        public async Task<(ObservableCollection<ClassTrainSample>, ClassLabelInfo)> LoadClassSamplesAsync(string _folder)
        {

            ObservableCollection<ClassTrainSample> classTrainSampleCollection = new ObservableCollection<ClassTrainSample>();
            ClassLabelInfo labelInfo = new ClassLabelInfo();

            try
            {
                using (StreamReader reader = new StreamReader(_folder + Path.DirectorySeparatorChar + this.configService.LabelInfoFileName, Encoding.UTF8))
                {
                    string labelContent = reader.ReadToEnd();
                    labelInfo = (ClassLabelInfo)JsonConvert.DeserializeObject(labelContent, typeof(ClassLabelInfo));
                }
            }
            catch (Exception e)
            {

            }

            int imageWidth = labelInfo.ImageHeight;
            int imageHeight = labelInfo.ImageWidth;
            int labelSize = labelInfo.LabelSize;




            try
            {

                string[] directories = Directory.GetDirectories(_folder);
                int[] labelIndexes = new int[directories.Length];
                for (int index = 0; index < directories.Length; index++)
                {
                    labelIndexes[index] = int.Parse(Path.GetFileName(directories[index]));
                }

                await Task.Run(async () =>
                {
                    for (int dirIndex=0; dirIndex < directories.Length; dirIndex++)
                    {
                        var directory = directories[dirIndex];

                        string[] images = Directory.GetFiles(directory);
                        foreach(var image in images)
                        {
                            ClassTrainSample info = new ClassTrainSample()
                            {
                                ImageWidth = labelInfo.ImageWidth,
                                ImageHeight = labelInfo.ImageHeight,
                                IsGray = labelInfo.IsGray,
                                Name = directory,
                                LabelIndex = labelIndexes[dirIndex]
                            };

                            info.InputImagePath = image;

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                classTrainSampleCollection.Add(info);
                            });
                            await Task.Delay(10);
                        }
                    }
                });
            }
            catch (Exception e)
            {

            }


            return (classTrainSampleCollection, labelInfo);
        }





        public async Task<(ObservableCollection<AnomalTrainSample>, AnomalLabelInfo)> LoadAnomalSamplesAsync(string _folder)
        {

            ObservableCollection<AnomalTrainSample> classTrainSampleCollection = new ObservableCollection<AnomalTrainSample>();
            AnomalLabelInfo labelInfo = new AnomalLabelInfo();

            try
            {
                using (StreamReader reader = new StreamReader(_folder + Path.DirectorySeparatorChar + this.configService.LabelInfoFileName, Encoding.UTF8))
                {
                    string labelContent = reader.ReadToEnd();
                    labelInfo = (AnomalLabelInfo)JsonConvert.DeserializeObject(labelContent, typeof(AnomalLabelInfo));
                }
            }
            catch (Exception e)
            {

            }

            int imageWidth = labelInfo.ImageHeight;
            int imageHeight = labelInfo.ImageWidth;
      




            try
            {

                string[] files = Directory.GetFiles(_folder);

                await Task.Run(async () =>
                {
                    foreach (var file in files)
                    {

                        string fileExt = Path.GetExtension(file);
                        if (fileExt != ".jpg") continue; 

                        AnomalTrainSample info = new AnomalTrainSample()
                        {
                            ImageWidth = labelInfo.ImageWidth,
                            ImageHeight = labelInfo.ImageHeight,
                            IsGray = labelInfo.IsGray,
                            Name = Path.GetFileName(file),
                        };

                        info.InputImagePath = file;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            classTrainSampleCollection.Add(info);
                        });
                        await Task.Delay(10);
                    }
                });
            }
            catch (Exception e)
            {

            }


            return (classTrainSampleCollection, labelInfo);
        }
    }
}
