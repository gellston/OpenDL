using OpenDL.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using OpenCvSharp;
using System.Linq;

namespace OpenDL.Service
{
    public class SegmentationService
    {

        public void DoSegmentationAugmentation(string _outputFolder, 
                                               ObservableCollection<SegmentLabelPolygon> _polygons,
                                               ObservableCollection<SegmentLabelUnit> _units,
                                               int _imageWidth,
                                               int _imageHeight)
        {
            foreach(var unit in _units)
            {
                if (unit.Polygons.Count <= 0) continue;

                string sampleRootFolder = _outputFolder + Path.DirectorySeparatorChar + unit.FileName;
                Directory.CreateDirectory(sampleRootFolder);

                int count = 0;
                foreach (var polygon in _polygons)
                {
                    Mat image = new Mat(new Size(unit.ImageWidth, unit.ImageHeight), MatType.CV_8UC1, new Scalar(0, 0, 0));
                    List<List<Point>> ListOfListOfPoint = new List<List<Point>>();
                    var shapes = unit.Polygons.Where(x => x.Name == polygon.Name);
                    foreach (var shape in shapes)
                    {
                        List<Point> points = new List<Point>();
                        foreach(var point in shape.Points)
                        {
                            points.Add(new Point()
                            {
                                X = (int)point.X,
                                Y = (int)point.Y
                            });
                        }
                        ListOfListOfPoint.Add(points);
                    }
                    image.FillPoly(ListOfListOfPoint, new Scalar(255));
                    image.ImWrite(sampleRootFolder + Path.DirectorySeparatorChar + count + ".jpg");
                    count++;
                }
            }
        }
    }
}
