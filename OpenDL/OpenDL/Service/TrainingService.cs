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

        public (NDArray, NDArray) LoadBatch(ObservableCollection<SegmentTrainSample> _sampleCollection, int _index, int _batchSize)
        {
            NDArray inputImageBatch;
            NDArray inputLabelBatch;

            return (null, null);
        }
       


        public async Task<ObservableCollection<SegmentTrainSample>> LoadSegmentSamplesAsync(string _folder)
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
            

            return segmenTrainSampleCollection;
        }
    }
}
