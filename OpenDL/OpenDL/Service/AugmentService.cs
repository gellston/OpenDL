using OpenDL.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using OpenCvSharp;
using System.Linq;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Windows;

using Size = OpenCvSharp.Size;
using Point = OpenCvSharp.Point;
using Rect = OpenCvSharp.Rect;

namespace OpenDL.Service
{
    public class AugmentService
    {
        private readonly ConfigureService configService;

        public AugmentService(ConfigureService _configService)
        {
            this.configService = _configService;
        }


        public async Task DoSegmentationAugmentationAsync(string _outputFolder, 
                                               ObservableCollection<SegmentLabelPolygon> _polygons,
                                               ObservableCollection<SegmentLabelUnit> _units,
                                               int _imageWidth,
                                               int _imageHeight,
                                               bool _isGray,
                                               Action<int, int> _progressCallback)
        {

            try
            {
                SegmentLabelInfo labelInfo = new SegmentLabelInfo()
                {
                    ImageHeight = _imageHeight,
                    ImageWidth = _imageWidth,
                    Labels = _polygons,
                    LabelSize = _polygons.Count,
                    IsGray = _isGray
                };

                string segmentLabelInfoJson = JsonConvert.SerializeObject(labelInfo, Formatting.Indented);
                using (StreamWriter sw = new StreamWriter(_outputFolder + Path.DirectorySeparatorChar + this.configService.LabelInfoFileName, false, Encoding.UTF8))
                {
                    sw.Write(segmentLabelInfoJson);
                }
            }
            catch (Exception e)
            {

            }

            try
            {
                await Task.Run(async () =>
                {
                    int labelCount = 0;
                    foreach (var unit in _units)
                    {
                        _progressCallback(labelCount, _units.Count);
                        labelCount++;
                        await Task.Delay(10);

                        if (unit.Polygons.Count <= 0) continue;

                        string sampleRootFolder = _outputFolder + Path.DirectorySeparatorChar + unit.FileName + "_" + DateTime.Now.ToString("yyyyMMddHHmmssFFF");
                        string sampleSourceImage = sampleRootFolder + Path.DirectorySeparatorChar + "source.jpg";
                        Directory.CreateDirectory(sampleRootFolder);

                        Mat sourceImage = null;
                        if (_isGray == true)
                            sourceImage = new Mat(unit.FilePath, ImreadModes.Grayscale);
                        else
                            sourceImage = new Mat(unit.FilePath, ImreadModes.Color);

                        Mat resizeImage = sourceImage.Resize(new Size(_imageWidth, _imageHeight));
                        resizeImage.SaveImage(sampleSourceImage);

                        int count = 0;
                        foreach (var polygon in _polygons)
                        {
                            Mat image = new Mat(new Size(unit.ImageWidth, unit.ImageHeight), MatType.CV_8UC1, new Scalar(0, 0, 0));
                            var shapes = unit.Polygons.Where(x => x.Name == polygon.Name);

                            foreach (var shape in shapes)
                            {
                                List<List<Point>> ListOfListOfPoint = new List<List<Point>>();
                                List<Point> points = new List<Point>();
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    foreach (var point in shape.Points)
                                    {
                                        points.Add(new Point()
                                        {
                                            X = (int)point.X,
                                            Y = (int)point.Y
                                        });
                                    }
                                });

                                ListOfListOfPoint.Add(points);
                                image.FillPoly(ListOfListOfPoint, new Scalar(255), LineTypes.AntiAlias);
                            }

                            Mat resizeLabel = image.Resize(new Size(_imageWidth, _imageHeight));
                            resizeLabel.ImWrite(sampleRootFolder + Path.DirectorySeparatorChar + count + ".jpg");
                            count++;
                        }
                    }
                });
            }
            catch(Exception e)
            {

            }
        }




        public async Task DoClassificationAugmentationAsync(string _outputFolder,
                                                           ObservableCollection<ClassificationLabelBox> _boxes,
                                                           ObservableCollection<ClassLabelUnit> _units,
                                                           int _imageWidth,
                                                           int _imageHeight,
                                                           bool _isGray,
                                                           Action<int, int> _progressCallback)
        {

            try
            {
                ClassLabelInfo labelInfo = new ClassLabelInfo()
                {
                    ImageHeight = _imageHeight,
                    ImageWidth = _imageWidth,
                    Labels = _boxes,
                    LabelSize = _boxes.Count,
                    IsGray = _isGray
                };

                string classLabelInfoJson = JsonConvert.SerializeObject(labelInfo, Formatting.Indented);
                using (StreamWriter sw = new StreamWriter(_outputFolder + Path.DirectorySeparatorChar + this.configService.LabelInfoFileName, false, Encoding.UTF8))
                {
                    sw.Write(classLabelInfoJson);
                }


                for(int index=0; index<_boxes.Count; index++)
                {
                    Directory.CreateDirectory(_outputFolder + Path.DirectorySeparatorChar + index.ToString());
                }
            }
            catch (Exception e)
            {

            }

            try
            {
                await Task.Run(async () =>
                {
                    int labelCount = 0;
                    foreach (var unit in _units)
                    {
                        _progressCallback(labelCount, _units.Count);
                        labelCount++;
                        await Task.Delay(10);

                        if (unit.Boxes.Count <= 0) continue;

           
                        Mat sourceImage = null;
                        if (_isGray == true)
                            sourceImage = new Mat(unit.FilePath, ImreadModes.Grayscale);
                        else
                            sourceImage = new Mat(unit.FilePath, ImreadModes.Color);

                        System.Console.WriteLine(unit.FilePath);
                        foreach(var box in unit.Boxes)
                        {
                            if (box.X < 0 || box.Y < 0) continue;
                            if (box.X + box.Width >= sourceImage.Width) continue;
                            if (box.Y + box.Height >= sourceImage.Height) continue;
                            if (box.Width < 0 | box.Height < 0) continue;

                            var labelObject = _boxes.Where(x => x.Name == box.Name).FirstOrDefault();
                            int index = _boxes.IndexOf(labelObject);
                            string fileName = _outputFolder + Path.DirectorySeparatorChar + index.ToString() + Path.DirectorySeparatorChar + DateTime.Now.ToString("yyyyMMddHHmmssFFF") + ".jpg";

                            var boxImage = new Mat(sourceImage, new Rect((int)box.X, (int)box.Y, (int)box.Width, (int)box.Height));
                            Mat resizeLabel = boxImage.Resize(new Size(_imageWidth, _imageHeight));
                            resizeLabel.SaveImage(fileName);
                            await Task.Delay(1);
                        }
                    }
                });
            }
            catch (Exception e)
            {

            }
        }



        public async Task DoAnomalyAugmentationAsync(string _outputFolder,
                                                           ObservableCollection<string> _files,
                                                           int _imageWidth,
                                                           int _imageHeight,
                                                           bool _isGray,
                                                           int _patchWidth,
                                                           int _patchHeight,
                                                           int _repeatCount,
                                                           Action<int, int> _progressCallback)
        {

            try
            {
                AnomalLabelInfo labelInfo = new AnomalLabelInfo()
                {
                    ImageHeight = _patchHeight,
                    ImageWidth = _patchWidth,
                    IsGray = _isGray
                };

                string anoLabelInfoJson = JsonConvert.SerializeObject(labelInfo, Formatting.Indented);
                using (StreamWriter sw = new StreamWriter(_outputFolder + Path.DirectorySeparatorChar + this.configService.LabelInfoFileName, false, Encoding.UTF8))
                {
                    sw.Write(anoLabelInfoJson);
                }

            }
            catch (Exception e)
            {

            }

            try
            {
                await Task.Run(async () =>
                {
                    int labelCount = 0;
                    foreach (var file in _files)
                    {
                        _progressCallback(labelCount, _files.Count);
                        labelCount++;
                        await Task.Delay(10);

                        Mat sourceImage = null;
                        if (_isGray == true)
                            sourceImage = new Mat(file, ImreadModes.Grayscale);
                        else
                            sourceImage = new Mat(file, ImreadModes.Color);

                        for(int index =0; index < _repeatCount; index++)
                        {
                            string fileName = _outputFolder + Path.DirectorySeparatorChar + index.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmssFFF") + ".jpg";

                            Random random = new Random();
                            int startX = random.Next(0, _imageWidth - _patchWidth);
                            int startY = random.Next(0, _imageHeight - _patchHeight);

                            Mat resizeLabel = sourceImage.Resize(new Size(_imageWidth, _imageHeight));
                            var boxImage = new Mat(resizeLabel, new Rect(startX, startY, _patchWidth, _patchHeight));

                            boxImage.SaveImage(fileName);
                        }
                    }
                });
            }
            catch (Exception e)
            {

            }
        }
    }
}
