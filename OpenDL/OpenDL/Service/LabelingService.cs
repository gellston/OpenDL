using DevExpress.Xpf.Core;
using Newtonsoft.Json;
using OpenDL.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace OpenDL.Service
{
    public class LabelingService
    {
        private readonly ConfigureService configService;
        public LabelingService(ConfigureService _configService)
        {
            this.configService = _configService;

        }


        public async Task<(ObservableCollection<ClassificationLabelBox>, ObservableCollection<ClassLabelUnit>)> LoadClassLabelAsync(string _root, string[] _files)
        {

            ObservableCollection<ClassificationLabelBox> boxCollection = new ObservableCollection<ClassificationLabelBox>();
            ObservableCollection<ClassLabelUnit> labelCollection = new ObservableCollection<ClassLabelUnit>();

            try
            {
                using (StreamReader reader = new StreamReader(_root + Path.DirectorySeparatorChar + this.configService.LabelInfoFileName, Encoding.UTF8))
                {
                    string labelContent = reader.ReadToEnd();
                    boxCollection = (ObservableCollection<ClassificationLabelBox>)JsonConvert.DeserializeObject(labelContent, typeof(ObservableCollection<ClassificationLabelBox>));
                }

            }
            catch (Exception e)
            {
                //System.Console.WriteLine(e.ToString());
            }

            try
            {
                await Task.Run(() =>
                {

                    foreach (var file in _files)
                    {
                        ClassLabelUnit unit = null;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            unit = new ClassLabelUnit();
                            labelCollection.Add(unit);
                        });

                        unit.FilePath = file;
                        string filePath = Path.GetDirectoryName(file);
                        string pureFileName = Path.GetFileNameWithoutExtension(file);
                        string fileNameWithExtension = Path.GetFileName(file);
                        unit.FileName = pureFileName;
                        unit.FileNameWithExtension = fileNameWithExtension;


                        OpenCvSharp.Mat image = new OpenCvSharp.Mat(unit.FilePath);
                        unit.ImageWidth = image.Width;
                        unit.ImageHeight = image.Height;

                        string jsonFileName = filePath + Path.DirectorySeparatorChar + pureFileName + ".json";
                        if (File.Exists(jsonFileName) != true)
                            continue;
                        using (StreamReader reader = new StreamReader(jsonFileName, Encoding.UTF8))
                        {
                            string labelContent = reader.ReadToEnd();

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                unit.Boxes = (ObservableCollection<ClassificationLabelBox>)JsonConvert.DeserializeObject(labelContent, typeof(ObservableCollection<ClassificationLabelBox>));
                            });
                        }
                    }
                }).ConfigureAwait(false);
            }
            catch (Exception e)
            {

            }

            return (boxCollection, labelCollection);
        }






        public async Task<ObservableCollection<string>> LoadAnomalLabelAsync(string _root, string[] _files)
        {

            ObservableCollection<string> fileLabel = new ObservableCollection<string>();
            //ObservableCollection<ClassLabelUnit> labelCollection = new ObservableCollection<ClassLabelUnit>();


            try
            {
                await Task.Run(() =>
                {

                    foreach (var file in _files)
                    {
                        //ClassLabelUnit unit = null;

                        //Application.Current.Dispatcher.Invoke(() =>
                        //{
                        //    unit = new ClassLabelUnit();
                        //    labelCollection.Add(unit);
                        //});


                        string filePath = Path.GetDirectoryName(file);
                        string pureFileName = Path.GetFileNameWithoutExtension(file);
                        string fileNameWithExtension = Path.GetFileName(file);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            fileLabel.Add(file);
                        });
                    }
                }).ConfigureAwait(false);
            }
            catch (Exception e)
            {

            }

            return fileLabel;
        }


        public async Task<(ObservableCollection<SegmentLabelPolygon>, ObservableCollection<SegmentLabelUnit>)> LoadSegmentedLabelAsync(string _root, string [] _files)
        {

            ObservableCollection<SegmentLabelPolygon> polygonCollection = new ObservableCollection<SegmentLabelPolygon>();
            ObservableCollection<SegmentLabelUnit> labelCollection = new ObservableCollection<SegmentLabelUnit>();

            try
            {
                using (StreamReader reader = new StreamReader(_root + Path.DirectorySeparatorChar + this.configService.LabelInfoFileName, Encoding.UTF8))
                {
                    string labelContent = reader.ReadToEnd();
                    polygonCollection = (ObservableCollection<SegmentLabelPolygon>)JsonConvert.DeserializeObject(labelContent, typeof(ObservableCollection<SegmentLabelPolygon>));
                }

            }
            catch(Exception e)
            {
                //System.Console.WriteLine(e.ToString());
            }

            try
            {
                await Task.Run(() =>
                {

                    foreach (var file in _files)
                    {
                        SegmentLabelUnit unit = null;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            unit = new SegmentLabelUnit();
                            labelCollection.Add(unit);
                        });

                        unit.FilePath = file;
                        string filePath = Path.GetDirectoryName(file);
                        string pureFileName = Path.GetFileNameWithoutExtension(file);
                        string fileNameWithExtension = Path.GetFileName(file);
                        unit.FileName = pureFileName;
                        unit.FileNameWithExtension = fileNameWithExtension;


                        OpenCvSharp.Mat image = new OpenCvSharp.Mat(unit.FilePath);
                        unit.ImageWidth = image.Width;
                        unit.ImageHeight = image.Height;

                        string jsonFileName = filePath + Path.DirectorySeparatorChar + pureFileName + ".json";
                        if (File.Exists(jsonFileName) != true)
                            continue;
                        using (StreamReader reader = new StreamReader(jsonFileName, Encoding.UTF8))
                        {
                            string labelContent = reader.ReadToEnd();

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                unit.Polygons = (ObservableCollection<SegmentLabelPolygon>)JsonConvert.DeserializeObject(labelContent, typeof(ObservableCollection<SegmentLabelPolygon>));
                            });
                        }
                    }
                }).ConfigureAwait(false);
            }
            catch(Exception e)
            {

            }

            return (polygonCollection, labelCollection);
        }


        public async Task<bool> SaveSegmentLabelInformationAsync(string _folder, ObservableCollection<SegmentLabelPolygon> _labelInfo, ObservableCollection<SegmentLabelUnit> _unitInfo)
        {

            await Task.Run(async () =>
            {
                string segmentLabelInfoJson = "";

                Application.Current.Dispatcher.Invoke(() =>
                {
                    segmentLabelInfoJson = JsonConvert.SerializeObject(_labelInfo, Formatting.Indented);
                });

                using (StreamWriter sw = new StreamWriter(_folder + Path.DirectorySeparatorChar + this.configService.LabelInfoFileName, false, Encoding.UTF8))
                {
                    sw.Write(segmentLabelInfoJson);
                }

                foreach (var info in _unitInfo)
                {
                    string filePath = Path.GetDirectoryName(info.FilePath);
                    string jsonFileName = filePath + Path.DirectorySeparatorChar + info.FileName + ".json";
                    string segmentUnitInfoJson = "";
                    
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        segmentUnitInfoJson = JsonConvert.SerializeObject(info.Polygons, Formatting.Indented);
                    });
                    using (StreamWriter sw = new StreamWriter(jsonFileName, false, Encoding.UTF8))
                    {
                        sw.Write(segmentUnitInfoJson);
                    }
                }
            }).ConfigureAwait(false);

            return true;
        }




        public async Task<bool> SaveClassLabelInformationAsync(string _folder, ObservableCollection<ClassificationLabelBox> _labelInfo, ObservableCollection<ClassLabelUnit> _unitInfo)
        {

            await Task.Run(async () =>
            {
                string classLabelInfoJson = "";

                Application.Current.Dispatcher.Invoke(() =>
                {
                    classLabelInfoJson = JsonConvert.SerializeObject(_labelInfo, Formatting.Indented);
                });

                using (StreamWriter sw = new StreamWriter(_folder + Path.DirectorySeparatorChar + this.configService.LabelInfoFileName, false, Encoding.UTF8))
                {
                    sw.Write(classLabelInfoJson);
                }

                foreach (var info in _unitInfo)
                {
                    string filePath = Path.GetDirectoryName(info.FilePath);
                    string jsonFileName = filePath + Path.DirectorySeparatorChar + info.FileName + ".json";
                    string classUnitInfoJson = "";

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        classUnitInfoJson = JsonConvert.SerializeObject(info.Boxes, Formatting.Indented);
                    });
                    using (StreamWriter sw = new StreamWriter(jsonFileName, false, Encoding.UTF8))
                    {
                        sw.Write(classUnitInfoJson);
                    }
                }
            }).ConfigureAwait(false);

            return true;
        }



    }
}
