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
//using static Tensorflow.Binding;

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
       
        public void DeleteUnzipFiles()
        {
            string[] files = Directory.GetFiles(configService.SegmentationTrainedModelUnzipPath);
            foreach (var file in files)
                File.Delete(file);
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

        public BitmapSource ConvertToBitmapSource(Bitmap bitmap)
        {
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(),
                                                                             IntPtr.Zero,
                                                                             Int32Rect.Empty,
                                                                             BitmapSizeOptions.FromEmptyOptions());

            return bitmapSource;
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
    }
}
