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
    public class LabelLoaderService
    {
        private readonly ConfigureService configService;
        public LabelLoaderService(ConfigureService _configService)
        {
            this.configService = _configService;

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
                    unit.FileName = pureFileName;
       
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

            return (polygonCollection, labelCollection);
        }


        public async Task<bool> SaveLabelInformationAsync(string _folder, ObservableCollection<SegmentLabelPolygon> _labelInfo, ObservableCollection<SegmentLabelUnit> _unitInfo)
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
    }
}
